using Agenciapp.Common.Class;
using Agenciapp.Common.Contrains;
using Agenciapp.Service.IAirShippingServices.Models;
using Agenciapp.Service.IBodegaServices.Models;
using Agenciapp.Service.ZelleServices;
using AgenciappHome.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Agenciapp.Service.IBodegaServices;
using Agenciapp.Common.Services.INotificationServices;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.Extensions.Configuration;
using Agenciapp.ApiClient.DCuba;
using Agenciapp.Common.Services;
using NPOI.SS.Formula.Functions;
using Address = AgenciappHome.Models.Address;
using Agenciapp.Domain.Models;
using Agenciapp.Service.IClientServices;

namespace Agenciapp.Service.IAirShippingServices
{
    public interface IAirShippingService
    {
        Task<Order> Create(CreateAirShipping model);
        Task AddAttachments(AddAttachmentsModel model);
        Task<Order> CreateManualOrder(User user, CreateManualOrder model);
    }

    public class AirShippingService : IAirShippingService
    {
        private readonly CultureInfo _culture;
        private readonly databaseContext _context;
        private readonly ILogger<AirShippingService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IBodegaService _bodegaService;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly IDCubaApiClient _dcClient;
        private readonly IWorkContext _workContext;
        private readonly IClientService _clientService;


        public AirShippingService(databaseContext context, 
            ILogger<AirShippingService> logger, 
            IWebHostEnvironment env, 
            IBodegaService bodegaService, 
            INotificationService notificationService,
            IConfiguration configuration,
            IDCubaApiClient dCubaApiClient,
            IWorkContext workContext,
            IClientService clientService)
        {
            _context = context;
            _logger = logger;
            _env = env;
            _bodegaService = bodegaService;
            _notificationService = notificationService;
            _configuration = configuration;
            _dcClient = dCubaApiClient;
            _workContext = workContext;
            _clientService = clientService;

            _culture = new CultureInfo("en-US", true);
        }

        public async Task<Order> Create(CreateAirShipping model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    bool existOrder = _context.Order.Any(x => x.Number == model.Number);
                    if (existOrder) throw new Exception($"El trámite con número {model.Number} ya se ha creado, verifique su trámite en el listado de órdenes.");

                    var agency = await _context.Agency.FindAsync(model.AgencyId);
                    var office = await _context.Office.FindAsync(model.OfficeId);
                    var user = await _context.User.FindAsync(model.UserId);
                    var client = await _context.Client.Include(x => x.Creditos).FirstOrDefaultAsync(x => x.ClientId == model.ClientId);
                    var contact = await _context.Contact.FindAsync(model.ContactId);
                    var wholesaler = await _context.Wholesalers.FirstOrDefaultAsync(x => x.IdWholesaler == model.WholesalerId);
                    Agency agencyTransferred = null;
                    if (wholesaler != null && wholesaler.byTransferencia)
                    {
                        var transferencia = _context.CostoxModuloMayorista
                            .Where(x => x.AgencyId == agency.AgencyId && x.modAsignados
                            .Where(y => y.IdWholesaler == wholesaler.IdWholesaler).Any())
                            .FirstOrDefault();

                        agencyTransferred = await _context.Agency.FindAsync(transferencia.AgencyMayoristaId);
                    }

                    Order order = new Order()
                    {
                        OrderId = Guid.NewGuid(),
                        Agency = agency,
                        agencyTransferida = agencyTransferred,
                        Office = office,
                        User = user,
                        Date = DateTime.Now,
                        Type = model.Type,
                        Number = model.Number,
                        NoOrden = model.NoOrden,
                        Client = client,
                        Contact = contact,
                        cantidad = 1,
                        express = model.Express,
                        Delivery = model.Delivery,
                        addCosto = model.AddCosto,
                        addPrecio = model.AddPrecio,
                        AditionalCharge = model.AditionalCharge,
                        PriceLbMedicina = model.PriceLbMedicina,
                        CantLbMedicina = model.CantLbMedicina,
                        ProductsShipping = model.ProductsShipping,
                        CantLb = model.CantLb,
                        PriceLb = model.PriceLb,
                        OtrosCostos = model.OtherCosts,
                        CustomsTax = model.CustomsTax,
                        Amount = model.Amount,
                        Balance = model.Balance,
                        Status = string.IsNullOrEmpty(model.Status) ? Order.STATUS_INICIADA : model.Status,
                        RegistroEstados = new List<RegistroEstado>(),
                        Nota = model.Note,
                        wholesaler = wholesaler,
                        credito = model.Credit >= model.Amount ? model.Amount : model.Credit,
                        Pagos = new List<RegistroPago>(),
                        costoMayorista = model.CostoMayorista,
                        PrincipalDistributorId = model.PrincipalDistributorId,
                        ReceivedDate = model.ReceivedDate
                    };
                    _context.Order.Add(order);

                    #region Relation Client - Contact
                    if (!_context.ClientContact.Where(cc => cc.ClientId == order.Client.ClientId && cc.ContactId == order.Contact.ContactId).Any())
                    {
                        ClientContact c_c = new ClientContact()
                        {
                            CCId = Guid.NewGuid(),
                            ClientId = model.ClientId,
                            ContactId = model.ContactId
                        };
                        _context.Add(c_c);
                    }
                    #endregion

                    #region Productos
                    Package package = new Package()
                    {
                        PackageId = Guid.NewGuid(),
                        PackageNavigation = order
                    };
                    order.Package = package;
                    _context.Package.Add(package);

                    //if (model.Bags.Count == 0 && order.Type != "Combo" && order.Type != "Tienda") throw new Exception("La órden debe tener al menos una bolsa");

                    var bagCount = BigInteger.Parse(order.Agency.BagCount);
                    if (string.IsNullOrEmpty(order.Agency.AgencyNumber)) throw new Exception("No se ha configurado el número de la agencia");

                    int productCount = _context.Product.Count();
                    // Crear bolsas
                    for (int i = 0; i < model.Bags.Count; i++)
                    {
                        var bagModel = model.Bags[i];
                        string code = $"BL{DateTime.Now.ToString("yyyyMMddHHmmss")}{i}";
                        if (order.Type != "Combo")
                        {
                            string numberAux = bagCount > 999999 ? bagCount.ToString() : bagCount.ToString("000000");
                            code = $"BL{order.Agency.AgencyNumber}{numberAux}";
                        }

                        Bag bag = new Bag()
                        {
                            BagId = Guid.NewGuid(),
                            Code = code,
                            OrderId = order.OrderId
                        };
                        _context.Bag.Add(bag);
                        bagCount++;

                        //Crear productos
                        for (int j = 0; j < bagModel.Products.Count; j++)
                        {
                            var p = model.Bags[i].Products[j];

                            ProductoBodega pbodega = null;
                            if (p.IdWineryProduct != null)
                            {
                                pbodega = await _context.ProductosBodegas.Include(x => x.Proveedor).FirstOrDefaultAsync(x => x.IdProducto == p.IdWineryProduct);
                            }

                            Product product = new Product()
                            {
                                ProductId = Guid.NewGuid(),
                                Agency = order.Agency,
                                Code = productCount.ToString(),
                                Tipo = p.Type,
                                Color = p.Color,
                                TallaMarca = p.Size,
                                Description = p.Description,
                                ProductoBodega = pbodega,
                                Wholesaler = pbodega?.Proveedor
                            };
                            _context.Product.Add(product);

                            PackageItem packageItem = new PackageItem()
                            {
                                PackageItemId = Guid.NewGuid(),
                                Package = package,
                                Qty = p.Quantity,
                                Product = product,
                                Description = p.Description
                            };
                            _context.PackageItem.Add(packageItem);

                            package.PackageItem.Add(packageItem);

                            productCount++;

                            var bagItem = new BagItem()
                            {
                                BagItemId = Guid.NewGuid(),
                                Bag = bag,
                                Product = product,
                                Qty = p.Quantity,
                                Order = j,
                            };

                            _context.BagItem.Add(bagItem);
                        }

                        //Creando QR
                        QRWrite qr = new QRWrite(_env);
                        var text = new
                        {
                            codeBolse = code,
                            countArticles = bagModel.Products.Sum(x => x.Quantity)
                        };
                        var t = text.ToString();
                        if (order.Type != "Combo")
                            qr.ShowQR(code + ".jpg", text.ToString());
                    }

                    if (order.Type != "Combo")
                        order.Agency.BagCount = bagCount.ToString();
                    #endregion

                    #region Pagos
                    var paymentTypes = _context.TipoPago.ToList();
                    for (int i = 0; i < model.Pays.Count; i++)
                    {
                        var pay = model.Pays[i];
                        if(pay.Paid > 0)
                        {
                            var paymentType = paymentTypes.FirstOrDefault(x => x.Type == pay.Type) ?? throw new Exception($"El tipo de pago {pay.Type} no existe.");

                            RegistroPago pago = new RegistroPago
                            {
                                RegistroPagoId = Guid.NewGuid(),
                                Agency = order.Agency,
                                date = DateTime.UtcNow,
                                number = "PAY" + order.Date.ToString("yyyyMMddHHmmss") + i,
                                Office = order.Office,
                                Order = order,
                                tipoPago = paymentType,
                                User = order.User,
                                valorPagado = pay.Paid,
                                Client = order.Client,
                                nota = pay.Note,
                            };
                            order.Pagos.Add(pago);

                            //Si tipo pago es zelle verifico si existe un pago zelle no asignado que coincida con el codigo de referencia
                            if (paymentType.Type == "Zelle")
                            {
                                var zelle = _context.ZelleItems.FirstOrDefault(x => x.Code == pay.Note && x.AgencyId == order.Agency.AgencyId && x.Type == STipo.None);
                                if (zelle != null)
                                {
                                    await ZelleService.Asociate(_context, zelle.ZellItemId, order.OrderId, STipo.Paquete, order.Agency.AgencyId);
                                    pago.ZelleItemId = zelle.ZellItemId;
                                }
                            }

                            order.ValorPagado += pago.valorPagado;

                            // AuthorizationCard
                            if (pago.tipoPago.Type == "Crédito o Débito" && model.AuthorizationCard != null)
                            {
                                var authCard = new AuthorizationCard()
                                {
                                    Id = Guid.NewGuid(),
                                    addressOfSend = model.AuthorizationCard.AddressOfSend,
                                    CardCreditEnding = model.AuthorizationCard.CardCreditEnding,
                                    CCV = model.AuthorizationCard.CCV,
                                    ConvCharge = model.AuthorizationCard.ConvCharge,
                                    Date = DateTime.UtcNow,
                                    email = model.AuthorizationCard.Email,
                                    ExpDate = model.AuthorizationCard.ExpDate ?? DateTime.UtcNow,
                                    OwnerAddressDiferent = model.AuthorizationCard.OwnerAddressDiferent,
                                    phone = model.AuthorizationCard.Phone,
                                    saleAmount = model.AuthorizationCard.SaleAmount,
                                    TotalCharge = model.AuthorizationCard.TotalCharge,
                                    typeCard = model.AuthorizationCard.TypeCard,
                                };
                                _context.AuthorizationCards.Add(authCard);
                                pago.AuthorizationCard = authCard;
                                order.authorizationCard = authCard;
                            }
                        }
                    }

                    if (order.credito > 0)
                    {
                        RegistroPago pagocredito = new RegistroPago
                        {
                            AgencyId = order.Agency.AgencyId,
                            date = DateTime.UtcNow,
                            number = "PAY" + order.Date.ToString("yyyyMMddHHmmss") + order.Pagos.Count + 1,
                            OfficeId = order.Agency.AgencyId,
                            OrderId = order.OrderId,
                            tipoPagoId = paymentTypes.FirstOrDefault(x => x.Type == "Crédito de Consumo").TipoPagoId,
                            UserId = order.User.UserId,
                            valorPagado = order.credito,
                            RegistroPagoId = Guid.NewGuid(),
                            ClientId = order.Client.ClientId,
                            nota = "",
                        };
                        order.Pagos.Add(pagocredito);

                        var credit = order.credito;
                        foreach (var item in order.Client.Creditos)
                        {
                            if (item.value > credit)
                            {
                                item.value -= credit;
                                _context.Credito.Update(item);
                                break;
                            }
                            else
                            {
                                credit -= item.value;
                                _context.Credito.Remove(item);
                            }
                        }
                    }

                    order.TipoPago = order.Pagos.FirstOrDefault()?.tipoPago ?? paymentTypes.FirstOrDefault(x => x.Type == "Cash");
                    #endregion

                    #region Cuenta por cobrar balance pendiente 

                    if ((order.Type != "Combo" && model.RetaiId != null) || (order.Type == "Combo" && order.agencyTransferida == null))
                    {
                        order.Minorista = _context.Minoristas.FirstOrDefault(x => x.Agency == order.Agency && x.Id == model.RetaiId);
                    }

                    if (order.Balance != 0)
                    {
                        if (order.Type == "Combo")
                        {
                            _context.servicioxCobrar.Add(new servicioxCobrar()
                            {
                                servicioxCobrarId = Guid.NewGuid(),
                                date = DateTime.UtcNow,
                                ServicioId = order.OrderId,
                                tramite = "Combos",
                                NoServicio = order.Number,
                                cliente = order.Client,
                                No_servicioxCobrar = "PAYT" + order.Date.ToString("yyyyMMddHHmmss"),
                                cobrado = 0,
                                remitente = order.Client,
                                destinatario = order.Contact,
                                valorTramite = order.Balance,
                                importeACobrar = order.Balance,
                                mayorista = order.Agency,
                                Order = order
                            });
                        }
                        else
                        {
                            if (order.Type == "Tienda") order.Status = Order.STATUS_PENDIENTE;

                            _context.servicioxCobrar.Add(new servicioxCobrar
                            {
                                servicioxCobrarId = Guid.NewGuid(),
                                date = DateTime.UtcNow,
                                ServicioId = order.OrderId,
                                tramite = "Paquete Aereo",
                                NoServicio = order.Number,
                                cliente = order.Client,
                                No_servicioxCobrar = "PAYT" + order.Date.ToString("yyyyMMddHHmmss"),
                                cobrado = 0,
                                remitente = order.Client,
                                destinatario = order.Contact,
                                valorTramite = order.Balance,
                                importeACobrar = order.Balance,
                                mayorista = order.Agency,
                                Order = order,
                                MinoristaTramite = order.Minorista
                            });
                        }
                    }
                    #endregion

                    order.RegistroEstados.Add(new RegistroEstado()
                    {
                        Estado = order.Status,
                        Date = DateTime.Now,
                        User = order.User
                    });

                    #region Registrar empleado que creo el tramite
                    TramiteEmpleado tramiteEmpleado = new TramiteEmpleado()
                    {
                        fecha = DateTime.UtcNow,
                        IdEmpleado = order.User.UserId,
                        IdTramite = order.OrderId,
                        tipoTramite = TramiteEmpleado.tipo_ENVIO,
                        IdAgency = order.Agency.AgencyId
                    };
                    _context.TramiteEmpleado.Add(tramiteEmpleado);
                    #endregion

                    _context.Logs.Add(new AgenciappHome.Models.Log
                    {
                        Date = DateTime.Now,
                        Event = LogEvent.Crear,
                        Type = order.Type == "Combo" ? LogType.Combo : LogType.Orden,
                        LogId = Guid.NewGuid(),
                        Message = order.Number,
                        User = order.User,
                        Client = order.Client,
                        Precio = order.Amount.ToString(),
                        Pagado = order.Pagos.Sum(x => x.valorPagado).ToString(),
                        AgencyId = order.AgencyId,
                        Order = order
                    });

                    #region Guardar adjunto
                    if (!string.IsNullOrEmpty(model.Attachment))
                    {
                        string attach = model.Attachment.Substring(model.Attachment.IndexOf(',') + 1);
                        string path = $"{_env.WebRootPath}{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}Orders{Path.DirectorySeparatorChar}Attachment";
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        string fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}.png";
                        byte[] imageBytes = Convert.FromBase64String(attach);
                        System.IO.File.WriteAllBytes(Path.Combine(path, fileName), imageBytes);

                        order.FileNameAttachment = fileName;
                    }
                    #endregion

                    if (order.wholesaler?.name == "Dimelos-Envios")
                    {
                        order.NumberWholesaler = await CreateOrderWholesaler(order);
                        if (string.IsNullOrEmpty(order.NumberWholesaler)) throw new Exception("No se ha podido crear el trámite.");
                    }

                    // Guardo los cambios en la bd
                    await _context.SaveChangesAsync();

                    #region Extraer los productos de la bodega
                    var productsToExtract = order.Package.PackageItem
                        .Where(x => x.Product.ProductoBodega != null)
                        .Select(x => new ExtractProductModel()
                        {
                            Product = x.Product.ProductoBodega,
                            Qty = (int)x.Qty
                        }).ToList();

                    if (productsToExtract.Any())
                    {
                        var extract = _bodegaService.Extract(productsToExtract);
                        if (extract.IsFailure)
                        {
                            throw new Exception("No se han podido extraer los productos de la bodega");
                        }
                    }
                    #endregion

                    transaction.Commit();

                    await  CreateOrderBackground(order);

                    return order;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    _logger.LogError(e, e.Message); 
                    throw e;
                }
                
            }
        }

        public async Task SendComprobante(Guid userId, Guid orderId, string email)
        {
            var aUser = _context.User.Find(userId);
            var aAgency = _context.Agency.Find(aUser.AgencyId);

            if (email != null && email != "")
            {

                var order = _context.Order.Find(orderId);

                string attach = CreateOrderComprobante(userId, orderId);

                string title = "Comprobante de Pago";
                string msg = "Comprobante de Pago de la Orden No. " + order.Number;

                var emailResponse = await _notificationService.sendEmail(
                            new SendGrid.Helpers.Mail.EmailAddress { Email = "do_not_reply@agenciapp.com", Name = aAgency.Name },
                            new SendGrid.Helpers.Mail.EmailAddress
                            {
                                Email = email
                            }, title, msg, new List<SendGrid.Helpers.Mail.Attachment>()
                            {
                            new SendGrid.Helpers.Mail.Attachment
                            {
                                Content = attach,
                                Filename = "document.pdf"
                            }
                        }, false);

                RegistroEnvioEmails registro = new RegistroEnvioEmails
                {
                    RegistroEnvioEmailsId = Guid.NewGuid(),
                    AgencyId = aAgency.AgencyId,
                    AgencyName = aAgency.Name,
                    fecha = DateTime.Now,
                    destinatario = email,
                    descripción = "Comprobante de Pago Order",
                    status = emailResponse.IsSuccess ? emailResponse.Value : emailResponse.Error
                };
                _context.RegistroEnvioEmails.Add(registro);
                await _context.SaveChangesAsync();
            }
        }

        public string CreateOrderComprobante(Guid userId,Guid id)
        {
            using (MemoryStream MStream = new MemoryStream())
            {
                Order order = _context.Order
                    .Include(x => x.Agency)
                    .Include(x => x.Bag)
                    .Include(x => x.User)
                    .Include(x => x.wholesaler)
                    .Include(o => o.Pagos).ThenInclude(x => x.tipoPago)
                    .Where(o => o.OrderId == id).FirstOrDefault();
                iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.LETTER);
                PdfWriter writer = PdfWriter.GetInstance(doc, MStream);

                doc.Open();

                iTextSharp.text.Font agencyFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                iTextSharp.text.Font underLineFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                iTextSharp.text.Font underLineBag = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                iTextSharp.text.Font headFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                iTextSharp.text.Font headFontRed = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.RED);
                iTextSharp.text.Font normalFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                iTextSharp.text.Font normalFontRed = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.RED);
                iTextSharp.text.Font headFontExpress = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.RED);

                try
                {
                    // VER AGENCIA
                    var aUser = _context.User.Find(userId);
                    var agency = _context.Agency.Where(x => x.AgencyId == aUser.AgencyId).FirstOrDefault();
                    AgenciappHome.Models.Address agencyAddress = _context.Address.Where(a => a.ReferenceId == agency.AgencyId).FirstOrDefault();
                    Phone agencyPhone = _context.Phone.Where(p => p.ReferenceId == agency.AgencyId).FirstOrDefault();

                    // Logo de la agencia
                    float[] columnWidths = { 5, 1 };
                    PdfPTable tableEncabezado = new PdfPTable(columnWidths);
                    tableEncabezado.WidthPercentage = 100;
                    PdfPCell celllogo = new PdfPCell();
                    celllogo.BorderWidth = 0;
                    PdfPCell cellAgency = new PdfPCell();
                    cellAgency.BorderWidth = 0;

                    string sWebRootFolder = _env.WebRootPath;
                    if (agency.logoName != null)
                    {
                        string namelogo = agency.logoName;
                        string filePathQR = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "LogosAgency";
                        filePathQR = Path.Combine(filePathQR, namelogo);
                        iTextSharp.text.Image imagelogo = iTextSharp.text.Image.GetInstance(filePathQR);
                        imagelogo.ScaleAbsolute(75, 75);
                        celllogo.AddElement(imagelogo);
                    }

                    if (order.express)
                    {
                        cellAgency.AddElement(new Phrase("EXPRESS", headFontExpress));
                    }
                    if (order.AgencyId == Guid.Parse("109F29C4-6E4B-4AD3-8690-40BBE945EE53"))
                    {
                        cellAgency.AddElement(new Phrase("Cafe Envios", agencyFont)); // Nombre de la empresa
                        cellAgency.AddElement(new Phrase(agencyAddress.AddressLine1.ToUpper(), normalFont)); // Dirección de la empresa
                        Phrase telefono = new Phrase("Teléfono: ", headFont);
                        telefono.AddSpecial(new Phrase("786-535-4905", normalFont));
                        cellAgency.AddElement(telefono);
                    }
                    else
                    {
                        cellAgency.AddElement(new Phrase(agency.Name.ToUpper(), agencyFont)); // Nombre de la empresa
                        cellAgency.AddElement(new Phrase(agencyAddress.AddressLine1.ToUpper(), normalFont)); // Dirección de la empresa
                        Phrase telefono = new Phrase("Teléfono: ", headFont);
                        telefono.AddSpecial(new Phrase(agencyPhone.Number, normalFont));
                        cellAgency.AddElement(telefono);
                    }

                    Phrase fax = new Phrase("Fax: ", headFont);
                    fax.AddSpecial(new Phrase("", normalFont));
                    cellAgency.AddElement(fax);

                    Phrase email = new Phrase(new Phrase("", normalFont));
                    email.AddSpecial(new Phrase("", normalFont));
                    cellAgency.AddElement(email);

                    tableEncabezado.AddCell(cellAgency);
                    tableEncabezado.AddCell(celllogo);

                    doc.Add(tableEncabezado);


                    iTextSharp.text.Font line = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.UNDERLINE, BaseColor.BLACK);
                    Paragraph parPaq = new Paragraph("COMPROBANTE DE PAGO", line);
                    parPaq.Alignment = Element.ALIGN_CENTER;
                    doc.Add(parPaq);
                    doc.Add(Chunk.NEWLINE);

                    float[] column = { 2, 2 };
                    PdfPTable tabledoc = new PdfPTable(column);
                    tableEncabezado.WidthPercentage = 100;
                    PdfPCell celleft = new PdfPCell();
                    celleft.BorderWidth = 0;
                    PdfPCell cellright = new PdfPCell();
                    cellright.BorderWidth = 0;

                    Phrase aux = new Phrase("Factura No: ", headFont);
                    aux.AddSpecial(new Phrase(order.Number, normalFont));
                    celleft.AddElement(aux);

                    aux = new Phrase("Dirección Oficina: ", headFont);
                    aux.AddSpecial(new Phrase(agencyAddress.AddressLine1.ToUpper(), normalFont));
                    celleft.AddElement(aux);

                    aux = new Phrase("Teléfono (Oficina): ", headFont);
                    aux.AddSpecial(new Phrase(agencyPhone.Number, normalFont));
                    celleft.AddElement(aux);

                    aux = new Phrase("Nombre Empleado: ", headFont);
                    aux.AddSpecial(new Phrase(order.User.Name + " " + order.User.LastName, normalFont));
                    celleft.AddElement(aux);

                    celleft.AddElement(Chunk.NEWLINE);

                    order.Client = _context.Client.Where(c => c.ClientId == order.ClientId).FirstOrDefault();
                    Address clientAddress = _context.Address.Where(a => a.ReferenceId == order.Client.ClientId).FirstOrDefault();
                    Phone clientPhone1 = _context.Phone.Where(p => p.ReferenceId == order.Client.ClientId && p.Type == "Móvil").FirstOrDefault();


                    aux = new Phrase("Datos Cliente", underLineFont);
                    celleft.AddElement(aux);

                    aux = new Phrase("Cliente No: ", headFont);
                    aux.AddSpecial(new Phrase(order.Client.ClientNumber, normalFont));
                    celleft.AddElement(aux);

                    aux = new Phrase("Nombre Cliente: ", headFont);
                    aux.AddSpecial(new Phrase(order.Client.Name.ToUpper() + " " + order.Client.LastName.ToUpper(), normalFont));
                    celleft.AddElement(aux);

                    aux = new Phrase("Teléfono: ", headFont);
                    aux.AddSpecial(new Phrase(clientPhone1.Number, normalFont));
                    celleft.AddElement(aux);

                    aux = new Phrase("Dirección: ", headFont);
                    aux.AddSpecial(new Phrase(clientAddress.AddressLine1.ToUpper() + ", " + clientAddress.City.ToUpper(), normalFont));
                    celleft.AddElement(aux);

                    celleft.AddElement(Chunk.NEWLINE);

                    aux = new Phrase("Datos Contacto", underLineFont);
                    cellright.AddElement(aux);

                    order.Contact = _context.Contact.Where(c => c.ContactId == order.ContactId).FirstOrDefault();
                    Address contactAddress = _context.Address.Where(a => a.ReferenceId == order.Contact.ContactId).FirstOrDefault();
                    Phone contactPhone1 = _context.Phone.Where(p => p.ReferenceId == order.Contact.ContactId && p.Type == "Móvil").FirstOrDefault();
                    Phone contactPhone2 = _context.Phone.Where(p => p.ReferenceId == order.Contact.ContactId && p.Type == "Casa").FirstOrDefault();


                    aux = new Phrase("Contacto No: ", headFont);
                    aux.AddSpecial(new Phrase(order.Contact.ContactNumber, normalFont));
                    cellright.AddElement(aux);

                    aux = new Phrase("Nombre Contacto: ", headFont);
                    aux.AddSpecial(new Phrase(order.Contact.Name.ToUpper() + " " + order.Contact.LastName.ToUpper(), normalFont));
                    cellright.AddElement(aux);

                    if (order.Contact.CI != null)
                    {
                        aux = new Phrase("Carnet de Identidad: ", headFont);
                        aux.AddSpecial(new Phrase(order.Contact.CI.ToUpper(), normalFont));
                        cellright.AddElement(aux);
                    }

                    if (contactPhone1 != null)
                    {
                        aux = new Phrase("Teléfono Fijo: ", headFont);
                        aux.AddSpecial(new Phrase(contactPhone1.Number, normalFont));
                        cellright.AddElement(aux);
                    }
                    if (contactPhone2 != null)
                    {
                        aux = new Phrase("Teléfono Móvil: ", headFont);
                        aux.AddSpecial(new Phrase(contactPhone2.Number, normalFont));
                        cellright.AddElement(aux);
                    }
                    aux = new Phrase("Otro teléfono: ", headFont);
                    cellright.AddElement(aux);

                    aux = new Phrase("Dirección: ", headFont);
                    aux.AddSpecial(new Phrase(contactAddress.AddressLine1, normalFont));
                    cellright.AddElement(aux);
                    aux = new Phrase("Provincia: ", headFont);
                    aux.AddSpecial(new Phrase(contactAddress.City, normalFont));
                    cellright.AddElement(aux);
                    aux = new Phrase("Municipio: ", headFont);
                    aux.AddSpecial(new Phrase(contactAddress.State, normalFont));
                    cellright.AddElement(aux);
                    aux = new Phrase("Reparto: ", headFont);
                    aux.AddSpecial(new Phrase(contactAddress.Zip, normalFont));
                    cellright.AddElement(aux);

                    tabledoc.AddCell(celleft);
                    tabledoc.AddCell(cellright);

                    celleft = new PdfPCell();
                    celleft.BorderWidth = 0;
                    cellright = new PdfPCell();
                    cellright.BorderWidth = 0;

                    aux = new Phrase("Datos Pago", underLineFont);
                    celleft.AddElement(aux);

                    aux = new Phrase("Tipo de Envío: ", headFont);
                    aux.AddSpecial(new Phrase(order.Type, normalFont));
                    celleft.AddElement(aux);

                    if (order.Type == "Tienda")
                    {
                        aux = new Phrase("Número de Orden: ", headFont);
                        aux.AddSpecial(new Phrase(order.NoOrden, normalFont));
                        celleft.AddElement(aux);
                    }

                    if (order.Pagos.Any())
                    {
                        aux = new Phrase("Pagos: ", headFont);
                        celleft.AddElement(aux);
                        foreach (var item in order.Pagos)
                        {
                            aux = new Phrase($"  -{item.tipoPago.Type}: ${item.valorPagado}", normalFont);
                            celleft.AddElement(aux);
                        }
                    }
                    if (order.Type != "Combo")
                    {
                        aux = new Phrase("Valores Aduanales: ", headFont);
                        aux.AddSpecial(new Phrase(order.CustomsTax.ToString(), normalFont));
                        celleft.AddElement(aux);
                    }
                    else
                    {
                        aux = new Phrase("Cantidad: ", headFont);
                        aux.AddSpecial(new Phrase(order.cantidad.ToString(), normalFont));
                        celleft.AddElement(aux);
                    }
                    aux = new Phrase("Monto Total: ", headFont);
                    aux.AddSpecial(new Phrase(order.Amount.ToString(), normalFont));
                    celleft.AddElement(aux);

                    aux = new Phrase("Total Pagado: ", headFont);
                    aux.AddSpecial(new Phrase(order.ValorPagado.ToString(), normalFont));
                    celleft.AddElement(aux);

                    aux = new Phrase("Balance: ", headFont);
                    aux.AddSpecial(new Phrase(order.Balance.ToString(), normalFont));
                    celleft.AddElement(aux);

                    aux = new Phrase("Se ha realizado un descuento de: ", headFontRed);
                    aux.AddSpecial(new Phrase(order.addPrecio.ToString(), normalFontRed));
                    celleft.AddElement(aux);
                    List<AuxOrderBolsas> bolsas = new List<AuxOrderBolsas>();
                    foreach (var bag in order.Bag)
                    {
                        AuxOrderBolsas bolsa = new AuxOrderBolsas(bag.BagId, bag.Code, _context);
                        if (order.Type == "Combo")
                        {
                            foreach (var item in bolsa.items)
                            {
                                aux = new Phrase(item.tipo, headFont);
                                cellright.AddElement(aux);
                                aux = new Phrase("Cantidad: ", headFont);
                                aux.AddSpecial(new Phrase(((int)item.cantidad).ToString(), normalFont));
                                cellright.AddElement(aux);
                                Paragraph p = new Paragraph(item.description, normalFont);
                                cellright.AddElement(p);
                                cellright.AddElement(Chunk.NEWLINE);
                            }
                        }
                        else if (order.Type != "Tienda")
                        {
                            aux = new Phrase(bolsa.code, underLineBag);
                            cellright.AddElement(aux);
                            foreach (var item in bolsa.items)
                            {
                                aux = new Phrase($"{item.tipo} ({(int)item.cantidad})", headFont);
                                cellright.AddElement(aux);
                                Paragraph p = new Paragraph(item.description, normalFont);
                                cellright.AddElement(p);

                            }
                        }
                    }
                    tabledoc.AddCell(celleft);
                    tabledoc.AddCell(cellright);
                    doc.Add(tabledoc);


                    doc.Add(Chunk.NEWLINE);


                    doc.Add(Chunk.NEWLINE);
                    doc.Add(Chunk.NEWLINE);

                    if (aUser.firmaname == null || aUser.firmaname == "")
                    {
                        doc.Add(new Paragraph("Firma del cliente: ______________________                    Firma del empleado: ______________________", normalFont));
                        doc.Add(new Paragraph("                                                                                                " + aUser.Name + " " + aUser.LastName, normalFont));
                    }
                    else
                    {
                        //Table de firma del empleado
                        PdfPTable table = new PdfPTable(2);
                        table.HorizontalAlignment = 0; //0=Left, 1=Centre, 2=Right
                        table.SpacingBefore = 0f;
                        table.SpacingAfter = 0f;

                        PdfPCell cell = new PdfPCell();
                        cell.Border = 0;
                        string filenameImg = aUser.firmaname;
                        string filePathImg = sWebRootFolder + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "Firmas";
                        filePathImg = Path.Combine(filePathImg, filenameImg);
                        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(filePathImg);
                        cell.AddElement(image);
                        cell.AddElement(new Paragraph(aUser.Name + " " + aUser.LastName, normalFont));

                        PdfPCell cell2 = new PdfPCell();
                        cell2.Border = 0;
                        cell2.AddElement(new Paragraph("Firma del empleado:", normalFont));

                        table.AddCell(cell2);
                        table.AddCell(cell);

                        //Tabla de firma del cliente
                        PdfPTable table2 = new PdfPTable(1);
                        table2.HorizontalAlignment = 0; //0=Left, 1=Centre, 2=Right
                        table2.SpacingBefore = 0f;
                        table2.SpacingAfter = 0f;
                        PdfPCell cell3 = new PdfPCell();
                        cell3.Border = 0;
                        cell3.AddElement(new Paragraph("Firma del cliente:______________________", normalFont));

                        table2.AddCell(cell3);


                        //Tabla de firmas
                        PdfPTable table1 = new PdfPTable(2);
                        table1.HorizontalAlignment = 0; //0=Left, 1=Centre, 2=Right
                        table1.SpacingBefore = 0f;
                        table1.SpacingAfter = 0f;
                        PdfPCell cell4 = new PdfPCell();
                        cell4.Border = 0;
                        cell4.AddElement(table2);
                        PdfPCell cell5 = new PdfPCell();
                        cell5.Border = 0;
                        cell5.AddElement(table);
                        table1.AddCell(cell4);
                        table1.AddCell(cell5);

                        //Add table to document    
                        doc.Add(table1);

                        if (order.Client.checkNotifications)
                        {
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(Chunk.NEWLINE);
                            doc.Add(new Phrase("[ X ] ¿" + order.Client.Name + " " + order.Client.LastName + " desea recibir información sobre los servicios de la agencia " + agency.LegalName + ", así como alertas y notificaciones del servicio recibido en cuestión por mensaje de texto?", headFont));
                        }
                    }



                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    doc.Close();
                    writer.Close();
                }
                return Convert.ToBase64String(MStream.ToArray());
            }
        }

        private async Task CreateOrderBackground(Order order)
        {
            try
            {
                await AccountsPayableAndReceivable(order);

                if (order.AgencyId == AgencyName.ViajePronto)
                {
                    await SendComprobante(order.User.UserId, order.OrderId, "lazaritocuba69@gmail.com");
                    await SendComprobante(order.User.UserId, order.OrderId, "anabelmh71@gmail.com");
                }

                if (AgencyName.IsDistrictCuba(order.Agency.AgencyId))
                {
                    List<string> allowedWholesalers = (_configuration["DCubaApi:Wholesalers"] ?? string.Empty).Split(",").ToList();
                    string idWholesaler = (order.wholesaler?.IdWholesaler ?? Guid.Empty).ToString().ToLower();
                    if (allowedWholesalers.Contains(idWholesaler))
                    {
                        await _dcClient.Emission(_workContext.Token, order.OrderId);
                    }
                }
            }
            catch(Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        private async Task AccountsPayableAndReceivable(Order order)
        {
            if (order.Type == "Combo")
            {
                string combos = string.Empty;
                var listProduct = order.Package.PackageItem.Where(x => x.Product.ProductoBodega != null);
                if(listProduct.Count() > 0)
                {
                    //Obtengo el mayorista
                    Wholesaler refMayoristaCombo = null;
                    Guid idfirstproduct = listProduct.First().Product.ProductoBodega.IdProducto;
                    ProductoBodega firstproduct = _context.ProductosBodegas.Include(x => x.Proveedor).FirstOrDefault(x => x.IdProducto == idfirstproduct);
                    if (firstproduct.Proveedor != null)
                    {
                        if(order.wholesaler == null) order.wholesaler =  firstproduct.Proveedor;

                        refMayoristaCombo = firstproduct.Proveedor;
                    }
                    else
                    {
                        throw new Exception("Los productos deben poseer un mayorista");
                    }
                    //verifico si existe un mayorista por transferencia para combos
                    bool bytransferencia = order.agencyTransferida != null;

                    decimal pagoMayorista = 0; //Cuando es por tranferencia el mayorista debe pagarle a su proveedor
                    foreach (var item in listProduct)
                    {
                        var producto = _context.ProductosBodegas
                                .Include(x => x.Precio1Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                                .Include(x => x.Precio2Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                                .Include(x => x.Precio3Minorista).ThenInclude(x => x.AgencyPrecioRefMinoristas)
                                .Include(x => x.Proveedor)
                                .FirstOrDefault(x => x.IdProducto == item.Product.ProductoBodega.IdProducto);

                        if (producto.Proveedor != null)
                        {
                            decimal cantidad = item.Qty;
                            combos += producto.Nombre + ", ";
                            if (bytransferencia || order.Minorista != null)
                            {
                                // Si es por transferencia se toma el precioreferencialminorista
                                //Busco el precio a minorista que definio la agencia mayorista 
                                decimal precioreferencialminorista = 0;
                                if (order.Minorista != null)
                                {
                                    if (producto.Precio1Minorista != null)
                                    {
                                        precioreferencialminorista = producto.Precio1Minorista.precio;
                                    }
                                }
                                else
                                {
                                    if (producto.Precio1Minorista != null)
                                    {
                                        if (producto.Precio1Minorista.AgencyPrecioRefMinoristas.Any(x => x.AgencyId == order.Agency.AgencyId))
                                        {
                                            precioreferencialminorista = producto.Precio1Minorista.precio;
                                        }
                                    }
                                    if (producto.Precio2Minorista != null)
                                    {
                                        if (producto.Precio2Minorista.AgencyPrecioRefMinoristas.Any(x => x.AgencyId == order.Agency.AgencyId))
                                        {
                                            precioreferencialminorista = producto.Precio2Minorista.precio;
                                        }
                                    }
                                    if (producto.Precio3Minorista != null)
                                    {
                                        if (producto.Precio3Minorista.AgencyPrecioRefMinoristas.Any(x => x.AgencyId == order.Agency.AgencyId))
                                        {
                                            precioreferencialminorista = producto.Precio3Minorista.precio;
                                        }
                                    }
                                }

                                order.costoMayorista += (precioreferencialminorista == 0 ? (decimal)producto.PrecioVentaReferencial : precioreferencialminorista) * cantidad;
                                order.costoDeProveedor += (decimal)producto.PrecioCompraReferencial * cantidad;
                                pagoMayorista += (decimal)producto.PrecioCompraReferencial * cantidad; //lo que paga el mayorista a su proveedor
                            }
                            else
                            {
                                order.costoMayorista += (decimal)producto.PrecioCompraReferencial * cantidad;
                            }
                        }
                    }
                    //Valor incrementado al costo en caso de los combos
                    order.costoMayorista += order.addCosto;

                    if (bytransferencia || order.Minorista != null)
                    {
                        //Creo el servicio por cobrar
                        servicioxCobrar servXCobrar = new servicioxCobrar();
                        servXCobrar.servicioxCobrarId = Guid.NewGuid();
                        servXCobrar.date = DateTime.UtcNow;
                        servXCobrar.ServicioId = order.OrderId;
                        servXCobrar.tramite = "Combos";
                        servXCobrar.NoServicio = order.Number;
                        servXCobrar.Order = order;
                        if (bytransferencia)
                        {
                            servXCobrar.mayorista = order.agencyTransferida;
                            servXCobrar.minorista = order.Agency;
                        }
                        else if (order.Minorista != null)
                        {
                            servXCobrar.mayorista = order.Agency;
                            servXCobrar.MinoristaTramite = order.Minorista;
                        }
                        servXCobrar.No_servicioxCobrar = "PAYT" + order.Date.ToString("yyyyMMddHHmmss");
                        servXCobrar.cobrado = 0;
                        servXCobrar.remitente = order.Client;
                        servXCobrar.destinatario = order.Contact;
                        servXCobrar.valorTramite = order.PriceLb; //Este es el precio de los combos
                        servXCobrar.importeACobrar = order.costoMayorista + order.OtrosCostos;
                        _context.servicioxCobrar.Add(servXCobrar);

                        //Servicio por pagar del mayorista a su proveedor
                        if (refMayoristaCombo != null && (order.agencyTransferida != null || order.Minorista != null))
                        {
                            var porPagarMayorista = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = pagoMayorista,
                                Mayorista = refMayoristaCombo,
                                Agency = order.agencyTransferida,
                                SId = order.OrderId,
                                Order = order,
                                NoServicio = order.Number,
                                Tipo = STipo.Paquete,
                                SubTipo = combos,
                                Express = order.express
                            };
                            if (order.Minorista != null)
                            {
                                porPagarMayorista.Agency = order.Agency;
                            }
                            _context.ServiciosxPagar.Add(porPagarMayorista);
                        }
                    }

                    if (order.Minorista == null)
                    {
                        //Creo el servicio por pagar
                        var porPagar = new ServiciosxPagar
                        {
                            Date = DateTime.Now,
                            ImporteAPagar = order.costoMayorista + (decimal)(bytransferencia ? order.OtrosCostos : 0),
                            Mayorista = order.wholesaler,
                            Agency = order.Agency,
                            SId = order.OrderId,
                            Order = order,
                            NoServicio = order.Number,
                            Tipo = STipo.Paquete,
                            SubTipo = combos,
                            Express = order.express
                        };
                        _context.ServiciosxPagar.Add(porPagar);
                    }        

                    if (order.ProductsShipping > 0)
                    {
                        var porPagar = new ServiciosxPagar
                        {
                            Date = DateTime.Now,
                            ImporteAPagar = order.ProductsShipping,
                            Order = order,
                            Mayorista = order.wholesaler,
                            Agency = order.Agency,
                            SId = order.OrderId,
                            NoServicio = order.Number,
                            Tipo = STipo.Paquete,
                            SubTipo = "-",
                            IsPaymentProductShipping = true
                        };
                        _context.ServiciosxPagar.Add(porPagar);
                    }
                }
            }
            else
            {
                if (order.wholesaler != null)
                {
                    var wholesaler = _context.Wholesalers
                        .Include(x => x.CostByProvinces).ThenInclude(x => x.Provincia)
                        .FirstOrDefault(x => x.IdWholesaler == order.wholesaler.IdWholesaler);

                    if (order.agencyTransferida != null)
                    {
                        decimal valortramite = order.Amount;
                        if (order.TipoPago.Type == "Crédito o Débito")
                        {
                            //Se le quita el porciento fee
                            //valortramite = order.CantLb * order.PriceLb + order.ValorAduanal + order.OtrosCostos;
                            valortramite = order.CantLb * (order.PriceLb * order.cantidad) + order.CustomsTax;
                        }

                        if (order.Type == "Tienda" && order.agencyTransferida.Name == "Rey Envios")
                        {
                            decimal importeACobrar = order.PriceLb;
                            order.costoDeProveedor = Math.Round((decimal)0.95 * (order.PriceLb - 20), 2);
                            servicioxCobrar servXCobrar = new servicioxCobrar();
                            servXCobrar.servicioxCobrarId = Guid.NewGuid();
                            servXCobrar.date = DateTime.UtcNow;
                            servXCobrar.ServicioId = order.OrderId;
                            servXCobrar.tramite = "Tienda";
                            servXCobrar.NoServicio = order.Number;
                            servXCobrar.mayorista = order.agencyTransferida;
                            servXCobrar.minorista = order.Agency;
                            servXCobrar.No_servicioxCobrar = "PAYT" + order.Date.ToString("yyyyMMddHHmmss");
                            servXCobrar.cobrado = 0;
                            servXCobrar.remitente = order.Client;
                            servXCobrar.destinatario = order.Contact;
                            servXCobrar.valorTramite = valortramite;
                            servXCobrar.importeACobrar = importeACobrar + order.OtrosCostos;
                            servXCobrar.Order = order;
                            _context.servicioxCobrar.Add(servXCobrar);

                            //Tramite por pagar
                            var porPagar = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = importeACobrar,
                                Mayorista = wholesaler,
                                Agency = order.Agency,
                                SId = order.OrderId,
                                Order = order,
                                NoServicio = order.Number,
                                Tipo = STipo.Tienda,
                                SubTipo = "-",
                                Express = order.express
                            };
                            porPagar.ServiciosxPagarId = Guid.NewGuid();
                            _context.ServiciosxPagar.Add(porPagar);
                            //Guardo el valor del costo en el tramite
                            order.costoMayorista = importeACobrar + order.OtrosCostos;

                            var wholesalerReyEnvios = await _context.Wholesalers.FirstOrDefaultAsync(x => x.EsVisible && x.AgencyId == Guid.Parse("2F7B03FB-4BE1-474D-8C95-3EE8C6EAEAC1") && x.Category.category == "Tienda");
                            if (wholesalerReyEnvios != null)
                            {
                                //Servicio por pagar del proveedor
                                porPagar = new ServiciosxPagar
                                {
                                    Date = DateTime.Now,
                                    ImporteAPagar = order.costoDeProveedor,
                                    Mayorista = wholesalerReyEnvios,
                                    Agency = order.agencyTransferida,
                                    SId = order.OrderId,
                                    Order = order,
                                    NoServicio = order.Number,
                                    Tipo = STipo.Tienda,
                                    SubTipo = "-",
                                    Express = order.express
                                };
                                _context.ServiciosxPagar.Add(porPagar);
                            }

                        }
                        else
                        {
                            var transferencia = _context.CostoxModuloMayorista
                               .Include(x => x.valoresTramites).Include(x => x.modAsignados)
                               .Where(x => x.AgencyId == order.Agency.AgencyId && x.modAsignados
                               .Where(y => y.IdWholesaler == order.wholesaler.IdWholesaler).Any())
                               .FirstOrDefault();

                            var valorxtramite = transferencia.valoresTramites.Where(x => x.Tramite == "Paquete Aereo").FirstOrDefault();
                            var addressaux = _context.Address.FirstOrDefault(x => x.ReferenceId == order.ContactId);
                            var valorAux = _context.ValorProvincia.Where(x => x.listValores.ValoresxTramiteId == valorxtramite.ValoresxTramiteId && x.provincia == addressaux.City).FirstOrDefault();
                            decimal valor = valorAux.valor;

                            if (order.Type == "Medicinas")
                                valor = valorAux.valor2;

                            decimal importeACobrar = order.CantLb * (valor * order.cantidad) + order.OtrosCostos + order.CustomsTax + order.Delivery;
                            if (order.Type == "Cantidad")
                            {
                                importeACobrar = order.OtrosCostos + order.AditionalCharge;
                            }
                            else if (order.Type == "Paquete" && (AgencyName.Exist_PaqueteMedicina(order.AgencyId)))
                            {
                                importeACobrar = order.CantLb * (valor * order.cantidad) + order.CantLbMedicina * valorAux.valor2 + order.OtrosCostos + order.CustomsTax + order.Delivery;
                            }

                            if (order.Type != "Cantidad" && order.AgencyId.Equals(AgencyName.Cuballama) && order.agencyTransferida.AgencyId.Equals(AgencyName.ViajeExpress))
                            {
                                importeACobrar += order.AditionalCharge;
                            }

                            servicioxCobrar tramitexCobrar = new servicioxCobrar();
                            tramitexCobrar.servicioxCobrarId = Guid.NewGuid();
                            tramitexCobrar.date = DateTime.UtcNow;
                            tramitexCobrar.ServicioId = order.OrderId;
                            tramitexCobrar.tramite = "Paquete Aereo";
                            tramitexCobrar.NoServicio = order.Number;
                            tramitexCobrar.mayorista = order.agencyTransferida;
                            tramitexCobrar.minorista = order.Agency;
                            tramitexCobrar.No_servicioxCobrar = "PAYT" + order.Date.ToString("yyyyMMddHHmmss");
                            tramitexCobrar.cobrado = 0;
                            tramitexCobrar.remitente = order.Client;
                            tramitexCobrar.destinatario = order.Contact;

                            tramitexCobrar.valorTramite = valortramite;
                            tramitexCobrar.importeACobrar = importeACobrar;
                            tramitexCobrar.Order = order;
                            _context.servicioxCobrar.Add(tramitexCobrar);

                            //Tramite por pagar
                            var porPagar = new ServiciosxPagar
                            {

                                Date = DateTime.Now,
                                ImporteAPagar = importeACobrar,
                                Mayorista = order.wholesaler,
                                Agency = order.Agency,
                                SId = order.OrderId,
                                Order = order,
                                NoServicio = order.Number,
                                Tipo = STipo.Paquete,
                                SubTipo = "-",
                                Express = order.express
                            };
                            porPagar.ServiciosxPagarId = Guid.NewGuid();
                            _context.ServiciosxPagar.Add(porPagar);
                            //Guardo el valor del costo en el tramite
                            order.costoMayorista = importeACobrar;

                            //Servicio por pagar a proveedor del mayorista transferido
                            string categoryWholesaler = order.Type;
                            if (order.Type == "Paquete" || order.Type == "Mixto" || order.Type == "Medicinas")
                                categoryWholesaler = "Paquete Aereo";

                            var auxWholesaler = await _context.Wholesalers.Include(x => x.Category)
                                .Include(x => x.CostByProvinces).ThenInclude(x => x.Provincia)
                                .FirstOrDefaultAsync(x => x.EsVisible && x.AgencyId == order.agencyTransferida.AgencyId && x.Category.category == categoryWholesaler);
                            if (auxWholesaler != null && order.Type != "Cantidad")
                            {
                                if (order.Type == "Tienda")
                                    importeACobrar = auxWholesaler.CostoMayorista;
                                else if (order.Type == "Paquete" && AgencyName.Exist_PaqueteMedicina(order.agencyTransferida.AgencyId))
                                {
                                    var costByProvince = auxWholesaler.CostByProvinces.Where(x => x.Type == TypeCostByProvince.Paquete).FirstOrDefault(x => string.Equals(x.Provincia.nombreProvincia, order.Contact.Address.City, StringComparison.OrdinalIgnoreCase));
                                    var costByProvinceMedicina = auxWholesaler.CostByProvinces.Where(x => x.Type == TypeCostByProvince.Medicina).FirstOrDefault(x => string.Equals(x.Provincia.nombreProvincia, order.Contact.Address.City, StringComparison.OrdinalIgnoreCase));
                                    importeACobrar = order.CantLb * (costByProvince?.Cost ?? decimal.Zero) + order.CantLbMedicina * (costByProvinceMedicina?.Cost ?? decimal.Zero) + order.CustomsTax;
                                }
                                else
                                {
                                    var costByProvince = auxWholesaler.CostByProvinces.Where(x => x.Type == TypeCostByProvince.Paquete).FirstOrDefault(x => string.Equals(x.Provincia.nombreProvincia, order.Contact.Address.City, StringComparison.OrdinalIgnoreCase));
                                    if (order.Type.Equals("Medicinas"))
                                        costByProvince = auxWholesaler.CostByProvinces.Where(x => x.Type == TypeCostByProvince.Medicina).FirstOrDefault(x => string.Equals(x.Provincia.nombreProvincia, order.Contact.Address.City, StringComparison.OrdinalIgnoreCase));

                                    importeACobrar = order.CantLb * (costByProvince?.Cost ?? decimal.Zero) + order.CustomsTax;
                                }
                                order.costoDeProveedor = importeACobrar;

                                porPagar = new ServiciosxPagar
                                {

                                    Date = DateTime.Now,
                                    ImporteAPagar = importeACobrar,
                                    Mayorista = auxWholesaler,
                                    Agency = order.agencyTransferida,
                                    SId = order.OrderId,
                                    Order = order,
                                    NoServicio = order.Number,
                                    Tipo = STipo.Paquete,
                                    SubTipo = "-",
                                    Express = order.express
                                };
                                _context.ServiciosxPagar.Add(porPagar);
                            }
                        }
                    }
                    else
                    {
                        decimal importeACobrar = 0;
                        var tipo = STipo.Paquete;
                        if (order.Type == "Tienda" && order.Agency.Name == "Rey Envios")
                        {
                            importeACobrar = Math.Round((decimal)0.95 * (order.PriceLb - 20), 2);
                            tipo = STipo.Tienda;
                        }
                        else if (order.Type == "Cantidad")
                        {
                            importeACobrar = order.costoMayorista;
                        }
                        else if (order.Type == "Paquete" && AgencyName.Exist_PaqueteMedicina(order.AgencyId))
                        {
                            var contact = _context.Contact.Include(x => x.Address).FirstOrDefault(x => x.ContactId == order.ContactId);
                            var costByProvince = wholesaler.CostByProvinces.Where(x => x.Type == TypeCostByProvince.Paquete).FirstOrDefault(x => string.Equals(x.Provincia.nombreProvincia, order.Contact.Address.City, StringComparison.OrdinalIgnoreCase));
                            var costByProvinceMedicina = wholesaler.CostByProvinces.Where(x => x.Type == TypeCostByProvince.Medicina).FirstOrDefault(x => string.Equals(x.Provincia.nombreProvincia, order.Contact.Address.City, StringComparison.OrdinalIgnoreCase));
                            importeACobrar = order.CantLb * (decimal)costByProvince?.Cost + order.CantLbMedicina * (decimal)costByProvinceMedicina?.Cost + order.CustomsTax;
                        }
                        else if (order.Type == "Medicinas")
                        {
                            var contact = _context.Contact.Include(x => x.Address).FirstOrDefault(x => x.ContactId == order.ContactId);
                            var costByProvince = wholesaler.CostByProvinces.Where(x => x.Type == TypeCostByProvince.Medicina).FirstOrDefault(x => string.Equals(x.Provincia.nombreProvincia, order.Contact.Address.City, StringComparison.OrdinalIgnoreCase));
                            importeACobrar = order.CantLb * (decimal)costByProvince?.Cost + order.CustomsTax;
                        }
                        else
                        {
                            var contact = _context.Contact.Include(x => x.Address).FirstOrDefault(x => x.ContactId == order.ContactId);
                            var costByProvince = wholesaler.CostByProvinces.Where(x => x.Type == TypeCostByProvince.Paquete).FirstOrDefault(x => string.Equals(x.Provincia.nombreProvincia, order.Contact.Address.City, StringComparison.OrdinalIgnoreCase));
                            importeACobrar = order.CantLb * (decimal)costByProvince?.Cost + order.CustomsTax;
                        }

                        if (importeACobrar > 0 && !order.wholesaler.Comodin)
                        {
                            //Tramite por pagar
                            var porPagar = new ServiciosxPagar
                            {
                                Date = DateTime.Now,
                                ImporteAPagar = importeACobrar,
                                Mayorista = order.wholesaler,
                                Agency = order.Agency,
                                SId = order.OrderId,
                                Order = order,
                                NoServicio = order.Number,
                                Tipo = tipo,
                                SubTipo = "-",
                                Express = order.express,
                            };
                            porPagar.ServiciosxPagarId = Guid.NewGuid();
                            //Guardo el valor del costo en el tramite
                            _context.ServiciosxPagar.Add(porPagar);
                        }
                        order.costoMayorista = importeACobrar;
                    }  
                }

                //Cuentas por pagar de productos de la bodega
                var listProduct = order.Package.PackageItem.Where(x => x.Product.ProductoBodega != null);
                foreach (var pBodega in listProduct
                    .Select(x => new ExtractProductModel { Product = x.Product.ProductoBodega, Qty = (int)x.Qty })
                    .GroupBy(x => x.Product.Proveedor))
                {
                    var cost = (decimal)pBodega.Sum(x => x.Product.PrecioCompraReferencial * x.Qty);
                    if (cost > 0)
                    {
                        var porPagar = new ServiciosxPagar
                        {
                            Date = DateTime.Now,
                            ImporteAPagar = cost,
                            Order = order,
                            Mayorista = pBodega.Key,
                            Agency = order.Agency,
                            SId = order.OrderId,
                            NoServicio = order.Number,
                            Tipo = STipo.Tienda,
                            SubTipo = "-"
                        };
                        _context.ServiciosxPagar.Add(porPagar);
                    }

                    order.costoProductosBodega += cost;

                    decimal shippingAux = (decimal)pBodega.Select(x => x.Product).Distinct().Sum(x => x.EnableShipping ? x.Shipping : 0);
                    if (order.Agency.AgencyId == AgencyName.ReyEnvios)
                    {
                        var firstProduct = pBodega.Select(x => x.Product).FirstOrDefault(x => x.EnableShipping);
                        shippingAux = firstProduct != null ? firstProduct.Shipping : 0;
                    }

                    if (shippingAux > 0)
                    {
                        var porPagar = new ServiciosxPagar
                        {
                            Date = DateTime.Now,
                            ImporteAPagar = shippingAux,
                            Order = order,
                            Mayorista = pBodega.Key,
                            Agency = order.Agency,
                            SId = order.OrderId,
                            NoServicio = order.Number,
                            Tipo = STipo.Tienda,
                            SubTipo = "-",
                            IsPaymentProductShipping = true
                        };
                        _context.ServiciosxPagar.Add(porPagar);
                    }
                }
            }
            

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
                    Agency = order.Agency,
                    SId = order.OrderId,
                    Order = order,
                    NoServicio = order.Number,
                    Tipo = STipo.Paquete,
                    SubTipo = order.Type
                };
                _context.ServiciosxPagar.Add(s);
            }

            _context.Update(order);
            await _context.SaveChangesAsync();
        }

        private async Task<string> CreateOrderWholesaler(Order order)
        {

            var agency = _context.Agency.Find(order.AgencyId);
            if (string.IsNullOrEmpty(agency.UserApiWholesaler) || string.IsNullOrEmpty(agency.PassApiWholesaler))
                return null;

            var provincias = new Dictionary<string, string>
            {
                ["Pinar Del Rio"] = "Pinar del R\u00edo",
                ["Camaguey"] = "Camag\u00fcey"
            };

            var municipios = new Dictionary<string, string>
            {
                ["10 De Octubre"] = "Diez de Octubre",
                ["San Miguel del Padron"] = "San Miguel del Padr\u00f3n",
                ["Plaza de la Revolucion"] = "Plaza de la Revoluci\u00f3n",
                ["Manati"] = "Manat\u00ed",
                ["Jesus Menendez"] = "Jes\u00fas Men\u00e9ndez",
                ["Tunas"] = "Las Tunas",
                ["Amancio Rdguez"] = "Amancio Rodr\u00edguez",
                ["Rio Cauto"] = "R\u00edo Cauto",
                ["Jiguani"] = "Jiguan\u00ed",
                ["Pilon"] = "Pil\u00f3n",
                ["Bartolome Maso"] = "Bartolom\u00e9 Mas\u00f3",
                ["Holguin"] = "Holgu\u00edn",
                ["Rafael Freire"] = "Rafael Freyre",
                ["Baguanos"] = "B\u00e1guanos",
                ["Calixto Garcia"] = "Calixto Garc\u00eda",
                ["Cacocun"] = "Cacoc\u00fam",
                ["Mayari"] = "Mayar\u00ed",
                ["Frank Pais"] = "Frank Pa\u00eds",
                ["Sagua De Tanamo"] = "Sagua de T\u00e1namo",
                ["Julio Antonio Mella"] = "Julio A. Mella",
                ["II Frente"] = "Segundo Frente",
                ["III Frente"] = "Tercer Frente",
                ["Guama"] = "Guam\u00e1",
                ["Guantanamo"] = "Guant\u00e1namo",
                ["Maisi"] = "Mais\u00ed",
                ["Imias"] = "Im\u00edas",
                ["Niceto Perez"] = "Niceto P\u00e9rez",
                ["Consolacion del Sur"] = "Consolaci\u00f3n del Sur",
                ["Pinar del Rio"] = "Pinar del R\u00edo",
                ["San Juan y Martinez"] = "San Juan y Mart\u00ednez",
                ["Guira De Melena"] = "G\u00fcira de Melena",
                ["Alquizar"] = "Alqu\u00edzar",
                ["Bahia Honda"] = "Bah\u00eda Honda",
                ["San Cristobal"] = "San Crist\u00f3bal",
                ["Quivican"] = "Quivic\u00e1n",
                ["Batabano"] = "Bataban\u00f3",
                ["San Jose de las Lajas"] = "San Jos\u00e9 de las Lajas",
                ["Guines"] = "G\u00fcines",
                ["San Nicolas de Bari"] = "San Nicol\u00e1s",
                ["Colon"] = "Col\u00f3n",
                ["Union de Reyes"] = "Uni\u00f3n de Reyes",
                ["Cienaga de Zapata"] = "Ci\u00e9naga de Zapata",
                ["Jaguey Grande"] = "Jag\u00fcey Grande",
                ["Arabos"] = "Los Arabos",
                ["Santa Isabel de las Lajas"] = "Lajas",
                ["Quemado de Guines"] = "Quemado de G\u00fcines",
                ["Camajuani"] = "Camajuan\u00ed",
                ["Caibarien"] = "Caibari\u00e9n",
                ["Cabaiguan"] = "Cabaigu\u00e1n",
                ["Sancti Spiritus"] = "Sancti Sp\u00edritus",
                ["Moron"] = "Mor\u00f3n",
                ["Ciego de Avila"] = "Ciego de \u00c1vila",
                ["Baragua"] = "Baragu\u00e1",
                ["Cespedes"] = "Carlos Manuel de C\u00e9spedes",
                ["Cubitas"] = "Sierra de Cubitas",
                ["Guaimaro"] = "Gu\u00e1imaro",
                ["Sibanicu"] = "Sibanic\u00fa",
                ["Camaguey"] = "Camag\u00fcey",
                ["Jimaguayu"] = "Jimaguay\u00fa"
            };

            var userShipping = new UserShipping()
            {
                Name = order.Contact.Name ?? "",
                Lastname = order.Contact.LastName ?? "",
                Phone1 = order.Contact.Phone1.Number != null ? order.Contact.Phone1.Number.Replace("(", "").Replace(")", "").Replace("-", "") : "",
                Phone2 = order.Contact.Phone2.Number != null ? order.Contact.Phone2.Number.Replace("(", "").Replace(")", "").Replace("-", "") : "",
                Address = order.Contact.Address.AddressLine1 ?? "",
                Province = order.Contact.Address.City != null ? provincias.GetValueOrDefault(order.Contact.Address.City, order.Contact.Address.City) : "",
                Municipality = order.Contact.Address.State != null ? municipios.GetValueOrDefault(order.Contact.Address.State, order.Contact.Address.State) : "",
                PersonalIdentity = order.Contact.CI ?? "",
                Email = "",
                Lastname2 = ""
            };

            var products = new List<ProductWholesaler>();
            var tags = new List<string>();
            foreach (var b in order.Bag)
            {
                foreach (var p in b.BagItems)
                {
                    tags.Add(p.Product.Tipo);
                }
            }

            if (order.Type == "Medicinas")
            {
                products.Add(new ProductWholesaler()
                {
                    Id = 2,
                    Values = new ProductValue
                    {
                        Tags = tags,
                        Units = ((int)order.CantLb).ToString()
                    }
                });
            }
            else
            {
                products.Add(new ProductWholesaler()
                {
                    Id = 1,
                    Values = new ProductValue
                    {
                        Tags = tags,
                        Units = ((int)order.CantLb).ToString()
                    }
                });
            }

            var orderParams = new ParamsWholesaler()
            {
                Note = order.Nota,
                Products = products,
                ShippingType = 8,
                UserPhone = order.Client.Phone.Number.Replace("(", "").Replace(")", "").Replace("-", ""),
                UserShipping = userShipping,
                Extras = new Extras()
                {
                    CustomsCosts = "",
                    ProcessingCharges = ""
                }
            };

            var orderWholesaler = new OrderWholesaler()
            {
                Action = "checkOut",
                ApiUser = agency.UserApiWholesaler,
                ApiPass = agency.PassApiWholesaler,
                Params = orderParams
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://www.mysmscuba.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                var json = JsonConvert.SerializeObject(orderWholesaler, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpResponse = await client.PostAsync("/p/api/miscellany/api_miscellany.php", content).ConfigureAwait(false);

                var jsonResponse = httpResponse.Content.ReadAsStringAsync().Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<OrderWholesalerResponse>(jsonResponse);
                    return result.Items?.ShippingCode;
                }
                return null;
            }
        }

        private async Task<bool> CancelOrderWholesaler(Order order)
        {
            var agency = _context.Agency.Find(order.AgencyId);
            if (string.IsNullOrEmpty(agency.UserApiWholesaler) || string.IsNullOrEmpty(agency.PassApiWholesaler))
                return false;

            var request = new
            {
                ApiUser = agency.UserApiWholesaler,
                ApiPass = agency.PassApiWholesaler,
                Action = "cancelOrder",
                Params = new { ShippingCode = order.NumberWholesaler }
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://www.mysmscuba.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpResponse = await client.PostAsync("/p/api/miscellany/api_miscellany.php", content).ConfigureAwait(false);

                return httpResponse.IsSuccessStatusCode;
            }

        }

        public async Task AddAttachments(AddAttachmentsModel model)
        {
            var order = await _context.Order.FindAsync(model.OrderId);
            if (order == null) throw new Exception("La orden no existe");
            string sWebRootFolder = _env.WebRootPath;
            var date = DateTime.Now.ToString("yMMddHHmmssff");
            int count = 1;
            foreach (var file in model.Files)
            {
                var auxName = file.FileName;
                var arrName = auxName.Split('.');
                string filename = date + count + '.' + arrName[arrName.Length - 1];
                string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "Upload" + Path.DirectorySeparatorChar + "Order";
                if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                string FullPath = Path.Combine(filePath, filename);
                using (var fileStream = new FileStream(FullPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                count++;

                var newFile = new AttachmentOrder
                {
                    CreatedAt = DateTime.UtcNow,
                    FileType = arrName[arrName.Length - 1],
                    Name = filename,
                    OriginalName = auxName,
                    Order = order,
                    Path = filePath,
                    Description = model.Description,
                };
                _context.Attach(newFile);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Order> CreateManualOrder(User user, CreateManualOrder model)
        {
            //generate client
            Random random = new Random();
            string phone = $"1{random.Next(100000000, 999999999)}";
            //generate order
            var agency = _context.Agency.Find(user.AgencyId);
            var office = _context.Office.FirstOrDefault(x => x.AgencyId == agency.AgencyId);
            var retail = _context.Minoristas.First(x => x.Id == model.RetailId);
            var client = await _clientService.Create(new IClientServices.Models.CreateClientModel()
            {
                Name = model.ClientName ?? retail.Name,
                LastName = "",
                Name2 = "",
                LastName2 = "",
                BirthDate = DateTime.Now,
                Email = "client@agenciapp.com",
                ID = "",
                PhoneCubaNumber = "",
                PhoneNumber = phone,
                Address = new Common.Models.CreateAddressModel
                {
                    AddressLine1 = "",
                    AddressLine2 = "",
                    City = "",
                    Country = "",
                    Countryiso2 = "",
                    State = "",
                    Zip = ""
                }
            }, user);

            if (client.IsFailure) throw new Exception(client.Error);

            
            var order = await this.Create(new CreateAirShipping
            {
                NoOrden = $"{model.Number}-{retail.Identifier}",
                ClientId = client.Value.ClientId,
                ContactId = model.ContactId,
                CantLb = model.CantLb,
                RetaiId = model.RetailId,
                Status = Order.STATUS_RECIBIDA,
                Number = $"PA{DateTime.Now.ToString("yMMddHHmmssff")}",
                ReceivedDate = DateTime.Now,
                AddCosto = 0,
                AddPrecio = 0,
                AditionalCharge = 0,
                AgencyId = user.AgencyId,
                Amount = 0,
                Attachment = null,
                AuthorizationCard = null,
                Bags = new List<CreateAirShipping.CreateBag>(),
                Balance = 0,
                CantLbMedicina = 0,
                CostoMayorista = 0,
                Credit = 0,
                CustomsTax = 0,
                Delivery = 0,
                Express = false,
                Note = "",
                UserId = user.UserId,
                Type = "Paquete",
                OfficeId = office.OfficeId,
                OtherCosts = 0,
                Pays = new List<CreateAirShipping.Pay>(),
                PriceLb = 0,
                PriceLbMedicina = 0,
                ProductsShipping = 0,
                WholesalerId = null,
                PrincipalDistributorId = model.PrincipalDistributorId
            });

            return order;
        }
    }
}
