using AgenciappHome.Models.ApiModel;
using RapidMultiservice.Models.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AgenciappHome.Models
{
    public class ProductoBodega
    {
        public ProductoBodega()
        {
            esVisible = true;
            CreatedAt = DateTime.Now;
            Products = new List<Product>();
            BodegaProductos = new List<BodegaProducto>();
        }

        [Key]
        public Guid IdProducto { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid IdAgency { get; set; }

        [Required]
        [Display(Name = "Categoría")]
        public Guid IdCategoria { get; set; }

        [Display(Name = "Categoría")]
        public CategoriaBodega Categoria { get; set; }

        public List<ProductoBodegaCatalogItem> productoBodegaCatalogItems { get; set; }
        //Para la api de la apk de rapid multiservice

        [Required]
        [Column("IdUnidad_Medida")]
        [Display(Name = "Unidad de Medida")]
        public Guid IdUnidadMedida { get; set; }

        [Display(Name = "Unidad de Medida")]
        public UnidadMedida UnidadMedida { get; set; }

        [Required]
        [StringLength(250)]
        public string Nombre { get; set; }

        public string Terms { get; set; }

        [StringLength(20)]
        [Display(Name = "Código")]
        public string Codigo { get; set; }

        [StringLength(50)]
        [Display(Name = "Ubicación")]
        public string Ubicacion { get; set; }

        [Column("Precio_Compra_Referencial", TypeName = "decimal(11, 2)")]
        [Display(Name = "Precio Compra Referencial")]
        public decimal? PrecioCompraReferencial { get; set; }

        [Column("Precio_Venta_Referencial", TypeName = "decimal(11, 2)")]
        [Display(Name = "Precio Venta Referencial")]
        public decimal? PrecioVentaReferencial { get; set; }

        [Column("Imagen_Producto")]
        [StringLength(50)]
        [Display(Name = "Imagen de Producto")]
        public string ImagenProducto { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Column("Ficha_Tecnica", TypeName = "text")]
        [Display(Name = "Ficha Técnica")]
        public string FichaTecnica { get; set; }

        [Column("Codigo_EAN", TypeName = "text")]
        [Display(Name = "Código EAN")]
        public string CodigoEan { get; set; }

        public Guid IdProveedor { get; set; } // Para el caso de Combos
        [Display(Name = "Proveedor")]
        public Wholesaler Proveedor { get; set; } //Para el caso de Combos

        public bool esVisible { get; set; } //Para el caso de Combos
        public bool IsVisibleAppRapid { get; set; } //Visibilidad para app de Rapid Multiservice
        public int OrderByRelevance { get; set; } //Para ordenar en app de Rapid Multiservice
        public bool isMoreSold { get; set; } //Para marcar los productos más vendidos

        public Guid? Precio1MinoristaId { get; set; }
        public PrecioRefMinorista Precio1Minorista { get; set; }
        public Guid? Precio2MinoristaId { get; set; }
        public PrecioRefMinorista Precio2Minorista { get; set; }
        public Guid? Precio3MinoristaId { get; set; }
        public PrecioRefMinorista Precio3Minorista { get; set; }

        public ICollection<InvoiceProductoBodega> InvoiceProductoBodega { get; set; }
        public List<SettingMinoristaProduct> SettingMinoristas { get; set; }
        public List<Product> Products { get; set; } //Para saber en cuantos tramites se ha vendido el producto
        public List<BodegaProducto> BodegaProductos { get; set; }
        public string Barcode { get; set; }
        public bool EnableShipping { get; set; }
        public decimal Shipping { get; set; }

        /// <summary>
        /// Obtener el precio de un producto
        /// </summary>
        /// <param name="agencyId"></param>
        /// <returns></returns>
        public decimal GetPrice(Agency agency)
        {
            if (IdAgency.Equals(agency.AgencyId))
                return PrecioVentaReferencial ?? decimal.Zero;

            var setting = SettingMinoristas.FirstOrDefault(x => x.Agency.AgencyId == agency.AgencyId);
            
            if(setting != null)
            {
                if (setting.Visibility)
                    return setting.Price;
            }
            else
            {
                if ((Precio1Minorista != null && Precio1Minorista.AgencyPrecioRefMinoristas.Any(x => x.AgencyId == agency.AgencyId) && Precio1Minorista.precio > 0))
                {
                    return Precio1Minorista.precio;
                }
                else if ((Precio2Minorista != null && Precio2Minorista.AgencyPrecioRefMinoristas.Any(x => x.AgencyId == agency.AgencyId) && Precio2Minorista.precio > 0))
                {
                    return Precio2Minorista.precio;
                }
                else if ((Precio3Minorista != null && Precio3Minorista.AgencyPrecioRefMinoristas.Any(x => x.AgencyId == agency.AgencyId) && Precio3Minorista.precio > 0))
                {
                    return Precio3Minorista.precio;
                }
            }

            return decimal.Zero;
        }
    }

    public class PrecioRefMinorista
    {
        public PrecioRefMinorista()
        {
            AgencyPrecioRefMinoristas = new List<AgencyPrecioRefMinorista>();
        }
        [Key]
        public Guid PrecioRefMinoristaId { get; set; }
        public decimal precio { get; set; }
        public List<AgencyPrecioRefMinorista> AgencyPrecioRefMinoristas { get; set; }
        public List<PriceProductByprovince> PriceByProvince { get; set; }
    }

    public class AgencyPrecioRefMinorista
    {
        [Key]
        public Guid AgencyPrecioRefMinoristaId { get; set; }
        public Guid PrecioRefMinoristaId { get; set; }
        [ForeignKey("PrecioRefMinoristaId")]
        public PrecioRefMinorista PrecioRefMinorista { get; set; }
        public Guid AgencyId { get; set; }
        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }

    }

    public class SettingMinoristaProduct
    {
        [Key] public Guid Id { get; set; }
        public bool Visibility { get; set; }
        public string AliasName { get; set; }
        public decimal Price { get; set; }
        public Agency Agency { get; set; }
        public ProductoBodega ProductoBodega { get; set; }
        public List<PriceProductByprovince> PriceByProvince { get; set; }
    }

    public class PriceProductByprovince
    {
        public Guid Id { get; set; }
        public Guid ProvinceId { get; set; }
        [ForeignKey("ProvinceId")]
        public Provincia Province { get; set; }
        public decimal Price { get; set; }
    }
}