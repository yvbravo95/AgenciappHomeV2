using Agenciapp.Domain.Models;
using Agenciapp.Service.BarcodeLookupServices.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agenciapp.Service.BarcodeLookupServices.Mappers
{
    public class ProductMapper: Profile
    {
        public ProductMapper()
        {
            CreateMap<Product, StoredProduct>()
                .ForMember(x => x.BarcodeFormats, y => y.MapFrom(z => z.barcode_formats))
                .ForMember(x => x.BarcodeNumber, y => y.MapFrom(z => z.barcode_number))
                .ForMember(x => x.Brand, y => y.MapFrom(z => z.brand))
                .ForMember(x => x.Category, y => y.MapFrom(z => z.category))
                .ForMember(x => x.Color, y => y.MapFrom(z => z.color))
                .ForMember(x => x.Description, y => y.MapFrom(z => z.description))
                .ForMember(x => x.Model, y => y.MapFrom(z => z.model))
                .ForMember(x => x.Mpn, y => y.MapFrom(z => z.mpn))
                .ForMember(x => x.Title, y => y.MapFrom(z => z.title))
                .ForMember(x => x.ImageUrl, y => y.MapFrom(z => z.images.FirstOrDefault()))
                .ForMember(x => x.Weight, y => y.MapFrom(z => z.weight));
        }
    }
}
