using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgenciappHome.Models;
using AgenciappHome.Models.ModelsEnvioCaribe;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AgenciappHome.Controllers.ApiRapidM
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnviosCaribeController : ControllerBase
    {
        private readonly databaseContext _context;
        private IWebHostEnvironment _env;

        public EnviosCaribeController(databaseContext context, IWebHostEnvironment env)
        {
            _context = context;
            this._env = env;
        }
        [HttpPost]
        public async Task<ActionResult> Create([FromBody]AuxEnvioCaribe request)
        {
            try
            {
                //Guardo el request en un file Json
                string sWebRootFolder = _env.WebRootPath;
                string path = sWebRootFolder + Path.DirectorySeparatorChar + "FileJsonEnviosCaribe";
                path = Path.Combine(path, request.numero + ".json");
                System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(request));

                if (request.servicio == "Correo-Aereo" || request.servicio == "Aerovaradero- Recogida" || request.servicio == "Maritimo-Palco Almacen" || request.servicio == "Palco ENTREGA A DOMICILIO")
                {
                    User user = _context.User.FirstOrDefault(x => x.otroUsername == request.user);
                    if (user != null)
                    {
                        Agency agency = _context.Agency.Find(user.AgencyId);
                        Office office = _context.Office.FirstOrDefault(x => x.AgencyId == agency.AgencyId);

                        //Cliente
                        var verify = _context.Client.Where(y => y.AgencyId == agency.AgencyId)
                            .Join(_context.Phone.Where(y => y.Type == "Móvil"),
                            cli => cli.ClientId,
                            p => p.ReferenceId,
                            (cli, p) => new { idcliente = cli.ClientId, name = cli.Name, lastname = cli.LastName, phone = p.Number })
                            .Where(y => y.name == request.client.name && y.lastname == request.client.lastName && y.phone.Replace("(", "").Replace(")", "").Replace("-", "") == request.client.phoneNumber).FirstOrDefault();
                        Client newClient;
                        if (verify != null)
                        {
                            //Si existe el cliente lo busco 
                            newClient = await _context.Client.FindAsync(verify.idcliente);
                        }
                        else
                        {
                            // Creo el cliente
                            newClient = new Client();
                            newClient.ClientId = Guid.NewGuid();
                            newClient.Agency = agency;
                            newClient.CreatedAt = DateTime.Now;
                            newClient.PhoneNumber = request.client.phoneNumber;
                            newClient.Name = request.client.name;
                            newClient.LastName = request.client.lastName;
                            newClient.Email = request.client.email;
                            newClient.ID = "";
                            newClient.checkNotifications = true;
                            string initialName = newClient.Name.ElementAt(0) + "".ToUpper();
                            string initialLastName = newClient.LastName.ElementAt(0) + "".ToUpper();
                            newClient.ClientNumber = "CL" + _context.Client.Count() + initialName + initialLastName + newClient.CreatedAt.ToString("mmss");

                            Address addclient = new Address();
                            addclient.ReferenceId = newClient.ClientId;
                            addclient.CreatedAt = DateTime.Now;
                            addclient.Type = "Casa";
                            addclient.UpdatedAt = DateTime.Now;
                            addclient.AddressId = Guid.NewGuid();
                            addclient.City = request.client.city != null? request.client.city:"" ;
                            addclient.State = request.client.state;
                            addclient.Country = request.client.country;
                            addclient.Zip = request.client.zip;
                            addclient.AddressLine1 = request.client.addressLine1;
                            addclient.AddressLine2 = request.client.addressLine2;
                            _context.Address.Add(addclient);

                            newClient.Address = addclient;
                            _context.Client.Add(newClient);

                            Phone phoneclient = new Phone();
                            phoneclient.PhoneId = Guid.NewGuid();
                            phoneclient.ReferenceId = newClient.ClientId;
                            phoneclient.Type = "Móvil";
                            phoneclient.Number = request.client.phoneNumber;
                            _context.Phone.Add(phoneclient);
                            newClient.Phone = phoneclient;
                        }

                        //Contacto
                        //Busco si el contacto existe
                        if (request.contact.phoneMovil != "" || request.contact.phoneCasa != "") 
                        {
                            if (request.contact.phoneMovil == "" && request.contact.phoneCasa != "")
                            {
                                request.contact.phoneMovil = request.contact.phoneCasa;
                            }
                        }

                        Contact newContacto;
                        var verifycontact = _context.Contact.Where(x => x.Phone1.Number == request.contact.phoneMovil.Replace("(", "").Replace(")", "").Replace("-", "") && x.Name == request.contact.name && x.LastName == request.contact.lastName)
                           .Join(_context.AgencyContact,
                           c => c.ContactId,
                           a => a.ContactId,
                           (c, a) => new { c, a })
                           .Where(x => x.a.AgencyId == agency.AgencyId).FirstOrDefault();
                        if (verifycontact != null)
                        {
                            newContacto = _context.Contact.Include(x => x.Address).FirstOrDefault(x => x.ContactId == verifycontact.c.ContactId);
                        }
                        else
                        {
                            int cantContacts = _context.Contact.Count() + 1;
                            string initialName = request.contact.name.ElementAt(0) + "".ToUpper();
                            string initialLastName = request.contact.lastName.ElementAt(0) + "".ToUpper();

                            newContacto = new Contact();
                            newContacto.ContactId = Guid.NewGuid();
                            newContacto.CI = request.contact.ci;
                            newContacto.CreatedAt = DateTime.Now;
                            newContacto.ContactNumber = "CL" + cantContacts + initialName + initialLastName + newContacto.CreatedAt.ToString("mmss");
                            newContacto.PhoneNumber1 = request.contact.phoneMovil;
                            newContacto.PhoneNumber2 = request.contact.phoneCasa;
                            newContacto.Name = request.contact.name;
                            newContacto.LastName = request.contact.lastName;

                            AgencyContact agencyContact = new AgencyContact();
                            agencyContact.AgencyContactId = Guid.NewGuid();
                            agencyContact.AgencyId = agency.AgencyId;
                            agencyContact.ContactId = newContacto.ContactId;
                            _context.AgencyContact.Add(agencyContact);

                            Address addresscontact = new Address();
                            addresscontact.AddressId = Guid.NewGuid();
                            addresscontact.Type = "Casa";
                            addresscontact.CreatedAt = DateTime.Now;
                            addresscontact.UpdatedAt = DateTime.Now;
                            addresscontact.ReferenceId = newContacto.ContactId;
                            addresscontact.AddressLine1 = request.contact.addressLine1;
                            addresscontact.AddressLine2 = request.contact.addressLine2;
                            if (request.contact.provincia.Contains("Pinar"))
                            {
                                request.contact.provincia = "Pinar del Rio";
                            }
                            else if(request.contact.provincia.Equals("Santiago De Cuba"))
                            {
                                request.contact.provincia = "Santiago de Cuba";
                            }
                            addresscontact.City = request.contact.provincia != null? request.contact.provincia:"";
                            addresscontact.State = request.contact.municipio;
                            addresscontact.Zip = request.contact.reparto; ;
                            addresscontact.Country = "";
                            _context.Address.Add(addresscontact);
                            newContacto.Address = addresscontact;
                            _context.Contact.Add(newContacto);

                            Phone phonecontact = new Phone();
                            phonecontact.PhoneId = Guid.NewGuid();
                            phonecontact.Number = request.contact.phoneMovil;
                            phonecontact.ReferenceId = newContacto.ContactId;
                            phonecontact.Type = "Móvil";
                            _context.Phone.Add(phonecontact);
                            newContacto.Phone1 = phonecontact;

                            Phone phonecontactCasa = new Phone();
                            phonecontactCasa.PhoneId = Guid.NewGuid();
                            phonecontactCasa.Number = request.contact.phoneCasa;
                            phonecontactCasa.ReferenceId = newContacto.ContactId;
                            phonecontactCasa.Type = "Casa";
                            _context.Phone.Add(phonecontactCasa);
                            newContacto.Phone2 = phonecontactCasa;
                        }

                        //Verifico si existe la relacion cliente contacto
                        ClientContact cc = _context.ClientContact.FirstOrDefault(x => x.ContactId == newContacto.ContactId && x.ClientId == newClient.ClientId);
                        if (cc == null)
                        {
                            ClientContact auxcc = new ClientContact
                            {
                                CCId = Guid.NewGuid(),
                                ClientId = newClient.ClientId,
                                ContactId = newContacto.ContactId
                            };
                            _context.ClientContact.Add(auxcc);
                        }

                        EnvioCaribe envio = new EnvioCaribe();
                        envio.Agency = agency;
                        envio.AgencyId = agency.AgencyId;
                        envio.Amount = 0;
                        envio.Client = newClient;
                        envio.ClientId = newClient.ClientId;
                        envio.Contact = newContacto;
                        envio.ContactId = newContacto.ContactId;
                        envio.costo = 0;
                        envio.Date = DateTime.UtcNow;
                        envio.modalidadEnvio = request.modalidadEnvio;
                        envio.servicio = request.servicio;
                        envio.Office = office;
                        envio.OfficeId = office.OfficeId;
                        envio.OtrosCostos = (decimal)1.0;
                        envio.ValorPagado = 0;
                        envio.numero = request.numero;

                        //Busco el mayorista 
                        Wholesaler w;

                        decimal precio = 0;
                        decimal costo = 0;

                        if (agency.Name == "Rapid Multiservice")
                        {
                            w = _context.Wholesalers
                                .Include(x => x.tipoServicioHabana)
                                .Include(x => x.tipoServicioRestoProv)
                                .FirstOrDefault(x =>x.EsVisible && x.AgencyId == agency.AgencyId && x.Category.category == "Maritimo-Aereo");

                            if (w == null)
                            {
                                return BadRequest("No existe un mayorista para la categoría Maritimo-Aereo");
                            }

                            List<TipoServicioMayorista> servicios = null;
                            if (request.contact.provincia == "La Habana")
                            {
                                servicios = w.tipoServicioHabana;
                            }
                            else
                            {
                                 servicios = w.tipoServicioRestoProv;
                            }

                            if (servicios != null)
                            {
                                if (request.servicio == "Correo-Aereo")
                                {
                                    var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                    precio = servicio.pventaAereo;
                                    costo = servicio.costoAereo;
                                }
                                else if (request.servicio == "Correo-Maritimo")
                                {
                                    var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Correo_Cuba);
                                    precio = servicio.pventaMaritimo;
                                    costo = servicio.costoMaritimo;
                                }
                                else if (request.servicio == "Aerovaradero- Recogida")
                                {
                                    var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Aereo_Varadero);
                                    precio = servicio.pventaAereo;
                                    costo = servicio.costoAereo;
                                }
                                else if (request.servicio == "Maritimo-Palco Almacen")
                                {
                                    var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Palco);
                                    precio = servicio.pventaMaritimo;
                                    costo = servicio.costoMaritimo;
                                }
                                else if (request.servicio == "Palco ENTREGA A DOMICILIO")
                                {
                                    var servicio = servicios.FirstOrDefault(x => x.Nombre == TipoServicioMayorista.Palco);
                                    precio = servicio.pventaAereo;
                                    costo = servicio.costoAereo;
                                }
                            }
                        }
                        else //Es un minorista
                        {
                            w = _context.Wholesalers.FirstOrDefault(x =>x.EsVisible && x.byTransferencia && x.AgencyId == agency.AgencyId && x.Category.category == "Maritimo-Aereo");
                            if (w == null)
                            {
                                return BadRequest("No existe un mayorista por transferencia para la categoría Maritimo-Aereo");
                            }
                            else
                            {
                                var costoxmodulo = _context.CostoxModuloMayorista.Include(x => x.valoresTramites).ThenInclude(x => x.valores).Include(x => x.modAsignados).Where(x => x.AgencyId == agency.AgencyId && x.modAsignados.Where(y => y.IdWholesaler == w.IdWholesaler).Any()).FirstOrDefault();
                                var valoresTramite = costoxmodulo.valoresTramites.FirstOrDefault(x => x.Tramite == "Maritimo-Aereo");
                                var valorxprovincia = valoresTramite.valores.FirstOrDefault(x => x.provincia == request.contact.provincia);
                                costo = valorxprovincia.valor;
                                envio.AgencyTransferidaId = costoxmodulo.AgencyMayoristaId;
                                envio.AgencyTransferida = _context.Agency.Find(costoxmodulo.AgencyMayoristaId);
                                var precioventa = _context.CostosxModulo.Include(x => x.valoresTramites).ThenInclude(x => x.valores).Where(x => x.AgencyId == agency.AgencyId && x.GetType().Name == "CostosxModulo").FirstOrDefault();
                                valoresTramite = precioventa.valoresTramites.FirstOrDefault(x => x.Tramite == "Maritimo-Aereo");
                                valorxprovincia = valoresTramite.valores.FirstOrDefault(x => x.provincia == request.contact.provincia);
                                precio = valorxprovincia.valor;
                            }
                        }

                        foreach (var item in request.paquetes)
                        {
                            PaqueteEnvCaribe paquete = new PaqueteEnvCaribe
                            {
                                descripcion = item.descripcion,
                                EnvioCaribe = envio,
                                EnvioCaribeId = envio.EnvioCaribeId,
                                numero = item.numero,
                                PaqueteEnvCaribeId = Guid.NewGuid(),
                                peso = item.peso,
                                tarifa = Math.Round(item.peso * precio, 2),
                                tipo_producto = item.tipo_producto,
                            };
                            _context.PaqueteEnvCaribes.Add(paquete);
                            if (paquete.peso == (decimal)3.30 && paquete.descripcion == "Miscelaneos")
                            {
                                envio.Amount += Math.Round(item.peso * precio, 2);
                            }
                            else
                            {
                                if (request.servicio == "Aerovaradero- Recogida")
                                {
                                    envio.Amount += Math.Round(item.peso * 5, 2);
                                }
                                else if(request.servicio == "Palco ENTREGA A DOMICILIO"){
                                    envio.Amount += Math.Round(item.peso * precio, 2);
                                }
                                else
                                {
                                    if (newContacto.Address.City == "La Habana")
                                    {
                                        envio.Amount += Math.Round(item.peso * 6, 2);// se multiplica por 6 si es a la habana
                                    }
                                    else
                                    {
                                        envio.Amount += Math.Round(item.peso * 7, 2);// se multiplica por 7 si es al resto de provincias
                                    }
                                }
                            }
                            envio.costo += Math.Round(item.peso * costo, 2);
                            
                        }
                        envio.Wholesaler = w;
                        envio.WholesalerId = w.IdWholesaler;
                        envio.Precio = envio.Amount;
                        envio.Amount += envio.OtrosCostos;

                        if (envio.Balance != 0)
                        {
                            envio.Status = EnvioCaribe.STATUS_PENDIENTE;
                            var sxc = new servicioxCobrar
                            {
                                date = DateTime.UtcNow,
                                ServicioId = envio.EnvioCaribeId,
                                tramite = "Envío Caribe",
                                NoServicio = envio.Number,
                                cliente = envio.Client,
                                No_servicioxCobrar = "PAYT" + envio.Date.ToString("yyyyMMddHHmmss"),
                                cobrado = 0,
                                remitente = envio.Client,
                                destinatario = envio.Contact,
                                valorTramite = envio.Amount,
                                importeACobrar = envio.Balance,
                                mayorista = envio.Agency
                            };
                            _context.servicioxCobrar.Add(sxc);

                        }
                        else
                        {
                            envio.Status = EnvioCaribe.STATUS_INICIADA;
                        }
                        envio.User = user;
                        envio.UserId = user.UserId;
                        _context.EnvioCaribes.Add(envio);

                        if (envio.AgencyTransferidaId != null) //Por transferencia
                        {
                            var porPagar = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = envio.costo + envio.OtrosCostos,
                                Mayorista = envio.Wholesaler,
                                Agency = agency,
                                SId = envio.EnvioCaribeId,
                                EnvioCaribe = envio,
                                NoServicio = envio.Number,
                                Tipo = STipo.EnvioCaribe,
                                SubTipo = envio.servicio,
                                Express = false
                            };
                            _context.ServiciosxPagar.Add(porPagar);

                            var s = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = envio.OtrosCostos,
                                Mayorista = null,
                                Agency = envio.AgencyTransferida,
                                SId = envio.EnvioCaribeId,
                                NoServicio = envio.Number,
                                EnvioCaribe = envio,
                                Tipo = STipo.EnvioCaribe,
                                SubTipo = envio.servicio,
                            };
                            _context.ServiciosxPagar.Add(s);
                        }
                        else
                        {
                            var porPagar = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = envio.costo,
                                Mayorista = envio.Wholesaler,
                                Agency = agency,
                                SId = envio.EnvioCaribeId,
                                EnvioCaribe = envio,
                                NoServicio = envio.Number,
                                Tipo = STipo.EnvioCaribe,
                                SubTipo = envio.servicio,
                                Express = false
                            };
                            _context.ServiciosxPagar.Add(porPagar);

                            var s = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = envio.OtrosCostos,
                                Mayorista = null,
                                Agency = agency,
                                SId = envio.EnvioCaribeId,
                                NoServicio = envio.Number,
                                EnvioCaribe = envio,
                                Tipo = STipo.EnvioCaribe,
                                SubTipo = envio.servicio
                            };
                            _context.ServiciosxPagar.Add(s);
                        }
                        

                        //Guardo que empleado realizo el tramite
                        TramiteEmpleado tramite = new TramiteEmpleado();
                        tramite.fecha = DateTime.UtcNow;
                        tramite.Id = Guid.NewGuid();
                        tramite.IdEmpleado = user.UserId;
                        tramite.IdTramite = envio.EnvioCaribeId;
                        tramite.tipoTramite = TramiteEmpleado.tipo_ENVIOCARIBE;
                        tramite.IdAgency = agency.AgencyId;
                        _context.TramiteEmpleado.Add(tramite);

                        

                        envio.RegistroEstados.Add(new RegistroEstado
                        {
                            Estado = envio.Status,
                            Date = DateTime.Now,
                            User = user
                        });

                        await _context.SaveChangesAsync();

                        return Ok("Success");
                    }
                    else
                    {
                        return BadRequest("El empleado no existe.");
                    }
                }
                else
                {
                    return BadRequest("El servicio no es Correo-Aéreo");
                }
                 
                 
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
            
        }
    }
}