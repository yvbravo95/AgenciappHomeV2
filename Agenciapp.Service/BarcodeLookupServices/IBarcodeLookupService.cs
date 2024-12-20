using Agenciapp.Domain.Models;
using Agenciapp.Service.BarcodeLookupServices.Models;
using Agenciapp.Service.HttpServices;
using Agenciapp.Service.TranslateService;
using AgenciappHome.Models;
using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Agenciapp.Service.BarcodeLookupServices
{
    public interface IBarcodeLookupService
    {
        Task<Result<StoredProduct>> FindByBarcode(string barcode);

        Task<Result<StoredProduct>> AddProduc(BarCodeProductoDto product, string userName);
    }

    public class BarcodeLookupService: IBarcodeLookupService
    {
        private readonly databaseContext _context;
        private readonly IConfiguration _configuration;
        private readonly ITranslateService _translate;
        private readonly IMapper _mapper;
        public BarcodeLookupService(databaseContext context, 
            IConfiguration configuration, 
            IMapper mapper, ITranslateService translateService)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
            _translate = translateService;
        }

        public async Task<Result<StoredProduct>> AddProduc(BarCodeProductoDto product, string userName)
        {
            var user = _context.User.FirstOrDefault(x => x.Username == userName);
            var p = new StoredProduct
            {
                User = user,
                BarcodeNumber = product.Code,
                Title = product.Name                    
            };

            _context.StoredProducts.Add(p);
            await _context.SaveChangesAsync();

            return p;
        }

        public async Task<Result<StoredProduct>> FindByBarcode(string barcode)
        {
            StoredProduct product = await _context.StoredProducts.FindAsync(barcode.Trim());
            if (product == null)
            {
                Root product_root = await FindOnlineByBarcode(barcode);
                if (product_root == null)
                    return Result.Failure<StoredProduct>("El producto no fué encontrado");

                foreach (var item in product_root.products)
                {
                    product = _mapper.Map<StoredProduct>(item);
                    _context.Add(product);
                }
                await _context.SaveChangesAsync();
            }

            if (!product.IsTranslated)
            {
                product.Title_es = await _translate.Translate(TranslateService.Models.SupportedLanguages.es, product.Title);
                product.IsTranslated = true;
                _context.StoredProducts.Update(product);
                await _context.SaveChangesAsync();
            }

            return Result.Success(product);
        }

        private async Task<Root> FindOnlineByBarcode(string barcode)
        {
            try
            {
                using (WebClient webClient = new System.Net.WebClient())
                {
                    WebClient n = new WebClient();
                    string api_key = _configuration["BarcodeLookupKey"];
                    string url = $"https://api.barcodelookup.com/v3/products?barcode={barcode}&formatted=y&key={api_key}";
                    var data = n.DownloadString(url);
                    var root = JsonConvert.DeserializeObject<Root>(data);
                    return root;
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return null;
            }
        }

    }

    public class BarCodeProductoDto
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
