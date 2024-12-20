using System.Globalization;
using Agenciapp.ApiClient.BusinessAirShipping;
using Agenciapp.ApiClient.BusinessMain;
using Agenciapp.ApiClient.FlightProviders;
using Agenciapp.ApiClient.Models;
using Agenciapp.Common.Services.INotificationServices.Models;
using Agenciapp.Service.IMerchantElavonServices.Models;
using Agenciapp.Service.TranslateService.Models;
using AgenciappHome.Controllers.Class;
using AgenciappHome.Middlewares;
using AgenciappHome.Models;
using AgenciappHome.ServicesInstallers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using WebMarkupMin.AspNetCore3;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddCors();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.Configure<FormOptions>(x => x.ValueCountLimit = 50000);
builder.Services.Configure<SettingMerchantElavon>(
    builder.Configuration.GetSection(nameof(SettingMerchantElavon)));
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddDistributedMemoryCache();

builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));
builder.Services.Configure<SendgridSetting>(builder.Configuration.GetSection("SendgridSetting"));
builder.Services.Configure<TwilioSetting>(builder.Configuration.GetSection("TwilioSetting"));
builder.Services.Configure<BusinessMainSetting>(builder.Configuration.GetSection("BusinessMainSetting"));
builder.Services.Configure<BusinessTicketSetting>(builder.Configuration.GetSection("BusinessTicketSetting"));
builder.Services.Configure<BusinessAirShippingSetting>(builder.Configuration.GetSection("BusinessAirShippingSetting"));
builder.Services.Configure<BusinessShippingSetting>(builder.Configuration.GetSection("BusinessShippingSetting"));
builder.Services.Configure<BusinessPassportSetting>(builder.Configuration.GetSection("BusinessPassportSetting"));
builder.Services.Configure<BusinessRemittanceSetting>(builder.Configuration.GetSection("BusinessRemittanceSetting"));
builder.Services.Configure<SettingTranslate>(builder.Configuration.GetSection("SettingTranslate"));
builder.Services.Configure<SecuritySetting>(builder.Configuration.GetSection("SecuritySetting"));

builder.Services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });

CultureInfo[] supportedCultures = new[]
           {
                new CultureInfo("en-US")
            };

builder.Services.Configure<RequestLocalizationOptions>(options =>
           {
               options.DefaultRequestCulture = new RequestCulture("en-US");
               options.SupportedCultures = supportedCultures;
               options.SupportedUICultures = supportedCultures;
               options.RequestCultureProviders = new List<IRequestCultureProvider>
               {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider()
               };
           });
builder.Services.AddTransient<IAuthorizationHandler, CustomRequirementHandler>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddHttpClient<ClientApi>();
builder.Services.AddHttpClient<TicketApi>();
builder.Services.AddHttpClient<RemittanceApi>();
builder.Services.AddHttpClient<OrderApi>();
//builder.Services.AddHttpClient<ShippingApi>();
builder.Services.AddHttpClient<ComboApi>();
builder.Services.AddHttpClient<PassportApi>();
builder.Services.AddHttpClient<FlightProvidersApi>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<databaseContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Database"),
                b =>
                {
                    b.CommandTimeout(120);
                }));

IoC.AddDependency(builder.Services);

builder.Services.AddAuthentication(options =>
           {
               options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
               options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
               options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
           }).AddCookie(options => { options.LoginPath = "/Account/Login"; });

builder.Services.AddMvc(options =>
 {
     options.EnableEndpointRouting = false;
 }).AddRazorPagesOptions(options =>
 {
     options.Conventions.AuthorizeFolder("/");
     options.Conventions.AllowAnonymousToPage("/Account/Login");
 });

builder.Services.AddWebMarkupMin(options =>
           {
               options.AllowMinificationInDevelopmentEnvironment = true;
               options.AllowCompressionInDevelopmentEnvironment = true;
           })
           .AddHtmlMinification(options =>
           {
               options.MinificationSettings.RemoveRedundantAttributes = true;
               options.MinificationSettings.RemoveHttpProtocolFromAttributes = true;
               options.MinificationSettings.RemoveHttpsProtocolFromAttributes = true;
           }).
           AddHttpCompression();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseMiddleware<AppMiddleware>();
app.UseSession();
app.UseWebMarkupMin();

app.UseCors(options =>
           {
               options.AllowAnyOrigin();
               options.AllowAnyMethod();
               options.AllowAnyHeader();
           });

app.UseMvc(routes =>
 {
     routes.MapRoute(
         name: "default",
         template: "{controller=Home}/{action=Index}/{id?}");

     routes.MapRoute(
        name: "api_setAutentication",
        template: "{controller=Shippings}/api/{action=setAutentication}/{json?}");

     routes.MapRoute(
         name: "api_CreateShipping",
         template: "{controller=Shippings}/api/{action=CreateShipping}/{user?}/{passwordhash?}/{json?}");
     routes.MapRoute(
        name: "rastrearOrden",
        template: "{agency}",
        defaults: new { controller = "P", action = "RastrearOrden" }
        );
 });

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
