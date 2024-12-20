using Agenciapp.Common.Contrains;
using Agenciapp.Service.IClientServices;
using Agenciapp.Service.IComboServices;
using Agenciapp.Service.IComboServices.Models;
using Agenciapp.Service.IContactServices;
using Agenciapp.Service.IWholesalerServices;
using Agenciapp.Service.IWholesalerServices.Models;
using Agenciapp.Service.ReyEnviosStore.Product;
using Agenciapp.Service.ReyEnviosStore.Product.Models;
using Agenciapp.Service.ReyEnviosStore.WooCommerce.Models;
using AgenciappHome.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.OpenXmlFormats.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Agenciapp.Service.ReyEnviosStore.WooCommerce
{
    public interface IWooCommerceService
    {
        Task<ProductItem> GetProduct(Guid agencyId, string id);
        Task<List<ProductItem>> GetAllProducts(Guid agencyId, string province, int? category);
        Task<List<CategoryItem>> GetCategories(int page, int length);
        Task Create(User user, CreateOrderWooCommerce dto);
        Task Cancel(Guid orderId);
    }

    public class WooCommerceService : IWooCommerceService
    {
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string _apiSecret;

        private readonly IWholesalerService _wholesalerService;
        private readonly IProductService _productService;
        private readonly IComboService _comboService;
        private readonly IContactService _contactServices;
        private readonly IClientService _clientService;
        private readonly databaseContext _context;

        public WooCommerceService(IConfiguration configuration,
            IWholesalerService wholesalerService,
            IProductService productService,
            IComboService comboService,
            IContactService contactServices,
            IClientService clientService,
            databaseContext context)
        {
            _baseUrl = configuration["WooCommerce:BaseUrl"];
            _apiKey = configuration["WooCommerce:ApiKey"];
            _apiSecret = configuration["WooCommerce:ApiSecret"];

            _wholesalerService = wholesalerService;
            _productService = productService;
            _comboService = comboService;
            _contactServices = contactServices;
            _clientService = clientService;
            _context = context;
        }

        public async Task<ProductItem> GetProduct(Guid agencyId, string id)
        {
            var setting = _context.SettingRetailsStore.FirstOrDefault(x => x.AgencyId.Equals(agencyId) && x.StoreType == Domain.Enums.StoreType.ReyEnvios);
            if (setting == null) throw new Exception("No se ha encontrado la configuracion de la tienda.");

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_apiKey}:{_apiSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/wp-json/wc/v3/products/{id}");
            request.Headers.Add("Authorization", $"Basic {auth}");
            var client = new HttpClient() { BaseAddress = new Uri(_baseUrl) };
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("No se ha podido recuperar la informacion del producto.");
            }

            var stringResponse = response.Content.ReadAsStringAsync().Result;
            var product = JsonConvert.DeserializeObject<ProductItem>(stringResponse);

            // apply fee
            product.SetFeeAgencia(setting.FeeRetail);
            product.SetFeeWholesaler(setting.FeeWholesaler);

            return product;
        }


        public async Task<ProductItem> GetProductBySku(Guid agencyId, string sku)
        {
            var setting = _context.SettingRetailsStore.FirstOrDefault(x => x.AgencyId.Equals(agencyId) && x.StoreType == Domain.Enums.StoreType.ReyEnvios);
            if (setting == null) throw new Exception("No se ha encontrado la configuracion de la tienda.");

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_apiKey}:{_apiSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/wp-json/wc/v3/products?sku={sku}");
            request.Headers.Add("Authorization", $"Basic {auth}");
            var client = new HttpClient() { BaseAddress = new Uri(_baseUrl) };
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("No se ha podido recuperar la informacion del producto.");
            }

            var stringResponse = response.Content.ReadAsStringAsync().Result;
            var products = JsonConvert.DeserializeObject<List<ProductItem>>(stringResponse);

            if (products.Count == 0) return null;
            var product = products[0];

            // apply fee
            product.SetFeeAgencia(setting.FeeRetail);
            product.SetFeeWholesaler(setting.FeeWholesaler);

            return product;
        }

        public async Task<List<ProductItem>> GetAllProducts(Guid agencyId, string provinceId, int? category)
        {
            var setting = _context.SettingRetailsStore.FirstOrDefault(x => x.AgencyId.Equals(agencyId) && x.StoreType == Domain.Enums.StoreType.ReyEnvios);
            if (setting == null) throw new Exception("No se ha encontrado la configuracion de la tienda.");

            var province = _context.Provincia.FirstOrDefault(x => x.Id == Guid.Parse(provinceId));

            string url = $"{_baseUrl}/wp-json/wpmf/v1/products?username=jsons&password=yhhsm$2.ei7assa&province={MapProvince(province.nombreProvincia)}";
            if (category.HasValue)
            {
                url += $"&category={category}";
            }
            //var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{"admin"}:{"H4Tumbrella#6"}"));
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            //request.Headers.Add("Authorization", $"Basic {auth}");
            var client = new HttpClient() { BaseAddress = new Uri(_baseUrl) };
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("No se han podido obtener los productos.");
            }

            var stringResponse = response.Content.ReadAsStringAsync().Result;
            var products = JsonConvert.DeserializeObject<List<ProductItem>>(stringResponse);
            products = products.Where(x => x.Catalog_visibility == "visible").ToList();
                
            List<ProductItem> data = new List<ProductItem>();
            foreach (var product in products)
            {
                if (product.StockQuantity == 0) continue;

                decimal.TryParse(product.MetaData.FirstOrDefault(x => x.Key.Equals("costo"))?.Value as string, out decimal cost);
                var providers = (product.MetaData.FirstOrDefault(x => x.Key.Equals("proveedores")).Value as JArray).ToArray();

                if (providers.Length == 0) continue;

                var supplierResult = await _wholesalerService.GetWholesalerByName(providers.ElementAt(0).Value<string>(), "Combos", AgencyName.ReyEnvios);
                if (supplierResult.IsFailure) continue;

                //shipping
                var shipping = supplierResult.Value.CostByProvinces.FirstOrDefault(x => x.Province.Id.Equals(Guid.Parse(provinceId)) && x.Type == Supplier.TypeCost.Combo);
                if (shipping == null) continue;

                product.SupplierId = supplierResult.Value.Id;
                product.SupplierName = supplierResult.Value.Name;
                product.ShippingCost = shipping?.Cost ?? 0;

                // apply fee
                product.SetFeeAgencia(setting.FeeRetail);
                product.SetFeeWholesaler(setting.FeeWholesaler);

                data.Add(product);
            }

            return data;
        }

        public async Task<List<CategoryItem>> GetCategories(int page, int length)
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_apiKey}:{_apiSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/wp-json/wc/v3/products/categories?per_page={length}&page={page}");
            request.Headers.Add("Authorization", $"Basic {auth}");
            var client = new HttpClient() { BaseAddress = new Uri(_baseUrl) };
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("No se han podido obtener las categorias.");
            }

            var stringResponse = response.Content.ReadAsStringAsync().Result;
            var categories = JsonConvert.DeserializeObject<List<CategoryItem>>(stringResponse);
            return categories;
        }

        public async Task<ProductItem> UpdateStock(string id, int quantity)
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_apiKey}:{_apiSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/wp-json/wc/v3/products/{id}");
            request.Headers.Add("Authorization", $"Basic {auth}");
            var client = new HttpClient() { BaseAddress = new Uri(_baseUrl) };
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("No se ha podido recuperar la informacion del producto.");
            }

            var stringResponse = response.Content.ReadAsStringAsync().Result;
            var product = JsonConvert.DeserializeObject<ProductItem>(stringResponse);

            product.StockQuantity = quantity;

            var json = JsonConvert.SerializeObject(new { stock_quantity = quantity });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var putRequest = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/wp-json/wc/v3/products/{id}");
            putRequest.Headers.Add("Authorization", $"Basic {auth}");
            putRequest.Content = content;
            var putResponse = await client.SendAsync(putRequest);
            if (!putResponse.IsSuccessStatusCode)
            {
                throw new Exception("No se ha podido actualizar el stock del producto.");
            }

            return product;
        }

        public async Task Create(User user, CreateOrderWooCommerce dto)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var setting = _context.SettingRetailsStore.FirstOrDefault(x => x.AgencyId.Equals(user.AgencyId) && x.StoreType == Domain.Enums.StoreType.ReyEnvios);
                    if (setting == null) throw new Exception("No se ha encontrado la configuracion de la tienda.");

                    Contact contact = await _contactServices.Get(dto.ContactId) ?? throw new Exception("El contacto no fue encontrado.");
                    Client client = await _clientService.Get(dto.ClientId) ?? throw new Exception("El cliente no fue encontrado.");

                    List<dynamic> productsToUpdate = new List<dynamic>();
                    List<dynamic> products = new List<dynamic>();
                    foreach (var product in dto.Products)
                    {
                        ProductItem shopProduct = await GetProduct(user.AgencyId, product.Id) ?? throw new Exception($"El producto {product.Id} no fue encontrado.");
                        if (shopProduct.StockQuantity < product.Quantity)
                        {
                            throw new Exception($"No hay suficiente stock para el producto {shopProduct.Name}");
                        }

                        decimal.TryParse(shopProduct.MetaData.FirstOrDefault(x => x.Key.Equals("costo"))?.Value as string, out decimal cost);
                        var providers = (shopProduct.MetaData.FirstOrDefault(x => x.Key.Equals("proveedores")).Value as JArray).ToArray();

                        if (providers.Length == 0) continue;

                        var supplierResult = await _wholesalerService.GetWholesalerByName(providers.ElementAt(0).Value<string>(), "Combos", AgencyName.ReyEnvios);
                        if (supplierResult.IsFailure) continue;

                        //shipping
                        var shipping = supplierResult.Value.CostByProvinces.FirstOrDefault(x => x.Province.Name.Equals(contact.Address.City) && x.Type == Supplier.TypeCost.Combo);
                        if (shipping == null) throw new Exception($"No se ha encontrado el costo de envio para la provincia {contact.Address.City}.");

                        var provinces = supplierResult.Value.CostByProvinces.Select(x => new Province { Id = x.Province.Id, Name = x.Province.Name }).ToList();

                        //crear o actualizar producto
                        var productResult = await _productService.CreateUpdateProduct(AgencyName.ReyEnvios, new ProductStore
                        {
                            Availability = shopProduct.StockQuantity,
                            Category = "Combos",
                            Code = product.Sku,
                            Description = shopProduct.Description,
                            IsAvailable = true,
                            Name = shopProduct.Name,
                            Provinces = provinces,
                            PurchasePrice = cost,
                            SalePrice = shopProduct.FarePrice ?? decimal.Zero,
                            Supplier = supplierResult.Value,
                            Shipping = shipping?.Cost ?? decimal.Zero
                        });

                        shopProduct.ShippingCost = shipping?.Cost ?? decimal.Zero;
                        shopProduct.SupplierId = supplierResult.Value.Id;
                        shopProduct.SupplierName = supplierResult.Value.Name;

                        if (productResult.IsFailure)
                            throw new Exception($"No se ha podido actualizar el producto {shopProduct.Sku}: {shopProduct.Name}");

                        var productBd = await _productService.GetProduct(AgencyName.ReyEnvios, product.Sku);
                        productsToUpdate.Add(new { id = product.Id, quantity = shopProduct.StockQuantity - product.Quantity });
                        products.Add(new { Id = (Guid)productBd.Value.Id, Qty = product.Quantity, Supplier = shopProduct.SupplierId, ShopProduct = shopProduct });
                    }

                    var wholesaler = _context.Wholesalers.Where(x => x.AgencyId == user.AgencyId && x.byTransferencia && x.Category.category == "Combos" && x.name == "Rey Envios").FirstOrDefault();
                    if (wholesaler == null) throw new Exception("No se ha encontrado un mayorista por transferencia para la categoria Combos.");

                    // crear un tramite por cada proveedor
                    bool applyServiceCost = false;
                    foreach (var itemsBySupplier in products.GroupBy(x => x.Supplier))
                    {
                        decimal price = itemsBySupplier.Sum(x => (int)x.Qty * (decimal)x.ShopProduct.Price);
                        decimal shippingCost = itemsBySupplier.FirstOrDefault().ShopProduct.ShippingCost;
                        decimal serviceCost = setting.ServiceCost;
                        decimal amount = price + shippingCost + serviceCost;
                        var order = await _comboService.Create(new CreateComboModel
                        {
                            StoreType = Domain.Enums.StoreType.ReyEnvios,
                            Editable = false,
                            AddCosto = 0,
                            AddPrecio = 0,
                            AgencyId = user.AgencyId,
                            Amount = amount,
                            ClientId = client.ClientId,
                            ContactId = contact.ContactId,
                            CostService = serviceCost,
                            Credito = 0,
                            Express = false,
                            Nota = "",
                            Pays = new List<Pay>(),
                            Products = itemsBySupplier.Select(x => new WarehouseProduct
                            {
                                Id = x.Id,
                                Qty = x.Qty
                            }).ToList(),
                            ProductsPrice = price,
                            Shipping = shippingCost,
                            UserId = user.UserId,
                            AuthorizationCard = new AuthorizationCard()
                        });

                        if (order.IsSuccess)
                        {
                            var servicioPorPagar = _context.ServiciosxPagar.FirstOrDefault(x => x.SId == order.Value.OrderId && x.Agency.AgencyId == user.AgencyId && x.Mayorista.IdWholesaler == wholesaler.IdWholesaler && !x.IsPaymentProductShipping);
                            servicioPorPagar.ImporteAPagar = itemsBySupplier.Sum(x => (int)x.Qty * (decimal)x.ShopProduct.Cost) + order.Value.OtrosCostos + shippingCost;
                            _context.Update(servicioPorPagar);

                            var servicioPorCobrar = _context.servicioxCobrar.FirstOrDefault(x => x.ServicioId == order.Value.OrderId && x.minorista.AgencyId == user.AgencyId && x.mayorista.AgencyId == AgencyName.ReyEnvios);
                            servicioPorCobrar.importeACobrar = itemsBySupplier.Sum(x => (int)x.Qty * (decimal)x.ShopProduct.Cost) + order.Value.OtrosCostos + shippingCost;
                            _context.Update(servicioPorCobrar);

                            _context.SaveChanges();
                        }
                        else
                        {
                            throw new Exception("No se ha podido crear el combo.");
                        }
                    }

                    // actualizar stock
                    foreach (var product in productsToUpdate)
                    {
                        await UpdateStock(product.id, product.quantity);
                    }
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw;
                }
            }

        }

        public async Task Cancel(Guid orderId)
        {
            var order = await _context.Order
                .Include(x => x.Bag).ThenInclude(x => x.BagItems).ThenInclude(x => x.Product).ThenInclude(x => x.ProductoBodega)
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (order == null) throw new Exception("No se ha encontrado la orden.");

            List<dynamic> productToUpdate = new List<dynamic>();

            foreach (var item in order.Bag.SelectMany(x => x.BagItems))
            {
                var productStore = await GetProductBySku(order.AgencyId, item.Product.ProductoBodega.Codigo);
                if (productStore == null) throw new Exception($"No se ha encontrado el producto {item.Product.ProductoBodega.Codigo}");
                productToUpdate.Add(new
                {
                    id = productStore.Id,
                    quantity = productStore.StockQuantity + item.Qty
                });
            }

            foreach (var product in productToUpdate)
            {
                await UpdateStock(product.id, product.quantity);
            }
        }

        private string CleanHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc.DocumentNode.InnerText.Trim();
        }

        private string MapProvince(string name)
        {
            switch (name)
            {
                case "Pinar Del Rio":
                    return "Pinar del Río";
                case "La Habana":
                    return "La Habana";
                case "Mayabeque":
                    return "Mayabeque";
                case "Artemisa":
                    return "Artemisa";
                case "Matanzas":
                    return "Matanzas";
                case "Villa Clara":
                    return "Villa Clara";
                case "Cienfuegos":
                    return "Cienfuegos";
                case "Sancti Spíritus":
                    return "Sancti Spíritus";
                case "Ciego de Ávila":
                    return "Ciego de Ávila";
                case "Camaguey":
                    return "Camagüey";
                case "Las Tunas":
                    return "Las Tunas";
                case "Granma":
                    return "Granma";
                case "Holguín":
                    return "Holguín";
                case "Santiago de Cuba":
                    return "Santiago de Cuba";
                case "Guantánamo":
                    return "Guantánamo";
                case "Isla de la Juventud":
                    return "Isla de la Juventud";
                default:
                    throw new Exception($"La provincia {name} no fue encontrada.");
            }
        }

    }
}
