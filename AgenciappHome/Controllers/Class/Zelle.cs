using AgenciappHome.Models;
using RapidMultiservice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Controllers.Class
{
    public static class Zelle
    {
        public static async Task<Result<string>> Asociate(databaseContext _context, Guid zelleId, Guid tramiteId, STipo type, Guid AgencyId)
        {
            try
            {
                var zelle = _context.ZelleItems.FirstOrDefault(x => x.AgencyId == AgencyId && x.ZellItemId == zelleId);
                if (zelle == null)
                {
                    return new Result<string>
                    {
                        Success = false,
                        ErrorMessage = "El pago zelle no existe"
                    };
                }
                zelle.ReferenceId = tramiteId;
                switch (type)
                {
                    case STipo.Recarga:
                        var rechargue = await _context.Rechargue.FindAsync(tramiteId);
                        if (rechargue == null)
                        {
                            return new Result<string>
                            {
                                Success = false,
                                ErrorMessage = "El trámite no existe"
                            };
                        }
                        zelle.Rechargue = rechargue;
                        zelle.OrderNumber = rechargue.Number;
                        zelle.Type = STipo.Recarga;
                        break;
                    case STipo.Remesa:
                        var remesa = await _context.Remittance.FindAsync(tramiteId);
                        if (remesa == null)
                        {
                            return new Result<string>
                            {
                                Success = false,
                                ErrorMessage = "El trámite no existe"
                            };
                        }
                        zelle.Remittance = remesa;
                        zelle.OrderNumber = remesa.Number;
                        zelle.Type = STipo.Remesa;
                        break;
                    case STipo.Paquete:
                        var paquete = await _context.Order.FindAsync(tramiteId);
                        if (paquete == null)
                        {
                            return new Result<string>
                            {
                                Success = false,
                                ErrorMessage = "El trámite no existe"
                            };
                        }
                        zelle.Order = paquete;
                        zelle.OrderNumber = paquete.Number;
                        zelle.Type = STipo.Paquete;
                        break;
                    case STipo.Maritimo:
                        var maritimo = await _context.EnvioMaritimo.FindAsync(tramiteId);
                        if (maritimo == null)
                        {
                            return new Result<string>
                            {
                                Success = false,
                                ErrorMessage = "El trámite no existe"
                            };
                        }
                        zelle.EnvioMaritimo = maritimo;
                        zelle.OrderNumber = maritimo.Number;
                        zelle.Type = STipo.Maritimo;
                        break;
                    case STipo.Cubiq:
                        var cubiq = await _context.OrderCubiqs.FindAsync(tramiteId);
                        if (cubiq == null)
                        {
                            return new Result<string>
                            {
                                Success = false,
                                ErrorMessage = "El trámite no existe"
                            };
                        }
                        zelle.OrderCubic = cubiq;
                        zelle.OrderNumber = cubiq.Number;
                        zelle.Type = STipo.Cubiq;
                        break;
                    case STipo.EnvioCaribe:
                        var caribe = await _context.EnvioCaribes.FindAsync(tramiteId);
                        if (caribe == null)
                        {
                            return new Result<string>
                            {
                                Success = false,
                                ErrorMessage = "El trámite no existe"
                            };
                        }
                        zelle.EnvioCaribe = caribe;
                        zelle.OrderNumber = caribe.Number;
                        zelle.Type = STipo.EnvioCaribe;
                        break;
                    case STipo.Reserva:
                        var reserva = await _context.Ticket.FindAsync(tramiteId);
                        if (reserva == null)
                        {
                            return new Result<string>
                            {
                                Success = false,
                                ErrorMessage = "El trámite no existe"
                            };
                        }
                        zelle.Reserva = reserva;
                        zelle.OrderNumber = reserva.ReservationNumber;
                        zelle.Type = STipo.Reserva;
                        break;
                    case STipo.Servicio:
                        var servicio = await _context.Servicios.FindAsync(tramiteId);
                        if (servicio == null)
                        {
                            return new Result<string>
                            {
                                Success = false,
                                ErrorMessage = "El trámite no existe"
                            };
                        }
                        zelle.Servicio = servicio;
                        zelle.OrderNumber = servicio.numero;
                        zelle.Type = STipo.Servicio;
                        break;
                    case STipo.Passport:
                        var pasaporte = await _context.Passport.FindAsync(tramiteId);
                        if (pasaporte == null)
                        {
                            return new Result<string>
                            {
                                Success = false,
                                ErrorMessage = "El trámite no existe"
                            };
                        }
                        zelle.Passport = pasaporte;
                        zelle.OrderNumber = pasaporte.OrderNumber;
                        zelle.Type = STipo.Passport;
                        break;
                    case STipo.None:
                        return new Result<string>
                        {
                            Success = false,
                            ErrorMessage = "El pago zelle ya tiene asociado un trámite."
                        };
                    default:
                        return new Result<string>
                        {
                            Success = false,
                            ErrorMessage = "El trámite no existe."
                        };
                }
                _context.Update(zelle);

                return new Result<string>
                {
                    Success = true,
                    SuccessMessage = "El trámite ha sido asociado"
                };
            }
            catch(Exception e)
            {
                return new Result<string>
                {
                    Success = false,
                    ErrorMessage = "No se ha podido sociar el trámite"
                };
            }
            
        }

        private static void Json(object p)
        {
            throw new NotImplementedException();
        }
    }
}
