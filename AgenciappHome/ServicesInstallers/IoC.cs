using Agenciapp.Common.Services;
using Agenciapp.Common.Services.INotificationServices;
using Agenciapp.Service;
using Agenciapp.Service.IBuildEmailServices;
using Agenciapp.Service.IClientServices;
using Agenciapp.Service.IMerchantElavonServices;
using Agenciapp.Service.IMerchantElavonServices.Models;
using Agenciapp.Service.PassportServices;
using Agenciapp.Service.IPromoCodeServices;
using Agenciapp.Service.IReporteCobroServices;
using Agenciapp.Service.IReportServices;
using Agenciapp.Service.PassportServices.IChequeServices;
using Agenciapp.Service.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Sieve.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Agenciapp.Service.IComboServices;
using Agenciapp.Service.IBodegaServices;
using Agenciapp.Service.IContactServices;
using Agenciapp.Service.PushNotifications;
using Agenciapp.Service.Files;
using Agenciapp.Service.ICubiqServices;
using Agenciapp.Service.ReyEnviosStore.Order;
using Agenciapp.Service.IStoreServices;
using Agenciapp.Service.ITusistPackageService;
using Agenciapp.Service.HttpServices;
using Agenciapp.Service.BarcodeLookupServices;
using System.Net.Http;
using AgenciappHome.Logger;
using Agenciapp.Service.TranslateService;
using Agenciapp.ApiClient.BusinessMain;
using Agenciapp.ApiClient.Security;
using Agenciapp.Service.ShippingService;
using Agenciapp.ApiClient.DCuba;
using Agenciapp.Service.IAirShippingServices;
using Agenciapp.Service.IMarketing;
using Agenciapp.Service.IOrderContainer;
using Agenciapp.Domain.Migrations;
using Agenciapp.Service.ReyEnviosStore.WooCommerce;
using Agenciapp.Service.IWholesalerServices;
using Agenciapp.Service.ReyEnviosStore.Product;
using Agenciapp.Service.IProductoBodegaServices;

namespace AgenciappHome.ServicesInstallers
{
    public static class IoC
    {
        public static IServiceCollection AddDependency(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ISieveProcessor, SieveProcessor>();
            services.AddTransient<IUserResolverService, UserResolverService>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<IBuildEmailService, BuildEmailService>();
            services.AddTransient<IMerchantElavonService, MerchantElavonService>();
            services.AddTransient<IChequeService, ChequeService>();
            services.AddTransient<IClientService, ClientService>();
            services.AddTransient<IContactService, ContactService>();
            services.AddTransient<IPromoCodeService, PromoCodeService>();
            services.AddTransient<IReporteCobroService, ReporteCobroService>();
            services.AddTransient<ITaskService, TaskService>();
            //services.AddSingleton<IHostedService, WorkerTask>();
            //services.AddSingleton<IHostedService, WorkerBackground>();
            services.AddTransient<IReportService, ReportService>();
            services.AddTransient<IUtilityService, UtilityService>();
            services.AddTransient<IPassportService, PassportService>();
            services.AddTransient<IComboService, ComboService>();
            services.AddTransient<IBodegaService, BodegaService>();
            services.AddSingleton<ISettingMerchantElavon>(sp =>
                sp.GetRequiredService<IOptions<SettingMerchantElavon>>().Value);
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            services.AddScoped<IFileService, FileService>();
            services.AddTransient<IContainerService, ContainerService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IStoreService, StoreService>();
            services.AddTransient<ITuristPackegeService, TuristPackageService>();
            services.AddTransient<IBarcodeLookupService, BarcodeLookupService>();
            services.AddTransient<ISupportLoggingClient, SupportLoggingClient>();
            services.AddHostedService<WorkerBackground>();
            services.AddScoped<ITranslateService, TranslateService>();
            services.AddScoped<IShippingService, ShippingService>();
            services.AddScoped<IWorkContext, WorkContext>();
            services.AddTransient<IAirShippingService, AirShippingService>();
            services.AddScoped<IMarketingService,  MarketingService>();
            services.AddScoped<IOrderContainerService, OrderContainerService>();
            services.AddScoped<IWooCommerceService, WooCommerceService>();
            services.AddScoped<IWholesalerService, WholesalerService>();

            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductoBodegaService, ProductoBodegaService>();

            //Clients
            services.AddScoped<ShippingApi>();
            services.AddScoped<SecurityLoginApi>();
            services.AddScoped<IDCubaApiClient, DCubaApiClient>();
            return services;
        }
    }
}
