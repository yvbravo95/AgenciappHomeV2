using Agenciapp.Common.Services;
using Agenciapp.Domain.Models;
using Agenciapp.Domain.Models.ValueObject;
using Agenciapp.Service.IClientServices.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Agenciapp.Service.IClientServices
{
    public interface IClientService
    {
        Task<Client> Exist(CreateClientModel model, Guid agencyId);
        Task<Result<Client>> Create(CreateClientModel model, User user);
        Task<Result<Note>> AddNote(AddNoteModel model);
        Task<byte[]> ExportExcel(Guid agencyId);
        Task<Result<Client>> SetPassport(ClientPassport passport, Guid clientId);
        Task<Result<Client>> SetDocument(ClientDocument document, Guid clientId);
        Task<Client> Get(Guid id);
    }

    public class ClientService : IClientService
    {
        private readonly databaseContext _context;
        private readonly IUserResolverService _userService;
        public ClientService(databaseContext context, IUserResolverService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<Result<Note>> AddNote(AddNoteModel model)
        {
            var user = _userService.GetUser();
            if (user == null) return Result.Failure<Note>("Debe estar autenticado");

            var client = await _context.Client.FindAsync(model.ClientId);
            if (client == null) return Result.Failure<Note>("El cliente no existe");

            var note = new Note(user, client, model.Note);
            _context.Attach(note);
            await _context.SaveChangesAsync();

            return Result.Success(note);
        }

        public async Task<Result<Client>> SetPassport(ClientPassport passport, Guid clientId)
        {
            var client = await _context.Client.FindAsync(clientId);
            if (client == null) return Result.Failure<Client>("El cliente no existe");
            client.Passport = passport;

            _context.Attach(client);
            await _context.SaveChangesAsync();
            return Result.Success(client);
        }

        public async Task<Result<Client>> SetDocument(ClientDocument document, Guid clientId)
        {
            var client = await _context.Client.FindAsync(clientId);
            if (client == null) return Result.Failure<Client>("El cliente no existe");
            client.OtherDocument = document;

            _context.Attach(client);
            await _context.SaveChangesAsync();
            return Result.Success(client);
        }

        public async Task<Result<Client>> Create(CreateClientModel model, User user)
        {
            Client client = await Exist(model, user.AgencyId);
            if (client != null) return Result.Failure<Client>("El cliente ya existe.");
            var agency = await _context.Agency.FindAsync(user.AgencyId);

            int cantClients = _context.Client.Count() + 1;
            string initialName = model.Name.ElementAt(0) + "".ToUpper();
            string initialLastName = string.IsNullOrEmpty(model.LastName) ? string.Empty : model.LastName.ElementAt(0) + "".ToUpper();
            var phoneResult = getValidPhoneNumber(model.PhoneNumber);
            if(phoneResult.IsFailure)
                return Result.Failure<Client>(phoneResult.Error);

            client = new Client()
            {
                ClientId = Guid.NewGuid(),
                Agency = agency,
                checkNotifications = model.EnableNotifications,
                ClientNumber = "CL" + cantClients + initialName + initialLastName + DateTime.Now.ToString("mmss"),
                Address = new Address
                {
                    AddressId = Guid.NewGuid(),
                    AddressLine1 = model.Address?.AddressLine1 ?? "",
                    AddressLine2 = model.Address?.AddressLine2 ?? "",
                    City = model.Address?.City ?? "",
                    Country = model.Address?.Country ?? "",
                    countryiso2 = model.Address?.Countryiso2 ?? "",
                    CreatedAt = DateTime.Now,
                    CreatedBy = user.UserId,
                    Current = true,
                    State = model.Address?.State ?? "",
                    Type = "Casa",
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = user.UserId,
                    Zip = model.Address?.Zip
                },
                Conflictivo = model.Conflictivo,
                CreatedAt = DateTime.Now,
                Email = model.Email,
                FechaNac = model.BirthDate ?? DateTime.MinValue,
                ID = model.ID,
                LastName = model.LastName,
                LastName2 = model.LastName2,
                Name = model.Name,
                Name2 = model.Name2,
                IsCarrier = model.IsCarrier,
                Phone = new Phone
                {
                    PhoneId = Guid.NewGuid(),
                    Current = true,
                    Number = phoneResult.Value,
                    Type = "Móvil"
                }
            };

            _context.Add(client);
            client.Address.ReferenceId = client.ClientId;
            client.Phone.ReferenceId = client.ClientId;
            await _context.SaveChangesAsync();
            return Result.Success(client);
        }

        public async Task<Client> Exist(CreateClientModel model, Guid agencyId)
        {
            var phone = getValidPhoneNumber(model.PhoneNumber);
            if (phone.IsFailure)
                return null;
            string name = model.Name.Trim().ToLower();
            string lastname = model.LastName.Trim().ToLower();
            Client exist = await _context.Client.Include(x => x.Phone)
                .FirstOrDefaultAsync(x => x.AgencyId == agencyId &&
            x.Phone.Number.Equals(phone.Value)
            && x.Name.Trim().ToLower() == name && x.LastName.Trim().ToLower() == lastname
            );
            return exist;
        }

        public async Task<byte[]> ExportExcel(Guid agencyId)
        {
            byte[] fileContents;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Clientes");

                var clients = await _context.Client
                    .Include(x => x.Phone)
                    .Include(x => x.Address)
                    .Where(x => x.AgencyId == agencyId).ToListAsync();

                int col = 1;
                int row = 1;
                worksheet.Cells[row, col].Value = "Primer Nombre";
                worksheet.Cells[row, col].Style.Font.Size = 12;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                col++;
                worksheet.Cells[row, col].Value = "Segundo Nombre";
                worksheet.Cells[row, col].Style.Font.Size = 12;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                col++;
                worksheet.Cells[row, col].Value = "Primer Apellido";
                worksheet.Cells[row, col].Style.Font.Size = 12;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                col++;
                worksheet.Cells[row, col].Value = "Segundo Apellido";
                worksheet.Cells[row, col].Style.Font.Size = 12;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                col++;
                worksheet.Cells[row, col].Value = "Telefono";
                worksheet.Cells[row, col].Style.Font.Size = 12;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                col++;
                worksheet.Cells[row, col].Value = "Direccion";
                worksheet.Cells[row, col].Style.Font.Size = 12;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                col++;
                worksheet.Cells[row, col].Value = "Estado";
                worksheet.Cells[row, col].Style.Font.Size = 12;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                col++;
                worksheet.Cells[row, col].Value = "Ciudad";
                worksheet.Cells[row, col].Style.Font.Size = 12;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;
                col++;
                worksheet.Cells[row, col].Value = "Zip";
                worksheet.Cells[row, col].Style.Font.Size = 12;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Border.Top.Style = ExcelBorderStyle.Hair;

                foreach (var item in clients)
                {
                    row++;
                    col = 1;
                    worksheet.Cells[row, col++].Value = item.Name;
                    worksheet.Cells[row, col++].Value = item.Name2;
                    worksheet.Cells[row, col++].Value = item.LastName;
                    worksheet.Cells[row, col++].Value = item.LastName2;
                    worksheet.Cells[row, col++].Value = item.Phone?.Number;
                    worksheet.Cells[row, col++].Value = item.Address?.AddressLine1;
                    worksheet.Cells[row, col++].Value = item.Address?.State;
                    worksheet.Cells[row, col++].Value = item.Address?.City;
                    worksheet.Cells[row, col++].Value = item.Address?.Zip;
                }

                fileContents = package.GetAsByteArray();
            }

            return fileContents;
        }

        private Result<string> getValidPhoneNumber(string phoneNumber)
        {
            phoneNumber = Common.Class.Extensions.GetOnlyNumbers(phoneNumber);
            if (phoneNumber.Length == 10)
                return Result.Success(phoneNumber);
            else if (phoneNumber.Length == 11 && phoneNumber[0] == '1')
                return Result.Success(phoneNumber.Substring(1));
            else
               return Result.Failure<string>("The number of digits of the phone number is not valid");
        }

        public Task<Client> Get(Guid id)
        {
            return _context.Client.Include(x => x.Phone).Include(x => x.Address).FirstOrDefaultAsync(x => x.ClientId == id);
        }
    }
}
