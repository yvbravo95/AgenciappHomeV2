using Agenciapp.Service.IContactServices.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agenciapp.Service.IContactServices
{
    public interface IContactService
    {
        Task<Contact> Exist(CreateContactModel model, User user);
        Task<Result<Contact>> Create(CreateContactModel model, User user);
        Task<Contact> Edit(Guid id, EditContactModel model);
        Task<Contact> Get(Guid id);
    }

    public class ContactService : IContactService
    {
        private readonly databaseContext _context;
        public ContactService(databaseContext context)
        {
            _context = context;
        }

        public async Task<Result<Contact>> Create(CreateContactModel model, User user)
        {
            Contact contact = await Exist(model, user);
            if (contact == null)
            {
                int cantContacts = _context.Contact.Count() + 1;
                string initialName = model.Name.ElementAt(0) + "".ToUpper();
                string initialLastName = model.LastName.ElementAt(0) + "".ToUpper();
                var phone1 = new Phone
                {
                    PhoneId = Guid.NewGuid(),
                    Current = true,
                    Number = model.PhoneNumberMovil,
                    Type = "Móvil"
                };
                _context.Add(phone1);

                Phone phone2 = null;
                if (!string.IsNullOrEmpty(model.PhoneNumberCasa))
                {
                    phone2 = new Phone
                    {
                        PhoneId = Guid.NewGuid(),
                        Current = true,
                        Number = model.PhoneNumberCasa,
                        Type = "Casa"
                    };
                    _context.Add(phone2);
                }

                contact = new Contact
                {
                    ContactId = Guid.NewGuid(),
                    CI = model.CI,
                    CreatedAt = DateTime.Now,
                    ContactNumber = "CL" + cantContacts + initialName + initialLastName + DateTime.Now.ToString("mmss"),
                    Name = model.Name,
                    LastName = model.LastName,
                    Address = new Address
                    {
                        AddressId = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        AddressLine1 = model.Address.AddressLine1,
                        AddressLine2 = model.Address.AddressLine2,
                        City = model.Address.Province,
                        Country = "Cuba",
                        countryiso2 = "CU",
                        CreatedBy = user.UserId,
                        Current = true,
                        State = model.Address.Municipality,
                        Type = "Casa",
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = user.UserId,
                        Zip = model.Address.Reparto
                    },
                    Phone1 = phone1,
                    Phone2 = phone2,
                };
                _context.Add(contact);
                contact.Address.ReferenceId = contact.ContactId;
                contact.Phone1.ReferenceId = contact.ContactId;
                if (contact.Phone2 != null)
                    contact.Phone2.ReferenceId = contact.ContactId;

                _context.AgencyContact.Add(new AgencyContact
                {
                    AgencyContactId = Guid.NewGuid(),
                    AgencyId = user.AgencyId,
                    ContactId = contact.ContactId,
                });

                await _context.SaveChangesAsync();
            }

            return Result.Success(contact);
        }

        public async Task<Contact> Edit(Guid id, EditContactModel model)
        {
            var contact = await _context.Contact
                .Include(x => x.Phone1)
                .Include(x => x.Phone2)
                .Include(x => x.Address)
                .FirstOrDefaultAsync(x => x.ContactId == id) ?? throw new Exception("Contacto no encontrado");

            contact.Name = model.Name;
            contact.LastName = model.LastName;
            contact.CI = model.CI;
            contact.Phone1.Number = model.PhoneNumberMovil;
            if(model.PhoneNumberCasa != null)
            {
                if(contact.Phone2 != null) contact.Phone2.Number = model.PhoneNumberCasa;
                else
                {
                    var phone2 = new Phone
                    {
                        PhoneId = Guid.NewGuid(),
                        Current = true,
                        Number = model.PhoneNumberCasa,
                        Type = "Casa"
                    };
                    _context.Add(phone2);
                    contact.Phone2 = phone2;
                }
            }
            contact.Address.AddressLine1 = model.AddressLine1;
            contact.Address.AddressLine2 = model.AddressLine2;
            contact.Address.City = model.Province;
            contact.Address.State = model.Municipality;
            contact.Address.Zip = model.Reparto;
            contact.Address.UpdatedAt = DateTime.Now;
            contact.Address.UpdatedBy = contact.ContactId;

            await _context.SaveChangesAsync();

            return contact;
        }

        public async Task<Contact> Exist(CreateContactModel model, User user)
        {
            var contact = await
                _context.AgencyContact.Where(c => c.AgencyId == user.AgencyId).Join(
                    _context.Contact.Include(x => x.Phone1).Include(x => x.Phone2).Include(x => x.Address),
                    cc => cc.ContactId,
                    c => c.ContactId,
                    (cc, c) => c
                )
                .FirstOrDefaultAsync(x =>
                x.Name.ToLower() == (model.Name ?? string.Empty).ToLower() &&
                x.LastName.ToLower() == (model.LastName ?? string.Empty).ToLower() &&
                (x.Phone1.Number == model.PhoneNumberMovil || x.Phone2.Number == model.PhoneNumberCasa));

            return contact;
        }

        public Task<Contact> Get(Guid id)
        {
            return _context.Contact
                .Include(x => x.Phone1)
                .Include(x => x.Phone2)
                .Include(x => x.Address)
                .FirstOrDefaultAsync(x => x.ContactId == id);
        }
    }
}
