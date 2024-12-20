using Agenciapp.Common.Models;
using Agenciapp.Common.Services;
using Agenciapp.Domain.Enums;
using Agenciapp.Domain.Models;
using Agenciapp.Service.IPromoCodeServices.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;


namespace Agenciapp.Service.IPromoCodeServices
{
    public interface IPromoCodeService
    {
        Task<Result<PromoCode>> Create(CreateCodeModel model);
        Task<Result<PromoCode>> Edit(EditCodeModel model);
        Task<Result<List<PromoCode>>> GetAssets(OrderType orderType);
        Task<Result<List<PromoCode>>> GetAllCodes();
        Task<Result<PromoCode>> GetAvailableCode(string code, Guid agencyId);
        Task<Result<PromoCode>> GetCode(Guid id);
        Task<Result> ChangeStatus(Guid Id, bool status);
        Result<IQueryable<PromoCode>> List();
    }

    public class PromoCodeService: IPromoCodeService
    {
        private readonly databaseContext _context;
        private readonly IUserResolverService _userService;
        public PromoCodeService(databaseContext context, IUserResolverService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<Result> ChangeStatus(Guid id, bool status)
        {
            var user = _userService.GetUser();
            if (user == null) return Result.Failure("Debe estar authenticado");

            var code = await _context.PromoCodes.FirstOrDefaultAsync(x => x.Id == id && x.Agency.AgencyId == user.AgencyId);
            if (code == null) return Result.Failure("El código no existe");

            code.ChangeStatus(status);
            _context.Attach(code);
            await _context.SaveChangesAsync();
            return Result.Success();

        }

        public async Task<Result<PromoCode>> Create(CreateCodeModel model)
        {
            var user = _userService.GetUser();
            if (user == null) return Result.Failure<PromoCode>("Debe estar authenticado");
            var agency = await _context.Agency.FindAsync(user.AgencyId);
            var code = new PromoCode(model.Code, model.Value, model.DateInit, model.DateEnd, model.OrderType, model.PromoType, agency, user);

            _context.Attach(code);
            await _context.SaveChangesAsync();

            return Result.Success(code);
        }

        public async Task<Result<PromoCode>> Edit(EditCodeModel model)
        {
            var user = _userService.GetUser();
            if (user == null) return Result.Failure<PromoCode>("Debe estar authenticado");

            var code = await _context.PromoCodes.FirstOrDefaultAsync(x => x.Id == model.Id && x.Agency.AgencyId == user.AgencyId);
            if (code == null) return Result.Failure<PromoCode>("El código no existe");

            code.Update(model.Code, model.Value, model.DateInit, model.DateEnd, model.OrderType, model.PromoType);
            _context.Attach(code);
            await _context.SaveChangesAsync();

            return Result.Success(code);
        }

        public async Task<Result<List<PromoCode>>> GetAllCodes()
        {
            var user = _userService.GetUser();
            if (user == null) return Result.Failure<List<PromoCode>>("Debe estar authenticado");

            return Result.Success( await _context.PromoCodes
                .Include(x => x.CreatedBy)
               .Where(x => x.Agency.AgencyId == user.AgencyId)
               .ToListAsync());
        }

        public async Task<Result<List<PromoCode>>> GetAssets(OrderType orderType)
        {
            var user = _userService.GetUser();
            if (user == null) return Result.Failure<List<PromoCode>>("Debe estar authenticado");
            DateTime date = DateTime.Now;
            return Result.Success( await _context.PromoCodes
                .Where(x => x.Agency.AgencyId == user.AgencyId && x.OrderType == orderType && x.isActive && x.DateInit <= date && x.DateEnd > date)
                .ToListAsync());
        }

        public async Task<Result<PromoCode>> GetAvailableCode(string codeNumber, Guid agencyId)
        {
            DateTime date = DateTime.Now;
            var code = await _context.PromoCodes
                .Include(x => x.CreatedBy)
                .FirstOrDefaultAsync(x => x.Agency.AgencyId == agencyId && x.Code.Equals(codeNumber) && x.isActive && x.DateInit <= date && x.DateEnd > date);
            if (code == null)
            {
                return Result.Failure<PromoCode>("El código no está disponible");
            }
            return Result.Success(code);

        }

        public async Task<Result<PromoCode>> GetCode(Guid id)
        {
            var user = _userService.GetUser();
            if (user == null) return Result.Failure<PromoCode>("Debe estar authenticado");

            var code = await _context.PromoCodes.FirstOrDefaultAsync(x => x.Id == id && x.Agency.AgencyId == user.AgencyId);
            if (code == null) return Result.Failure<PromoCode>("El código no existe");

            return Result.Success(code);

        }

        public Result<IQueryable<PromoCode>> List()
        {
            var user = _userService.GetUser();
            if (user == null) return Result.Failure<IQueryable<PromoCode>>("Debe estar authenticado");

            var codes = _context.PromoCodes.AsNoTracking().Where(x => x.Agency.AgencyId == user.AgencyId);
            return Result.Success(codes);
        }
    }
}
