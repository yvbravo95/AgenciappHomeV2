using Agenciapp.Domain.Enums;
using Agenciapp.Domain.Models;
using Agenciapp.Domain.Models.ApiPassport;
using Agenciapp.Domain.Models.BuildEmail;
using Agenciapp.Domain.Models.DBViewModels;
using Agenciapp.Domain.Models.Notification;
using Agenciapp.Domain.Models.SqlViews;
using AgenciappHome.Models.ApiModel;
using AgenciappHome.Models.Auxiliar;
using Microsoft.EntityFrameworkCore;
using RapidMultiservice.Models.Responses;
using Microsoft.EntityFrameworkCore.SqlServer;
using System;

namespace AgenciappHome.Models
{
    public partial class databaseContext : DbContext
    {
        public databaseContext()
        {
        }

        public databaseContext(DbContextOptions<databaseContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=tcp:agenciappserverdb.database.windows.net,1433;Initial Catalog=agenciappdb;Persist Security Info=False;User ID=admin_agenciapp;Password=Habana79@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                //optionsBuilder.UseSqlServer("Server=ANTONIO-PC\\SQLEXPRESS;Initial Catalog=agenciappdb;Integrated Security=True;");
                //optionsBuilder.UseSqlServer("Server=MISTER\\SQLEXPRESS;Initial Catalog=agenciappdb;Integrated Security=True;");
            }
        }
        public virtual DbSet<ConnectionLog> ConnectionLogs { get; set; }
        public virtual DbSet<SettingMinoristaProduct> SettingMinoristaProducts { get; set; }
        public virtual DbSet<Pallet> PalletCubiq { get; set; }
        public virtual DbSet<RegistroCobro> RegistroCobro { get; set; }
        public virtual DbSet<Minorista> Minoristas { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<DocumentPassenger> Documents { get; set; }
        public virtual DbSet<ReporteCobro> ReporteCobros { get; set; }
        public virtual DbSet<PromoCode> PromoCodes { get; set; }
        public virtual DbSet<Address> Address { get; set; }
        public virtual DbSet<Agency> Agency { get; set; }
        public virtual DbSet<AgencyContact> AgencyContact { get; set; }
        public virtual DbSet<Carrier> Carrier { get; set; }
        public virtual DbSet<Client> Client { get; set; }
        public virtual DbSet<Contact> Contact { get; set; }
        public virtual DbSet<ClientContact> ClientContact { get; set; }
        public virtual DbSet<Config> Config { get; set; }
        public virtual DbSet<Office> Office { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<Package> Package { get; set; }
        public virtual DbSet<PackageItem> PackageItem { get; set; }
        public virtual DbSet<Pago> Pago { get; set; }
        public virtual DbSet<Phone> Phone { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<Shipping> Shipping { get; set; }
        public virtual DbSet<ShippingItem> ShippingItem { get; set; }
        public virtual DbSet<TipoPago> TipoPago { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserOffice> UserOffice { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<ValorAduanal> ValorAduanal { get; set; }
        public virtual DbSet<ValorAduanalItem> ValorAduanalItem { get; set; }
        public virtual DbSet<Rechargue> Rechargue { get; set; }
        public virtual DbSet<Passport> Passport { get; set; }
        public virtual DbSet<Airline> Airline { get; set; }
        public virtual DbSet<Ticket> Ticket { get; set; }
        public virtual DbSet<AirlineFlights> AirlineFlights { get; set; }
        public virtual DbSet<Flights> Flights { get; set; }
        public virtual DbSet<Bag> Bag { get; set; }
        public virtual DbSet<BagItem> BagItem { get; set; }
        public virtual DbSet<ImageClient> ImageClient { get; set; }
        public virtual DbSet<Wholesaler> Wholesalers { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<AuthorizationCard> AuthorizationCards { get; set; }
        public virtual DbSet<IdImage> IdImage { get; set; }
        public virtual DbSet<PassportImage> PassportImage { get; set; }
        public virtual DbSet<ResidenciaImage> ResidenciaImage { get; set; }
        public virtual DbSet<ImageAttachment> ImageAttachment { get; set; }
        public virtual DbSet<AttachmentTicket> AttachmentTicket { get; set; }
        public virtual DbSet<DocumentAttachment> DocumentAttachment { get; set; }
        public virtual DbSet<PaymentTicket> PaymentTicket { get; set; }
        public virtual DbSet<formBuilder> formBuilder { get; set; }
        public virtual DbSet<ResponseForm> ResponseForm { get; set; }
        public virtual DbSet<Provincia> Provincia { get; set; }
        public virtual DbSet<Municipio> Municipio { get; set; }
        public virtual DbSet<TramiteEmpleado> TramiteEmpleado { get; set; }
        public virtual DbSet<OrderRevisada> OrderRevisadas { get; set; }
        public virtual DbSet<AccessList> AccessLists { get; set; }
        public virtual DbSet<EnvioMaritimo> EnvioMaritimo { get; set; }
        public virtual DbSet<PagoEnvioMaritimo> PagoEnvioMaritimos { get; set; }
        public virtual DbSet<ValorAduanalItemEnvMaritimo> ValorAduanalItemEnvMaritimos { get; set; }
        public virtual DbSet<ProductEM> ProductEMs { get; set; }
        public virtual DbSet<ModuloAsignado> ModuloAsignados { get; set; }
        public virtual DbSet<CostosxModulo> CostosxModulo { get; set; }
        public virtual DbSet<CostoxModuloMayorista> CostoxModuloMayorista { get; set; }
        public virtual DbSet<ValoresxTramite> ValoresxTramite { get; set; }
        public virtual DbSet<ValorProvincia> ValorProvincia { get; set; }
        public virtual DbSet<servicioxCobrar> servicioxCobrar { get; set; }
        public virtual DbSet<RegistroPago> RegistroPagos { get; set; }
        public virtual DbSet<Factura> Facturas { get; set; }
        public virtual DbSet<TipoRecarga> tipoRecarga { get; set; }
        public virtual DbSet<Servicio> Servicios { get; set; }
        public virtual DbSet<TipoServicio> TipoServicios { get; set; }
        public virtual DbSet<SubServicio> SubServicios { get; set; }
        public virtual DbSet<ServiciosxPagar> ServiciosxPagar { get; set; }
        public virtual DbSet<Bill> Bills { get; set; }
        public virtual DbSet<AccessListUser> AccessListUsers { get; set; }
        public virtual DbSet<RegistroEnvioEmails> RegistroEnvioEmails { get; set; }
        public virtual DbSet<Bodega> Bodegas { get; set; }
        public virtual DbSet<BodegaProducto> BodegaProductos { get; set; }
        public virtual DbSet<CategoriaBodega> CategoriaBodegas { get; set; }
        public virtual DbSet<Cuenta> Cuentas { get; set; }
        public virtual DbSet<EstadoMovimiento> EstadoMovimientos { get; set; }
        public virtual DbSet<Movimiento> Movimientos { get; set; }
        public virtual DbSet<MovimientoProducto> MovimientosProductos { get; set; }
        public virtual DbSet<ProductoBodega> ProductosBodegas { get; set; }
        public virtual DbSet<TipoCuenta> TipoCuentas { get; set; }
        public virtual DbSet<TipoDocumento> TiposDocumentos { get; set; }
        public virtual DbSet<TipoMovimiento> TiposMovimientos { get; set; }
        public virtual DbSet<UnidadMedida> UnidadMedidas { get; set; }
        public virtual DbSet<RegistroEstado> RegistroEstado { get; set; }
        public virtual DbSet<UserWholesaler> UserWholesalers { get; set; }
        public virtual DbSet<Credito> Credito { get; set; }
        public virtual DbSet<UserClient> UserClients { get; set; }
        public virtual DbSet<OrderCubiq> OrderCubiqs { get; set; }
        public virtual DbSet<Paquete> Paquete { get; set; }
        public virtual DbSet<GuiaAerea> GuiaAerea { get; set; }
        public virtual DbSet<CostoCarga> CostosCarga { get; set; }
        public virtual DbSet<Zona> Zona { get; set; }
        public virtual DbSet<EnvioCaribe> EnvioCaribes { get; set; }
        public virtual DbSet<PaqueteEnvCaribe> PaqueteEnvCaribes { get; set; }
        public virtual DbSet<Configuracion> Configuracion { get; set; }
        public virtual DbSet<PassportAux> PassportAux { get; set; }
        public virtual DbSet<PassportAux2> PassportAux2 { get; set; }
        public virtual DbSet<TipoServicioMayorista> TipoServicioMayoristas { get; set; }
        public virtual DbSet<LandingItem> LandingItems { get; set; }
        public virtual DbSet<CatalogItem> CatalogItems { get; set; }
        public virtual DbSet<Mercado> Mercado { get; set; }
        public virtual DbSet<ProductosVendidos> ProductosVendidos { get; set; }
        public virtual DbSet<PrecioPasaporte> PrecioPasaportes { get; set; }
        public virtual DbSet<ServConsularMayorista> ServConsularMayoristas { get; set; }
        public virtual DbSet<PrecioRefMinorista> PrecioRefMinoristas { get; set; }
        public virtual DbSet<AgencyPrecioRefMinorista> AgencyPrecioRefMinoristas { get; set; }
        public virtual DbSet<CuentaBancaria> CuentaBancarias { get; set; }
        public virtual DbSet<RecBancariaAgency> RecBancariaAgencies { get; set; }
        public virtual DbSet<CashAccountingBoxItem> CashAccountingBoxItems { get; set; }
        public virtual DbSet<CashAdjustment> CashAdjustments { get; set; }
        public virtual DbSet<CashBox> CashBoxes { get; set; }
        public virtual DbSet<BoxHistory> BoxHistories { get; set; }
        public virtual DbSet<Pasajero> Pasajero { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<InvoiceProductoBodega> InvoiceProductoBodegas { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<Reclamo> Reclamos { get; set; }
        public virtual DbSet<ImagenPromotion> ImagenPromotion { get; set; }
        public virtual DbSet<ImagenPromotionRapidApp> ImagenPromotionRapidApp { get; set; }
        public virtual DbSet<PassportExpress> PassportExpresses { get; set; }
        public virtual DbSet<SettingPassportExpress> SettingPassportExpresses { get; set; }
        public virtual DbSet<RefundLog> RefundLogs { get; set; }
        public virtual DbSet<Remittance> Remittance { get; set; }
        public virtual DbSet<CostByProvince> CostByProvinces { get; set; }
        public virtual DbSet<NotificationByAgency> NotificationByAgencies { get; set; }
        public virtual DbSet<CardRemittance> CardRemittances { get; set; }
        public virtual DbSet<ProductoBodegaCatalogItem> ProductoBodegaCatalogItems { get; set; }
        public virtual DbSet<ZelleItem> ZelleItems { get; set; }
        public virtual DbSet<ZelleEmailSetting> ZelleEmailSettings { get; set; }
        public virtual DbSet<PaymentCard> PaymentCards { get; set; }
        public virtual DbSet<AuxReserva> AuxReservas { get; set; }
        public virtual DbSet<Rentadora> Rentadora { get; set; }
        public virtual DbSet<PreciosAutos> PreciosAutos { get; set; }
        public virtual DbSet<RemesaAux> RemesaAux { get; internal set; }
        public virtual DbSet<Credit> Credits { get; internal set; }
        public virtual DbSet<CreditWholesaler> CreditWholesalers { get; internal set; }
        public virtual DbSet<Task_> Tasks { get; internal set; }
        public virtual DbSet<TaskLog> TaskLogs { get; internal set; }
        public virtual DbSet<Subject> Subjects { get; internal set; }
        public virtual DbSet<GenerateXml> GenerateXml { get; internal set; }
        public virtual DbSet<GenerateXmlAux> GenerateXmlAux { get; internal set; }
        public virtual DbSet<MinoristaPasaporte> MinoristaPasaportes { get; internal set; }
        public virtual DbSet<GuiaPasaporte> GuiasPasaporte { get; set; }
        public virtual DbSet<ManifiestoPasaporte> ManifiestosPasaporte { get; set; }
        public virtual DbSet<Estado> Estados { get; set; }
        public virtual DbSet<Ciudad> Ciudades { get; set; }

        public virtual DbSet<EmailTemplate> EmailTemplates { get; internal set; }
        public virtual DbSet<EmailBody> EmailBodies { get; internal set; }
        public virtual DbSet<EmailAttached> EmailAttacheds { get; internal set; }
        public virtual DbSet<RegistrationSendEmail> RegistrationSendEmails { get; internal set; }
        public virtual DbSet<Cheque> Cheques { get; internal set; }
        public virtual DbSet<ChequeProrroga> ChequeProrrogas { get; internal set; }
        public virtual DbSet<ChequeProrrogaDoble> ChequeProrrogaDobles { get; internal set; }
        public virtual DbSet<ChequeRenovacion> ChequeRenovacions { get; internal set; }
        public virtual DbSet<ChequePrimeraVez> ChequePrimeraVezs { get; internal set; }
        public virtual DbSet<ChequeOtorgamiento> ChequeOtorgamientos { get; internal set; }
        public virtual DbSet<ChequeHE11> ChequeHE11s { get; internal set; }
        public virtual DbSet<Note> Notes { get; internal set; }
        public virtual DbSet<TipoHabitacion> TipoHabitacion { get; set; }
        public virtual DbSet<Habitaciones> Habitaciones { get; set; }
        public virtual DbSet<Attachment> Attachments { get; internal set; }
        public virtual DbSet<AttachmentPassport> AttachmentPassports { get; internal set; }
        public virtual DbSet<AttachmentReclamo> AttachmentReclamos { get; internal set; }
        public virtual DbSet<AttachmentServicio> AttachmentServicios { get; internal set; }
        public virtual DbSet<AttachmentOrder> AttachmentOrder { get; internal set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<StatusDescription> StatusDescriptions { get; set; }
        public virtual DbSet<AccessGuiaAereaAgency> AccessGuiaAereaAgencies { get; set; }
        public virtual DbSet<Hotel> Hoteles { get; set; }
        public virtual DbSet<FcmToken> FcmTokens { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<UserDistributor> UserDistributors { get; set; }
        public virtual DbSet<CostoTraslado> CostoTraslados { get; set; }
        public virtual DbSet<ReporteLiquidacion> ReporteLiquidacion { get; set; } // SQL VIEW
        public virtual DbSet<ApiPassportSetting> ApiPassportSetting { get; set; } // SQL VIEW
        public virtual DbSet<ImagenPromotionPassport> ImagenPromotionPassports { get; set; } // SQL VIEW
        public virtual DbSet<PaqueteTuristico> PaquetesTuristicos { get; set; }
        public virtual DbSet<InvoiceNote> InvoiceNotes { get; set; }
        public virtual DbSet<StoredProduct> StoredProducts { get; set; }
        public virtual DbSet<RegistroPagosToday> RegistroPagosToday { get; set; } //sql view
        public virtual DbSet<Cotizador> Cotizador { get; set; }
        public virtual DbSet<PriceProductByprovince> PriceProductByprovinces { get; set; }
        public virtual DbSet<Aduana> Aduana { get; set; }
        public virtual DbSet<RelacionMinorista> RelacionMinoristas { get; set; }
        public virtual DbSet<UserAgencyTransferred> UserAgencyTransferreds { get; set; }
        public virtual DbSet<Pais> Paises { get; set; }
        public virtual DbSet<MinoristaOtrosServ> MinoristaOtrosServs { get; set; }
        public virtual DbSet<MinorAuthorization> MinorAuthorizationOrders { get; set; }
        public virtual DbSet<HMpaquetesPriceByProvince> HMpaquetesPriceByProvinces { get; set; }
        public virtual DbSet<OrdersReceivedByAgency> OrdersReceivedByAgency { get; set; } // SQL View
        public virtual DbSet<OrdersByProvince> OrdersByProvince { get; set; } // SQL View
        public virtual DbSet<IncompletePassport> IncompletePassport { get; set; } // Api Passport
        public virtual DbSet<ServicesByPayNotBilled> ServicesByPayNotBilled { get; set; } // Api Passport
        public virtual DbSet<HMIncompleteOrdersReceived> HMIncompleteOrdersReceived { get; set; } // SQL VIEW
        public virtual DbSet<OrdersDispatchedBaggageReceived> OrdersDispatchedBaggageReceived { get; set; } // SQL VIEW
        public virtual DbSet<MinoristaCarga> MinoristaCargas { get; set; }
        public virtual DbSet<Marketing> Marketings { get; set; }
        public virtual DbSet<MarketingReceiptCampaing> MarketingReceiptCampaings { get; set; }
        public virtual DbSet<OrderContainer> OrderContainers { get; set; }
        public virtual DbSet<SettingRetailsStore> SettingRetailsStore { get; set; }
        public virtual DbSet<BagEM> BagEMs { get; set; }
        public virtual DbSet<PreDespachoCubiq> PreDespachoCubiqs { get; set; }
        public virtual DbSet<PreDespachoVerifiedItemCubiq> PreDespachoItemCubiqs { get; set; }
        public virtual DbSet<CargaAMSeguro> CargaAMSeguros { get; set; }
        public virtual DbSet<CargaAMSeguroItem> CargaAMSeguroItems { get; set; }
        public virtual DbSet<FlightCubaSearch> FlightCubaSearch { get; set; }
        public virtual DbSet<ProductCargaAm> ProductsCargaAm { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BagEM>()
            .HasIndex(b => b.Number)
            .IsUnique();

            modelBuilder.Entity<Pallet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Number).IsUnique();
                entity.HasMany(x => x.Packages).WithOne(x => x.Pallet).HasForeignKey(x => x.PalletId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<PreDespachoCubiq>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User);
                entity.Property(e => e.Number).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.HasIndex(e => e.Number).IsUnique();
                entity.Property(e => e.Date).IsRequired();
                entity.HasOne(e => e.Agency).WithMany().HasForeignKey(e => e.AgencyId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.AgencyTransferida).WithMany().HasForeignKey(e => e.AgencyTransferidaId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<PreDespachoVerifiedItemCubiq>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.PreDespacho).WithMany(e => e.Items).HasForeignKey(e => e.PreDespachoId);
                entity.HasOne(e => e.OrderCubiq).WithMany().HasForeignKey(e => e.OrderId);
                entity.HasOne(e => e.Paquete).WithMany().HasForeignKey(e => e.PaqueteId).OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.OrderNumber).IsRequired();
                entity.Property(e => e.PaqueteNumber).IsRequired();
            });

            modelBuilder.Entity<HMpaquetesPriceByProvince>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.Agency).WithMany().HasForeignKey(x => x.AgencyId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.RetailAgency).WithMany().HasForeignKey(x => x.RetailAgencyId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Retail).WithMany().HasForeignKey(x => x.RetailId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Province).WithMany().HasForeignKey(x => x.ProvinceId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Municipality).WithMany().HasForeignKey(x => x.MunicipalityId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ImagenPromotionRapidApp>(entity =>
            {
                entity.Property(p => p.Type)
                .HasConversion(t => t.ToString(), t => (TypeImagenPromotion)Enum.Parse(typeof(TypeImagenPromotion), t));
            });

            modelBuilder.Entity<MinoristaCarga>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<EnvioMaritimo>(entity =>
            {
                entity.Property(x => x.Number).IsRequired();
                entity.HasIndex(x => x.Number).IsUnique();
            });

            modelBuilder.Entity<CreditWholesaler>(entity =>
            {
                entity.Property(p => p.MoneyType)
                .HasConversion(t => t.ToString(), t => (MoneyType)Enum.Parse(typeof(MoneyType), t));
            });

            modelBuilder.Entity<ZelleItem>(entity =>
            {
                entity.Property(p => p.Type)
                .HasConversion(t => t.ToString(), t => (STipo)Enum.Parse(typeof(STipo), t));
            });

            modelBuilder.Entity<ProductoBodegaCatalogItem>()
       .HasKey(bc => new { bc.ProductoBodegaId, bc.CatalogItemId });
            modelBuilder.Entity<ProductoBodegaCatalogItem>()
                .HasOne(bc => bc.ProductoBodega)
                .WithMany(b => b.productoBodegaCatalogItems)
                .HasForeignKey(bc => bc.ProductoBodegaId);
            modelBuilder.Entity<ProductoBodegaCatalogItem>()
                .HasOne(bc => bc.CatalogItem)
                .WithMany(c => c.productoBodegaCatalogItems)
                .HasForeignKey(bc => bc.CatalogItemId);

            //relacion de muchos a muchos entre Invoice y ProductoBodega
            modelBuilder.Entity<InvoiceProductoBodega>()
            .HasKey(t => new { t.InvoiceId, t.ProductoBodegaId });

            modelBuilder.Entity<InvoiceProductoBodega>()
                .HasOne(pt => pt.Invoice)
                .WithMany(p => p.InvoiceProductoBodega)
                .HasForeignKey(pt => pt.InvoiceId);

            modelBuilder.Entity<InvoiceProductoBodega>()
                .HasOne(pt => pt.ProductoBodega)
                .WithMany(t => t.InvoiceProductoBodega)
                .HasForeignKey(pt => pt.ProductoBodegaId);

            //relacion de muchos a muchos entre Usuario y Wholesaler
            modelBuilder.Entity<UserWholesaler>()
            .HasKey(t => new { t.UserId, t.WholesalerId });

            modelBuilder.Entity<UserWholesaler>()
                .HasOne(pt => pt.User)
                .WithMany(p => p.UserWholesalers)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<UserWholesaler>()
                .HasOne(pt => pt.Wholesaler)
                .WithMany(t => t.UserWholesalers)
                .HasForeignKey(pt => pt.WholesalerId);
            //Fin relacion muchos a muchos


            modelBuilder.Entity<AttachmentTicket>()
            .HasOne(p => p.Ticket)
            .WithMany(b => b.Attachments);

            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.AddressId)
                    .IsClustered(false);

                entity.Property(e => e.AddressId).ValueGeneratedNever();

                entity.Property(e => e.AddressLine1)
                    .HasMaxLength(250);

                entity.Property(e => e.AddressLine2).HasMaxLength(250);

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Country)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.State)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Zip).HasMaxLength(50);
            });

            modelBuilder.Entity<PaymentCard>(entity =>
            {
                entity.HasOne(d => d.UserClient)
                    .WithMany(p => p.PaymentCards)
                    .HasForeignKey(d => d.UserClientId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Agency>(entity =>
            {
                entity.HasKey(e => e.AgencyId)
                    .IsClustered(false);

                entity.Property(e => e.AgencyId).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LegalName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Ignore(e => e.Address).Ignore(e => e.PhoneNumber);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<AgencyContact>(entity =>
            {
                entity.HasKey(e => e.AgencyContactId)
                    .IsClustered(false);

                entity.Property(e => e.AgencyContactId).ValueGeneratedNever();

                entity.Property(e => e.AgencyId)
                    .IsRequired();

                entity.Property(e => e.ContactId)
                    .IsRequired();
            });

            modelBuilder.Entity<Carrier>(entity =>
            {
                entity.HasKey(e => e.CarrierId)
                    .IsClustered(false);

                entity.Property(e => e.CarrierId).ValueGeneratedNever();

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.CreateAt)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.AgencyId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(e => e.Client)
                .WithOne();
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.ClientId)
                    .IsClustered(false);

                entity.HasIndex(e => new { e.AgencyId, e.ClientId });

                entity.Property(e => e.ClientId).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ClientNumber)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .HasMaxLength(100);

                entity.OwnsOne(x => x.Passport);

                entity.OwnsOne(x => x.OtherDocument);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Ignore(e => e.PhoneNumber);

                entity.HasOne(d => d.Agency)
                    .WithMany(p => p.Client)
                    .HasForeignKey(d => d.AgencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RefAgency8");
            });

            modelBuilder.Entity<Contact>(entity =>
            {
                entity.HasKey(e => e.ContactId)
                    .IsClustered(false);

                entity.Property(e => e.ContactId).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ContactNumber)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Ignore(cc => cc.PhoneNumber1).Ignore(cc => cc.PhoneNumber2);
            });

            modelBuilder.Entity<ClientContact>(entity =>
            {
                entity.HasKey(e => e.CCId)
                    .IsClustered(false);

                entity.Property(e => e.CCId).ValueGeneratedNever();
                entity.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId);
                entity.HasOne(x => x.Contact).WithMany().HasForeignKey(x => x.ContactId);

                //entity.Ignore(cc => cc.Contact).Ignore(cc => cc.Client);
            });

            modelBuilder.Entity<Config>(entity =>
            {
                entity.HasKey(c => c.Id)
                    .IsClustered(false);

                entity.Property(c => c.Id).ValueGeneratedNever();

                entity.Property(c => c.Email_Server)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Email_Port)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Email_User)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Email_Pass)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Office>(entity =>
            {
                entity.HasKey(e => e.OfficeId)
                    .IsClustered(false);

                entity.Property(e => e.OfficeId).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Ignore(e => e.OfficePhone).Ignore(e => e.OfficeAddress);

                entity.HasOne(d => d.Agency)
                    .WithMany(p => p.Office)
                    .HasForeignKey(d => d.AgencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RefAgency9");


            });

            modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId)
                .IsClustered(false);

            entity.Property(e => e.OrderId).ValueGeneratedNever();

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.Balance).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.CantLb).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.Date).HasColumnType("datetime");

            entity.Property(e => e.Number)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.OtrosCostos).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.PriceLb).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ValorAduanal).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.ValorPagado).HasColumnType("decimal(10, 2)");

            entity.Property(e => e.StoreType)
            .HasConversion(t => t.ToString(), t => (StoreType)Enum.Parse(typeof(StoreType), t))
            .HasDefaultValue(StoreType.Agenciapp);

            entity.HasOne(d => d.Agency)
                .WithMany(p => p.Order)
                .HasForeignKey(d => d.AgencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("RefAgency17");

            entity.HasOne(d => d.Client)
                .WithMany(p => p.Order)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("RefClient18");

            entity.HasOne(d => d.Contact)
                .WithMany(p => p.Order)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Contact");

            entity.HasOne(d => d.Office)
                .WithMany(p => p.Order)
                .HasForeignKey(d => d.OfficeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("RefOffice19");

            entity.HasOne(d => d.TipoPago)
                .WithMany(p => p.Order)
                .HasForeignKey(d => d.TipoPagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_TipoPago");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Order)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_User");

            entity.HasOne(d => d.PrincipalDistributor)
            .WithMany()
            .HasForeignKey(x => x.PrincipalDistributorId);

            entity.HasOne(d => d.Repartidor)
            .WithMany()
            .HasForeignKey(x => x.RepartidorId);

        });

            modelBuilder.Entity<Package>(entity =>
            {
                entity.HasKey(e => e.PackageId)
                    .IsClustered(false);

                entity.Property(e => e.PackageId).ValueGeneratedNever();

                entity.HasOne(d => d.PackageNavigation)
                    .WithOne(p => p.Package)
                    .HasForeignKey<Package>(d => d.PackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RefOrder20");
            });

            modelBuilder.Entity<PackageItem>(entity =>
            {
                entity.HasKey(e => e.PackageItemId)
                    .IsClustered(false);

                entity.Property(e => e.PackageItemId).ValueGeneratedNever();


                entity.Property(e => e.Supplier)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Qty).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.PackageItem)
                    .HasForeignKey(d => d.PackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RefPackage1");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.PackageItem)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RefProduct2");
            });

            modelBuilder.Entity<Phone>(entity =>
            {
                entity.Property(e => e.PhoneId).ValueGeneratedNever();

                entity.Property(e => e.Number)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId)
                    .IsClustered(false);

                entity.Property(e => e.ProductId).ValueGeneratedNever();

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Color)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.TallaMarca)
                    .IsRequired()
                    .HasColumnName("Talla/Marca")
                    .HasMaxLength(50);

                entity.Property(e => e.Tipo)
                    .IsRequired();

                entity.HasOne(d => d.Agency)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.AgencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RefAgency12");
            });

            modelBuilder.Entity<Shipping>(entity =>
            {
                entity.HasKey(e => e.PackingId)
                    .IsClustered(false);

                entity.Property(e => e.PackingId).ValueGeneratedNever();

                entity.HasOne(d => d.Agency)
                    .WithMany(p => p.Shipping)
                    .HasForeignKey(d => d.AgencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RefAgency4");

                entity.HasOne(d => d.Carrier)
                    .WithMany(p => p.Shipping)
                    .HasForeignKey(d => d.CarrierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RefCarrier5");

                entity.HasOne(d => d.Office)
                    .WithMany(p => p.Shipping)
                    .HasForeignKey(d => d.OfficeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RefOffice16");
            });

            modelBuilder.Entity<ShippingItem>(entity =>
            {
                entity.HasKey(e => e.ShippingItemId)
                    .IsClustered(false);

                entity.Property(e => e.ShippingItemId).ValueGeneratedNever();

                entity.Property(e => e.Qty).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.ShippingItem)
                    .HasForeignKey(d => d.PackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RefPackage14");

                entity.HasOne(d => d.Packing)
                    .WithMany(p => p.ShippingItem)
                    .HasForeignKey(d => d.PackingId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RefShipping13");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ShippingItem)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RefProduct15");
            });

            modelBuilder.Entity<TipoPago>(entity =>
            {
                entity.Property(e => e.TipoPagoId).ValueGeneratedNever();

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .IsClustered(false);

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ExpiresSecureCode).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.PasswordSalt)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.SecureCode).HasMaxLength(250);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Timestamp).HasColumnType("datetime");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);

            });

            modelBuilder.Entity<UserAgencyTransferred>(entity =>
            {
                entity.HasKey(e => new
                {
                    e.AgencyId,
                    e.UserId
                });

                entity.HasOne(x => x.Agency).WithMany().HasForeignKey(y => y.AgencyId);
                entity.HasOne(x => x.User).WithMany().HasForeignKey(y => y.UserId);
            });

            modelBuilder.Entity<UserOffice>(entity =>
            {
                entity.HasKey(e => e.UserOfficeId)
                    .IsClustered(false);

                entity.Property(e => e.UserOfficeId).ValueGeneratedNever();

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.OfficeId)
                    .IsRequired();

            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.HasKey(e => e.RoleId)
                    .IsClustered(false);

                entity.Property(e => e.RoleId).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<ValorAduanal>(entity =>
            {
                entity.Property(e => e.ValorAduanalId).ValueGeneratedNever();

                entity.Property(e => e.Article)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description).HasMaxLength(250);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Observaciones).HasMaxLength(250);

                entity.Property(e => e.Um)
                    .IsRequired()
                    .HasColumnName("UM")
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnType("decimal(10, 2)");
            });

            modelBuilder.Entity<ValorAduanalItem>(entity =>
            {
                entity.Property(e => e.ValorAduanalItemId).ValueGeneratedNever();

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.ValorAduanalItem)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Table_Order");

                entity.HasOne(d => d.ValorAduanal)
                    .WithMany(p => p.ValorAduanalItem)
                    .HasForeignKey(d => d.ValorAduanalId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Table_ValorAduanal");
            });

            modelBuilder.Entity<Rechargue>(entity =>
            {
                entity.HasKey(e => e.RechargueId)
                    .IsClustered(false);

                entity.Property(e => e.RechargueId).ValueGeneratedNever();

                entity.Property(e => e.ClientId)
                    .IsRequired();

                entity.Property(e => e.AgencyId)
                    .IsRequired();

                entity.Property(e => e.NumberPhone)
                    .IsRequired();

                entity.Property(e => e.Count)
                    .IsRequired();

                entity.Property(e => e.Number)
                    .IsRequired();



            });

            modelBuilder.Entity<ServConsularMayorista>(entity =>
            {
                entity.Property(p => p.servicio)
                .HasConversion(t => t.ToString(), t => (ServicioConsular)Enum.Parse(typeof(ServicioConsular), t));

            });

            modelBuilder.Entity<Passport>(entity =>
            {
                entity.HasKey(e => e.PassportId)
                    .IsClustered(false);

                entity.Property(x => x.OrderNumber).IsRequired();
                entity.HasIndex(x => x.OrderNumber).IsUnique();

                entity.Property(p => p.Type)
                .HasConversion(t => t.ToString(), t => (PassportType)Enum.Parse(typeof(PassportType), t));

                entity.Property(p => p.Tramite)
                               .HasConversion(t => t.ToString(), t => (TramiteType)Enum.Parse(typeof(TramiteType), t));


                entity.Property(p => p.ServicioConsular)
                                .HasConversion(t => t.ToString(), t => (ServicioConsular)Enum.Parse(typeof(ServicioConsular), t));

                entity.Property(p => p.ProrrogaType)
                                .HasConversion(t => t.ToString(), t => (Prorroga1Type)Enum.Parse(typeof(Prorroga1Type), t));

                entity.Property(p => p.TipoSolicitud)
                                .HasConversion(t => t.ToString(), t => (TypeSolicitud)Enum.Parse(typeof(TypeSolicitud), t));

                entity.Property(p => p.CategoriaProfesion)
                               .HasConversion(t => t.ToString(), t => (CategoriaProfesion)Enum.Parse(typeof(CategoriaProfesion), t));


                entity.Property(p => p.NivelCultural)
                               .HasConversion(t => t.ToString(), t => (NivelEscolar)Enum.Parse(typeof(NivelEscolar), t));


                entity.Property(p => p.RazonNoDisponibilidad)
                               .HasConversion(t => t.ToString(), t => (RazonNoDisponibilidad)Enum.Parse(typeof(RazonNoDisponibilidad), t));


                entity.Property(p => p.Sex)
                    .HasConversion(t => t.ToString(), t => (Sex)Enum.Parse(typeof(Sex), t));

                entity.Property(p => p.ColorOjos)
                                .HasConversion(t => t.ToString(), t => (ColorOjos)Enum.Parse(typeof(ColorOjos), t));

                entity.Property(p => p.ColorPiel)
                .HasConversion(t => t.ToString(), t => (ColorPiel)Enum.Parse(typeof(ColorPiel), t));

                entity.Property(p => p.ColorCabello)
                                .HasConversion(t => t.ToString(), t => (ColorCabello)Enum.Parse(typeof(ColorCabello), t));

                entity.Property(p => p.ClasificacionMigratoria)
                .HasConversion(t => t.ToString(), t => (ClasificacionMigratoria)Enum.Parse(typeof(ClasificacionMigratoria), t));
            });

            modelBuilder.Entity<Airline>(entity =>
            {
                entity.HasKey(e => e.AirlineId)
                    .IsClustered(false);
                entity.Property(e => e.AirlineId).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(10);
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.TicketId)
                    .IsClustered(false);
                entity.Property(e => e.TicketId).ValueGeneratedNever();

                entity.HasOne(e => e.Client)
                    .WithMany()
                    .HasForeignKey(x => x.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.State)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.DateOut)
                    .IsRequired()
                    .HasColumnType("datetime");

                entity.Property(e => e.DateIn)
                    .IsRequired()
                    .HasColumnType("datetime");

                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(20);


                entity.Property(e => e.Checked)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.AirlineFlightsId)
                    .IsRequired();

                entity.Property(e => e.Door)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RegisterDate)
                    .IsRequired()
                    .HasColumnType("datetime");

                entity.Property(e => e.TicketBy)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Charges);

                entity.Property(e => e.Price)
                    .IsRequired();

                entity.Property(e => e.Cost);

                entity.Property(e => e.Tax)
                    .IsRequired();

                entity.Property(e => e.Commission)
                    .IsRequired();

                entity.Property(e => e.Discount);

                entity.Property(e => e.TypePayment)
                    .IsRequired();

                entity.Property(e => e.Total)
                    .IsRequired();

                entity.Property(e => e.Payment)
                    .IsRequired();

                entity.Property(e => e.Debit)
                    .IsRequired();
            });

            modelBuilder.Entity<AirlineFlights>(entity =>
           {
               entity.HasKey(e => e.AirlineFlightsId)
                   .IsClustered(false);
               entity.Property(e => e.AirlineFlightsId).ValueGeneratedNever();

               entity.Property(e => e.AirlineId)
                   .IsRequired();

               entity.Property(e => e.FlightsId)
                   .IsRequired();
           });

            modelBuilder.Entity<Flights>(entity =>
            {
                entity.HasKey(e => e.FlightsId)
                    .IsClustered(false);
                entity.Property(e => e.FlightsId).ValueGeneratedNever();

                entity.Property(e => e.CityIn)
                    .IsRequired();

                entity.Property(e => e.CityOut)
                    .IsRequired();
            });

            modelBuilder.Entity<Bag>(entity =>
            {
                entity.HasKey(e => e.BagId)
                    .IsClustered(false);
                entity.Property(e => e.BagId).ValueGeneratedNever();

                entity.Property(e => e.Code)
                     .IsRequired();
                entity.HasIndex(e => e.Code).IsUnique();

                entity.Property(e => e.IsComplete).HasDefaultValue(true);
            });

            modelBuilder.Entity<BagItem>(entity =>
            {
                entity.HasKey(e => e.BagItemId)
                    .IsClustered(false);
                entity.Property(e => e.BagItemId).ValueGeneratedNever();

                entity.Property(e => e.ProductId)
                     .IsRequired();

                entity.Property(e => e.Qty)
                     .IsRequired();

                entity.Property(e => e.BagId)
                     .IsRequired();

                entity.HasOne(e => e.Bag).WithMany(e => e.BagItems);

                entity.HasOne(x => x.Product).WithOne(x => x.BagItem);
            });

            modelBuilder.Entity<ServiciosxPagar>(entity =>
            {
                entity.Property(sp => sp.Tipo)
                .HasConversion(t => t.ToString(), t => (STipo)Enum.Parse(typeof(STipo), t));

                entity.Property(sp => sp.NoServicio).IsRequired();
                entity.HasIndex(x => x.NoServicio);
                entity.HasIndex(x => x.SId);
            });




            modelBuilder.Entity<OrderCubiq>(entity =>
            {
                entity.HasIndex(d => d.Number).IsUnique();

                entity.HasOne(d => d.Agency)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Client)
                    .WithMany()
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Contact)
                    .WithMany()
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Office)
                    .WithMany()
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.agencyTransferida)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<PrecioPasaporte>()
                .Property(sp => sp.ServicioConsular)
                .HasConversion(t => t.ToString(), t => (ServicioConsular)Enum.Parse(typeof(ServicioConsular), t));

            modelBuilder.Entity<Reclamo>()
               .Property(sp => sp.Type)
               .HasConversion(t => t.ToString(), t => (ReclamoType)Enum.Parse(typeof(ReclamoType), t));


            modelBuilder.Entity<Log>(e =>
            {
                e.Property(l => l.Type)
                    .HasConversion(t => t.ToString(), t => (LogType)Enum.Parse(typeof(LogType), t));
                e.Property(l => l.Event)
                    .HasConversion(t => t.ToString(), t => (LogEvent)Enum.Parse(typeof(LogEvent), t));
            });
            modelBuilder.Entity<Hotel>(e =>
            {
                e.HasOne(d => d.Mayorista)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<Paquete>(e =>
            {
                e.HasKey(x => x.PaqueteId);
                e.HasIndex(x => x.Numero).IsUnique(true);
                e.HasOne(x => x.Pallet).WithMany(x => x.Packages).HasForeignKey(x => x.PalletId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<UserDistributor>(builder =>
            {
                builder.HasKey(x => x.Id);

                builder.HasOne(x => x.Distributor).WithMany().HasForeignKey("DistributorId");
                builder.HasOne(x => x.Employee).WithMany().HasForeignKey("EmployeeId");
            });

            modelBuilder.Entity<MinorAuthorization>(entity =>
            {
                entity.HasKey(x => x.Id);
            });
        }
    }
}
