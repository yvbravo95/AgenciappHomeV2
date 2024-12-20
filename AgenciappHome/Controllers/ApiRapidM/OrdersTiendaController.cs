using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Agenciapp.Common.Services.INotificationServices;
using AgenciappHome.Controllers.Class;
using AgenciappHome.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace AgenciappHome.Controllers.ApiRapidM
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrdersTiendaController : ControllerBase
    {
        private readonly databaseContext _context;
        private IWebHostEnvironment _env;
        private readonly INotificationService _notificationService;
        public OrdersTiendaController(databaseContext context, IWebHostEnvironment env, INotificationService notificationService)
        {
            _context = context;
            _env = env;
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> create([FromBody]Newtonsoft.Json.Linq.JObject value)
        {
            try
            {
                string status = value.SelectToken("status").Value<string>();

                string ordernumber = "CO" + DateTime.Now.ToString("yMMddHHmmssff");
                //Guardo el Json en un fichero con el numero de orden
                string sWebRootFolder = _env.WebRootPath;
                string path = sWebRootFolder + Path.DirectorySeparatorChar + "FileJsonWeb";
                path = Path.Combine(path, ordernumber + "-" + status + ".json");
                System.IO.File.WriteAllText(path, value.ToString());
               
                if (status == "completed")
                {
                    Agency agency = _context.Agency.FirstOrDefault(y => y.Name == "Rapid Multiservice");
                    Office office = _context.Office.Where(y => y.AgencyId == agency.AgencyId).FirstOrDefault();
                    User user = _context.User.FirstOrDefault(y => y.Username == "3055152551" && y.AgencyId == agency.AgencyId);
                    if (user != null)
                    {
                        // var x = value.GetValue("body");
                        Order order = new Order();
                        #region Cliente
                        // Para el cliente
                        var cliente = value.SelectToken("billing");
                        string nameaux = cliente.SelectToken("first_name").Value<String>();
                        string lastnameaux = cliente.SelectToken("last_name").Value<String>();
                        string phoneaux = cliente.SelectToken("phone").Value<String>();

                        //Verifico si existe el cliente
                        var verify = _context.Client.Where(y => y.AgencyId == agency.AgencyId)
                            .Join(_context.Phone.Where(y => y.Type == "Móvil"),
                            cli => cli.ClientId,
                            p => p.ReferenceId,
                            (cli, p) => new { idcliente = cli.ClientId, name = cli.Name, lastname = cli.LastName, phone = p.Number })
                            .Where(y => y.name == nameaux && y.lastname == lastnameaux && y.phone == phoneaux).FirstOrDefault();
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
                            newClient.PhoneNumber = phoneaux;
                            newClient.Name = cliente.SelectToken("first_name").Value<String>();
                            newClient.LastName = cliente.SelectToken("last_name").Value<String>();
                            newClient.Email = cliente.SelectToken("email").Value<String>();
                            newClient.ID = "";
                            string initialName = newClient.Name.ElementAt(0) + "".ToUpper();
                            string initialLastName = newClient.LastName.ElementAt(0) + "".ToUpper();
                            newClient.ClientNumber = "CL" + _context.Client.Count() + initialName + initialLastName + newClient.CreatedAt.ToString("mmss");

                            Address addclient = new Address();
                            addclient.ReferenceId = newClient.ClientId;
                            addclient.CreatedAt = DateTime.Now;
                            addclient.Type = "Casa";
                            addclient.UpdatedAt = DateTime.Now;
                            addclient.AddressId = Guid.NewGuid();
                            addclient.City = cliente.SelectToken("city").Value<String>();
                            addclient.State = cliente.SelectToken("state").Value<String>();
                            addclient.Country = cliente.SelectToken("country").Value<String>();
                            addclient.Zip = cliente.SelectToken("postcode").Value<String>();
                            addclient.AddressLine1 = cliente.SelectToken("address_1").Value<String>();
                            addclient.AddressLine2 = cliente.SelectToken("address_2").Value<String>();
                            _context.Address.Add(addclient);

                            newClient.Address = addclient;
                            _context.Client.Add(newClient);

                            Phone phoneclient = new Phone();
                            phoneclient.PhoneId = Guid.NewGuid();
                            phoneclient.ReferenceId = newClient.ClientId;
                            phoneclient.Type = "Móvil";
                            phoneclient.Number = cliente.SelectToken("phone").Value<String>();
                            _context.Phone.Add(phoneclient);
                        }
                        #endregion

                        #region Contacto
                        //Para el contacto
                        var contacto = value.SelectToken("shipping");
                        string namecontactaux = contacto.SelectToken("first_name").Value<String>();
                        string lastnamecontactaux = contacto.SelectToken("last_name").Value<String>();
                        var metadatacontact = value.SelectToken("meta_data");
                        var auxdata = metadatacontact.ElementAt(1); //Posicion del teléfono
                        string phone = auxdata.SelectToken("value").Value<String>();
                        auxdata = metadatacontact.ElementAt(1); //Posicion carnet de identidad
                        string carnetIdentidad = auxdata.SelectToken("value").Value<String>();
                        //Busco si el contacto existe
                        Contact newContacto;
                        var verifycontact = _context.AgencyContact.Where(y => y.AgencyId == agency.AgencyId)
                            .Join(_context.Contact,
                            ag => ag.ContactId,
                            co => co.ContactId,
                            (ag, co) => new { co })
                            .Join(_context.Phone.Where(y => y.Type == "Móvil"),
                            con => con.co.ContactId,
                            p => p.ReferenceId,
                            (con, p) => new { idcontact = con.co.ContactId, name = con.co.Name, lastname = con.co.LastName, phone = p.Number })
                            .Where(z => z.lastname == lastnamecontactaux && z.name == namecontactaux && z.phone == phone).FirstOrDefault();

                        if (verifycontact != null)
                        {
                            newContacto = await _context.Contact.FindAsync(verifycontact.idcontact);
                        }
                        else
                        {
                            int cantContacts = _context.Contact.Count() + 1;
                            string initialName = namecontactaux.ElementAt(0) + "".ToUpper();
                            string initialLastName = lastnamecontactaux.ElementAt(0) + "".ToUpper();

                            newContacto = new Contact();
                            newContacto.ContactId = Guid.NewGuid();
                            newContacto.CI = carnetIdentidad;
                            newContacto.CreatedAt = DateTime.Now;
                            newContacto.ContactNumber = "CL" + cantContacts + initialName + initialLastName + newContacto.CreatedAt.ToString("mmss");
                            newContacto.PhoneNumber1 = phone;
                            newContacto.Name = namecontactaux;
                            newContacto.LastName = lastnamecontactaux;

                            Address addresscontact = new Address();
                            addresscontact.AddressId = Guid.NewGuid();
                            addresscontact.Type = "Casa";
                            addresscontact.CreatedAt = DateTime.Now;
                            addresscontact.UpdatedAt = DateTime.Now;
                            addresscontact.ReferenceId = newContacto.ContactId;
                            addresscontact.AddressLine1 = contacto.SelectToken("address_1").Value<String>();
                            addresscontact.AddressLine2 = contacto.SelectToken("address_2").Value<String>();
                            addresscontact.City = contacto.SelectToken("city").Value<String>();
                            addresscontact.State = contacto.SelectToken("state").Value<String>();
                            addresscontact.Zip = "";
                            addresscontact.Country = contacto.SelectToken("country").Value<String>();
                            _context.Address.Add(addresscontact);
                            newContacto.Address = addresscontact;
                            _context.Contact.Add(newContacto);

                            Phone phonecontact = new Phone();
                            phonecontact.PhoneId = Guid.NewGuid();
                            phonecontact.Number = phone;
                            phonecontact.ReferenceId = newContacto.ContactId;
                            phonecontact.Type = "Móvil";
                            _context.Phone.Add(phonecontact);

                            Phone phonecontactCasa = new Phone();
                            phonecontactCasa.PhoneId = Guid.NewGuid();
                            phonecontactCasa.Number = "";
                            phonecontactCasa.ReferenceId = newContacto.ContactId;
                            phonecontactCasa.Type = "Casa";
                            _context.Phone.Add(phonecontactCasa);
                        }
                        #endregion

                        #region Obtener los productos
                        //Añado los productos al tramite
                        Package package = new Package();
                        package.PackageId = Guid.NewGuid();
                        package.PackageNavigation = order;
                        order.Package = package;
                        _context.Add(package);

                        var code = "BL" + DateTime.Now.ToString("yMMddHHmmssff");
                        var bag = new Bag();
                        bag.BagId = Guid.NewGuid();
                        bag.Code = code;
                        bag.OrderId = order.OrderId;

                        _context.Add(bag);

                        var productos = value.SelectToken("line_items");
                        List<ProductoBodega> productosBodega = new List<ProductoBodega>();
                        decimal priceProduct = 0;
                        int SumaCantidad = 0;
                        int count = 0;
                        string combos = "";

                        foreach (var product in productos.AsEnumerable())
                        {
                            string productname = product.SelectToken("name").Value<String>();
                            int productquantity = product.SelectToken("quantity").Value<int>();
                            string sku = product.SelectToken("sku").Value<String>();
                            decimal price = product.SelectToken("price").Value<decimal>();

                            priceProduct += price;

                            ProductoBodega pbodega = _context.ProductosBodegas.Include(x => x.Proveedor).FirstOrDefault(y => y.IdAgency == agency.AgencyId && y.Codigo == sku);
                            if (pbodega == null) //Si no existe el producto envio un sms al admin 
                            {
                                //Envio mensaje a los administradores
                                var admins = _context.User.Where(y => y.Type == "Agencia" && y.AgencyId == agency.AgencyId);
                                foreach (var item in admins)
                                {
                                    var phoneadmin = _context.Phone.FirstOrDefault(y => y.Type == "Móvil");
                                    string msg = "No se ha podido crear el trámite order_key <<" + value.SelectToken("order_key").Value<string>() + ">>";
                                    await _notificationService.sendSms(msg, phoneadmin.Number.Replace("(", "").Replace(")", "").Replace("-", ""));
                                }
                                return BadRequest("No existe el producto con sku: " + sku + " en la bodega");
                            }

                            if (count == 0)
                            {
                                order.wholesaler = pbodega.Proveedor;
                                var transferencia = _context.CostoxModuloMayorista.Include(x => x.modAsignados).Where(x => x.AgencyId == agency.AgencyId && x.modAsignados.Where(y => y.IdWholesaler == pbodega.Proveedor.IdWholesaler).Any()).FirstOrDefault();
                                if (transferencia != null)
                                {
                                    order.agencyTransferida = _context.Agency.Find(transferencia.AgencyMayoristaId);
                                }
                            }

                            if (order.agencyTransferida != null)
                            {
                                // Si es por transferencia se toma el precioreferencialminorista
                                decimal precioreferencialminorista = 0;
                                if (pbodega.Precio1Minorista != null)
                                {
                                    if (pbodega.Precio1Minorista.AgencyPrecioRefMinoristas.Any(x => x.AgencyId == agency.AgencyId))
                                    {
                                        precioreferencialminorista = pbodega.Precio1Minorista.precio;
                                    }
                                }
                                if (pbodega.Precio2Minorista != null)
                                {
                                    if (pbodega.Precio2Minorista.AgencyPrecioRefMinoristas.Any(x => x.AgencyId == agency.AgencyId))
                                    {
                                        precioreferencialminorista = pbodega.Precio2Minorista.precio;
                                    }
                                }
                                if (pbodega.Precio3Minorista != null)
                                {
                                    if (pbodega.Precio3Minorista.AgencyPrecioRefMinoristas.Any(x => x.AgencyId == agency.AgencyId))
                                    {
                                        precioreferencialminorista = pbodega.Precio3Minorista.precio;
                                    }
                                }
                                order.costoMayorista += (precioreferencialminorista == 0 ? (decimal)pbodega.PrecioVentaReferencial : precioreferencialminorista) * productquantity;
                                order.costoDeProveedor += (decimal)pbodega.PrecioCompraReferencial * productquantity;
                            }
                            else
                            {
                                order.costoMayorista += (decimal)pbodega.PrecioCompraReferencial * productquantity;
                            }

                            //variables
                            SumaCantidad += productquantity;
                            var tipo = productname;
                            var color = "";
                            var tallamarca = "";
                            var descripcion = pbodega.Descripcion;

                            //Proceso de crear un producto
                            PackageItem packageItem = new PackageItem();
                            packageItem.PackageItemId = Guid.NewGuid();
                            packageItem.PackageId = package.PackageId;
                            packageItem.Package = package;
                            packageItem.Qty = productquantity;

                            Product prod = new Product();
                            prod.ProductId = Guid.NewGuid();
                            prod.Agency = _context.Agency.FirstOrDefault();
                            prod.AgencyId = prod.Agency.AgencyId;
                            prod.Code = _context.Product.Count().ToString();
                            prod.Tipo = tipo;
                            prod.Color = color;
                            prod.TallaMarca = tallamarca;
                            prod.Description = descripcion;

                            if (pbodega != null)
                            {
                                prod.ProductoBodega = pbodega;
                                prod.Wholesaler = pbodega.Proveedor;
                            }

                            _context.Add(prod);
                            packageItem.Product = prod;
                            packageItem.ProductId = prod.ProductId;
                            packageItem.Description = descripcion;

                            package.PackageItem.Add(packageItem);

                            _context.Add(packageItem);
                            //Fin de crear producto

                            //Bolsa
                            var bagItem = new BagItem();
                            bagItem.BagItemId = Guid.NewGuid();
                            bagItem.BagId = bag.BagId;

                            bagItem.ProductId = prod.ProductId;
                            bagItem.Qty = productquantity;
                            _context.BagItem.Add(bagItem);
                            bag.BagItems.Add(bagItem);

                            count++;
                        }

                        //creando QR
                        QRWrite qr = new QRWrite(_env);
                        var text = new
                        {
                            codeBolse = code,
                            countArticles = SumaCantidad
                        };
                        var t = text.ToString();
                        var qrpath = qr.ShowQR(code + ".jpg", text.ToString());

                        order.Bag.Add(bag);
                        #endregion

                        #region Order


                        order.OrderId = Guid.NewGuid();
                        order.Agency = agency;
                        order.AgencyId = agency.AgencyId;
                        order.Office = office;
                        order.OfficeId = office.OfficeId;
                        order.User = user;
                        order.UserId = order.User.UserId;
                        order.Date = DateTime.Now;
                        order.Type = "Combo";
                        order.Number = ordernumber;
                        order.NoOrden = "";
                        order.ClientId = newClient.ClientId;
                        order.Client = newClient;
                        order.ContactId = newContacto.ContactId;
                        order.Contact = newContacto;
                        order.cantidad = 1; //Ya no se toma de la vista porque los productos se cargan de la bodegas
                        order.express = false;
                        order.addCosto = 0;
                        order.addPrecio = 0;
                        order.iswebapi = true;
                        order.fechadespacho = DateTime.Now;
                        order.fechaEntrega = DateTime.Now;
                        order.order_key = value.SelectToken("order_key").Value<string>();
                        order.CantLb = 1;
                        order.PriceLb = priceProduct;
                        order.OtrosCostos = (decimal)0.9;
                        order.ValorPagado = value.SelectToken("total").Value<decimal>();
                        order.ValorAduanal = 0;
                        order.Amount = value.SelectToken("total").Value<decimal>();
                        order.Balance = 0;
                        order.Status = Order.STATUS_INICIADA;

                        var re = new RegistroEstado
                        {
                            Estado = order.Status,
                            Date = DateTime.Now,
                            User = order.User
                        };
                        var tipopago = _context.TipoPago.FirstOrDefault(y => y.Type == "Web");
                        RegistroPago pago = new RegistroPago()
                        {
                            number = "PAY" + order.Date.ToString("yyyyMMddHHmmss"),
                            Agency = order.Agency,
                            AgencyId = order.Agency.AgencyId,
                            Office = order.Office,
                            Order = order,
                            Client = order.Client,
                            tipoPago = tipopago,
                            valorPagado = order.ValorPagado,
                            date = DateTime.UtcNow,
                            User = order.User, // VER
                            nota = ""
                        };
                        order.Pagos = new List<RegistroPago>();
                        order.Pagos.Add(pago);
                        order.TipoPago = tipopago;
                        order.RegistroEstados = new List<RegistroEstado>();
                        order.RegistroEstados.Add(re);

                        order.Nota = value.SelectToken("customer_note").Value<string>();

                        _context.Add(order);

                        //Mayorista
                        if (order.agencyTransferida != null)
                        {
                            //Creo el servicio por cobrar
                            servicioxCobrar tramitexPagar = new servicioxCobrar();
                            tramitexPagar.servicioxCobrarId = Guid.NewGuid();
                            tramitexPagar.date = DateTime.UtcNow;
                            tramitexPagar.ServicioId = order.OrderId;
                            tramitexPagar.tramite = "Combos";
                            tramitexPagar.NoServicio = order.Number;
                            tramitexPagar.mayorista = order.agencyTransferida;
                            tramitexPagar.minorista = order.Agency;
                            tramitexPagar.No_servicioxCobrar = "PAYT" + order.Date.ToString("yyyyMMddHHmmss");
                            tramitexPagar.cobrado = 0;
                            tramitexPagar.remitente = order.Client;
                            tramitexPagar.destinatario = order.Contact;
                            tramitexPagar.valorTramite = order.PriceLb; //Este es el precio de los combos
                            tramitexPagar.importeACobrar = order.costoMayorista + order.OtrosCostos;
                            _context.servicioxCobrar.Add(tramitexPagar);
                        }

                        //Creo el servicio por pagar
                        var porPagar = new ServiciosxPagar
                        {
                            Date = DateTime.Now,
                            ImporteAPagar = order.costoMayorista + (decimal)(order.agencyTransferida != null ? order.OtrosCostos : 0),
                            Mayorista = order.wholesaler,
                            Agency = agency,
                            SId = order.OrderId,
                            Order = order,
                            NoServicio = order.Number,
                            Tipo = STipo.Paquete,
                            SubTipo = combos,
                            Express = order.express
                        };
                        _context.ServiciosxPagar.Add(porPagar);


                        //Para AgenciApp
                        if (order.agencyTransferida != null) //Si el tramite es transferido el costo por agencia pasa al mayorista
                        {
                            var s = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = order.OtrosCostos,
                                Mayorista = null,
                                Agency = order.agencyTransferida,
                                SId = order.OrderId,
                                Order = order,
                                NoServicio = order.Number,
                                Tipo = STipo.Paquete,
                                SubTipo = order.Type
                            };
                            _context.ServiciosxPagar.Add(s);
                        }
                        else
                        {
                            var s = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = order.OtrosCostos,
                                Mayorista = null,
                                Agency = agency,
                                SId = order.OrderId,
                                Order = order,
                                NoServicio = order.Number,
                                Tipo = STipo.Paquete,
                                SubTipo = order.Type
                            };
                            _context.ServiciosxPagar.Add(s);
                        }
                        #endregion
                        //Guardo que empleado realizo el tramite
                        TramiteEmpleado tramite = new TramiteEmpleado();
                        tramite.fecha = DateTime.UtcNow;
                        tramite.Id = Guid.NewGuid();
                        tramite.IdEmpleado = user.UserId;
                        tramite.IdTramite = order.OrderId;
                        tramite.tipoTramite = TramiteEmpleado.tipo_ENVIO;
                        tramite.IdAgency = agency.AgencyId;
                        _context.TramiteEmpleado.Add(tramite);

                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return BadRequest("The employee does not exist.");
                    }

                }
                
                return Ok("Success");
            }
            catch(Exception e)
            {
                return NotFound(e.Message);
            }
            
        }

    }
}