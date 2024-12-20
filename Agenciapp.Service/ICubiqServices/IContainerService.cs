using Agenciapp.Domain.Models;
using Agenciapp.Service.ICubiqServices.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agenciapp.Service.ICubiqServices
{
    public interface IContainerService
    {
        Task<Result<Pallet>> Create(Agency agency, CreatePalletModel model);
        Task<Result<Pallet>> AddPackage(AddPackageModel model, Guid userId);
        Task<Pallet> GetPallet(Guid id);
        Task<Result> DeletePackageOfPallet(Guid packageId);
        Task<Result> DeletePallet(Guid id);
        Task<List<Pallet>> ListByAgency(Agency agency);
    }

    public class ContainerService : IContainerService
    {
        private readonly databaseContext _context;
        public ContainerService(databaseContext context)
        {
            _context = context;
        }

        public async Task<Result<Pallet>> Create(Agency agency, CreatePalletModel model)
        {
            var equipment = new Pallet(agency, model.QtyPallets);

            _context.PalletCubiq.Add(equipment);
            await _context.SaveChangesAsync();

            return Result.Success(equipment);
        }

        public async Task<Result<Pallet>> AddPackage(AddPackageModel model, Guid userId)
        {
            var pallet = await _context.PalletCubiq.Include(x => x.Packages).FirstOrDefaultAsync(x => x.Id == model.PalletId);

            if (pallet == null)
                return Result.Failure<Pallet>("El pallet no existe");

            var package = await _context.Paquete.Include(x => x.OrderCubiq).Include(x => x.Pallet).FirstOrDefaultAsync(x => x.Numero == model.PackageNumber);
            if (package == null)
                return Result.Failure<Pallet>("El paquete no existe");
            if (package.Pallet != null)
                return Result.Failure<Pallet>($"El paquete ya fué añadido al pallet {package.Pallet.Number}");
            if (pallet.Packages.Contains(package))
                return Result.Failure<Pallet>("El paquete ya existe en el pallet");


            package.Pallet = pallet;
            package.Status = Paquete.STATUS_PALLET;
            //package.OrderCubiq.UpdateStatus(OrderCubiq.STATUS_PALLET, userId);
            pallet.AddPackage(package);
            _context.PalletCubiq.Update(pallet);
            _context.Paquete.Update(package);
            _context.OrderCubiqs.Update(package.OrderCubiq);
            await _context.SaveChangesAsync();

            // verificar si todos los paquetes del pre despacho fueron añadidos a pallets y cambiar estado
            if(package.PreDespachoId != null)
            {
                var preDespacho = await _context.PreDespachoCubiqs.Include(x => x.Paquetes).FirstOrDefaultAsync(x => x.Id == package.PreDespachoId);
                if(preDespacho.Paquetes.All(x => x.Status == Paquete.STATUS_PALLET))
                {

                   preDespacho.Status = PreDespachoCubiq.STATUS_COMPLETADO;
                    _context.Update(preDespacho);
                    await _context.SaveChangesAsync();
                }
            }

            return Result.Success<Pallet>(await GetPallet(pallet.Id));
        }

        public async Task<Pallet> GetPallet(Guid id)
        {
            return await _context.PalletCubiq
             .AsNoTracking()
             .Include(p => p.Agency)
             .Include(p => p.Guia)
             .Include(sp => sp.Packages)
                     .ThenInclude(p => p.OrderCubiq)
                         .ThenInclude(oc => oc.Client)
                            .ThenInclude(oc => oc.Address)
                .Include(sp => sp.Packages)
                     .ThenInclude(p => p.OrderCubiq)
                         .ThenInclude(oc => oc.Client)
                            .ThenInclude(oc => oc.Phone)
             .Include(sp => sp.Packages)
                     .ThenInclude(p => p.OrderCubiq)
                         .ThenInclude(oc => oc.Contact)
                             .ThenInclude(c => c.Address)
             .Include(sp => sp.Packages)
                     .ThenInclude(p => p.OrderCubiq)
                         .ThenInclude(oc => oc.Contact)
                             .ThenInclude(c => c.Phone1)
             .Include(sp => sp.Packages)
                     .ThenInclude(p => p.OrderCubiq)
                         .ThenInclude(oc => oc.Contact)
                             .ThenInclude(c => c.Phone2)
             .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Result> DeletePackageOfPallet(Guid packageId)
        {
            var package = await _context.Paquete.Include(x => x.Pallet).FirstOrDefaultAsync(x => x.PaqueteId == packageId);
            if (package == null) return Result.Failure("El paquete no existe");
            if (package.Pallet == null) return Result.Failure("El paquete no existe en el contenedor");

            package.Status = Paquete.STATUS_INICIADO;
            package.Pallet = null;
            _context.Attach(package);
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> DeletePallet(Guid id)
        {
            var equipment = await _context.PalletCubiq
            .Include(x => x.Packages).FirstOrDefaultAsync(x => x.Id == id);

            if (equipment.Packages.Count > 1)
                return Result.Failure("El contenedor no puede ser eliminado ya que contiene paquetes");

            _context.Remove(equipment);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<List<Pallet>> ListByAgency(Agency agency)
        {
            var containers = await _context.PalletCubiq
                .Where(x => x.Agency.AgencyId == agency.AgencyId).OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return containers;
        } 
    }
}