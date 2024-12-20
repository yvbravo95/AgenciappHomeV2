using Agenciapp.Domain.Models;
using Agenciapp.Service.PassportServices.IChequeServices.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agenciapp.Service.PassportServices.IChequeServices
{
    public interface IChequeService
    {
        Task<Result<bool>> Create(CreateChequeModel model);
        Task<Result<bool>> Update(CreateChequeModel model);
        Task<Result<bool>> CreateOtorgamiento(CreateChequeOtorgamientoModel model);
    }

    public class ChequeService: IChequeService
    {
        private readonly databaseContext _context;
        public ChequeService(databaseContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Create(CreateChequeModel model)
        {
            ManifiestoPasaporte manifiesto = await _context.ManifiestosPasaporte.Include(x => x.Passports).Include(x => x.Cheques).FirstOrDefaultAsync(x => x.Id == model.ManifiestoId);
            if(manifiesto == null)
            {
                return Result.Failure<bool>("El manifiesto no existe");
            }
            else if (manifiesto.Cheques.Where(x => !x.GetType().Equals(typeof(ChequeOtorgamiento))).Any())
            {
                return Result.Failure<bool>("El manifiesto ya tiene cheques");
            }

            if (!manifiesto.Passports.Any())
            {
                return Result.Failure<bool>("El manifiesto no tiene pasaportes");
            }
            int number = 0;
            switch (manifiesto.Passports.FirstOrDefault().ServicioConsular)
            {
                case ServicioConsular.None:
                    break;
                case ServicioConsular.PrimerVez:
                case ServicioConsular.PrimerVez2:
                    number = model.Number1 - (manifiesto.Passports.Count() * 2) + 1;
                    foreach (var passport in manifiesto.Passports)
                    {
                        var chequePrimerVez = new ChequePrimeraVez(number.ToString(), manifiesto, manifiesto.Passports.FirstOrDefault().ServicioConsular, passport, false);
                        manifiesto.Cheques.Add(chequePrimerVez);
                        number++;
                        chequePrimerVez = new ChequePrimeraVez(number.ToString(), manifiesto, manifiesto.Passports.FirstOrDefault().ServicioConsular, passport, true);
                        manifiesto.Cheques.Add(chequePrimerVez);
                        number++;
                    }
                    break;
                case ServicioConsular.Prorroga1:
                    var chequeProrroga1 = new ChequeProrroga(model.Number1.ToString(), manifiesto, manifiesto.Passports.FirstOrDefault().ServicioConsular);
                    manifiesto.Cheques.Add(chequeProrroga1);
                    break;
                case ServicioConsular.Prorroga2:
                    var chequeProrroga2 = new ChequeProrrogaDoble(model.Number1.ToString(), manifiesto, manifiesto.Passports.FirstOrDefault().ServicioConsular);
                    manifiesto.Cheques.Add(chequeProrroga2);
                    break;
                case ServicioConsular.Renovacion:
                case ServicioConsular.Renovacion2:
                    var chequeRenovacion = new ChequeRenovacion(model.Number1.ToString(), manifiesto, manifiesto.Passports.FirstOrDefault().ServicioConsular,false);
                    manifiesto.Cheques.Add(chequeRenovacion);
                    chequeRenovacion = new ChequeRenovacion(model.Number2.ToString(), manifiesto, manifiesto.Passports.FirstOrDefault().ServicioConsular, true);
                    manifiesto.Cheques.Add(chequeRenovacion);
                    break;
                case ServicioConsular.HE11:
                    number = model.Number1 - manifiesto.Passports.Count() + 1;
                    foreach (var item in manifiesto.Passports)
                    {
                        var chequeHE11 = new ChequeHE11(number.ToString(), manifiesto, manifiesto.Passports.FirstOrDefault().ServicioConsular, item);
                        manifiesto.Cheques.Add(chequeHE11);
                        number++;
                    }
                    
                    break;
                default:
                    var cheque = new Cheque(model.Number1.ToString(), manifiesto, manifiesto.Passports.FirstOrDefault().ServicioConsular);
                    manifiesto.Cheques.Add(cheque);
                    break;
            }
            _context.Attach(manifiesto);

            var guia = _context.GuiasPasaporte.Include(x => x.ManifiestosPasaporte).ThenInclude(x => x.Cheques).FirstOrDefault(x => x.GuiaId == manifiesto.GuiaPasaporteGuiaId);
            if(guia.ManifiestosPasaporte.All(x => x.Cheques.Any()))
            {
                guia.Status = GuiaPasaporte.Status_Consulado;
                guia.FechaEnvio = DateTime.Now;
                _context.Attach(guia);
            }

            await _context.SaveChangesAsync();
            return Result.Success(true);
        }

        public async Task<Result<bool>> CreateOtorgamiento(CreateChequeOtorgamientoModel model)
        {
            ManifiestoPasaporte manifiesto = await _context.ManifiestosPasaporte.Include(x => x.Passports).Include(x => x.Cheques).FirstOrDefaultAsync(x => x.Id == model.ManifiestoId);
            if (manifiesto == null)
            {
                return Result.Failure<bool>("El manifiesto no existe");
            }
            var cheque = await _context.ChequeOtorgamientos.FirstOrDefaultAsync(x => x.ManifiestoPasaporte.Id == model.ManifiestoId && x.Passport.PassportId == model.PassportId);
            if(cheque != null)
                return Result.Failure<bool>("El pasaporte ya tiene un cheque de otorgamiento");
            Passport passport = await _context.Passport.FirstOrDefaultAsync(x => x.PassportId == model.PassportId);
            if(passport == null)
                return Result.Failure<bool>("El pasaporte no existe");
            if(passport.ServicioConsular != ServicioConsular.PrimerVez && passport.ServicioConsular != ServicioConsular.PrimerVez2 && passport.ServicioConsular != ServicioConsular.HE11)
                return Result.Failure<bool>("El pasaporte no puede tener un cheque otorgamiento");

            var chequeOtorgamiento = new ChequeOtorgamiento(model.Number.ToString(), manifiesto, passport.ServicioConsular, passport);
            _context.Attach(chequeOtorgamiento);
            await _context.SaveChangesAsync();

            return Result.Success(true);
        }

        public async Task<Result<bool>> Update(CreateChequeModel model)
        {
            var manifiesto = await _context.ManifiestosPasaporte
                .Include(x => x.Cheques)
                .Include(x => x.Passports)
                .FirstOrDefaultAsync(x => x.Id == model.ManifiestoId);
            if (manifiesto == null)
                return Result.Failure<bool>("El manifiesto no existe");

            //Elimino los cheques generados
            var allcheques = _context.Cheques.Where(x => !x.GetType().Equals(typeof(ChequeOtorgamiento)) && x.ManifiestoPasaporte == manifiesto);
            _context.Cheques.RemoveRange(allcheques);

            await _context.SaveChangesAsync();

            var response = await Create(model);

            return Result.Success(true);
        }

        private async Task<int> GetLastNumber()
        {
            int number = -1;
            if (_context.Cheques.Any())
            {
                number = int.Parse((await _context.Cheques.OrderByDescending(x => x.Number).FirstOrDefaultAsync()).Number);
                number++;
            }
            else
            {
                var numberInit = _context.Configuracion.FirstOrDefault(x => x.Key == "ChequePassport");
                if (numberInit == null)
                {
                    return -1;
                }
                number = int.Parse(numberInit.Value);
            }
            return number;
        }

        private async Task UpdateChequePrimeraVez(ManifiestoPasaporte manifiesto,  int number)
        {
            //Agrego los cheques nuevos
            foreach (var passport in manifiesto.Passports)
            {
                var existCheque = await _context.ChequePrimeraVezs.AnyAsync(x =>  x.Passport == passport && x.ManifiestoPasaporte == manifiesto);
                if (!existCheque)
                {
                    var chequePrimerVez = new ChequePrimeraVez(number.ToString(), manifiesto, manifiesto.Passports.FirstOrDefault().ServicioConsular, passport, false);
                    manifiesto.Cheques.Add(chequePrimerVez);
                    number++;
                    chequePrimerVez = new ChequePrimeraVez(number.ToString(), manifiesto, manifiesto.Passports.FirstOrDefault().ServicioConsular, passport, true);
                    manifiesto.Cheques.Add(chequePrimerVez);
                    number++;
                }
            }
            //Elimino los cheques anteriores
            var allcheques = _context.ChequePrimeraVezs.Include(x => x.Passport).Where(x => x.ManifiestoPasaporte == manifiesto);
            foreach (var item in allcheques)
            {
                if(!manifiesto.Passports.Any(x => x == item.Passport))
                {
                    _context.ChequePrimeraVezs.Remove(item);
                }
            }
        }

        private async Task UpdateChequeHE11(ManifiestoPasaporte manifiesto, int number)
        {
            //Agrego los cheques nuevos
            foreach (var passport in manifiesto.Passports)
            {
                var existCheque = await _context.ChequeHE11s.AnyAsync(x => x.Passport == passport && x.ManifiestoPasaporte == manifiesto);
                if (!existCheque)
                {
                    var chequeHE11 = new ChequeHE11(number.ToString(), manifiesto, manifiesto.Passports.FirstOrDefault().ServicioConsular, passport);
                    manifiesto.Cheques.Add(chequeHE11);
                    number++;
                }
            }
            //Elimino los cheques anteriores
            var allcheques = _context.ChequeHE11s.Include(x => x.Passport).Where(x => x.ManifiestoPasaporte == manifiesto);
            foreach (var item in allcheques)
            {
                if (!manifiesto.Passports.Any(x => x == item.Passport))
                {
                    _context.ChequeHE11s.Remove(item);
                }
            }
        }

    }
}
