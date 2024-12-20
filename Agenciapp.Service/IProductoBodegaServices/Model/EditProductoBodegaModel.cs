using AgenciappHome.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Service.IProductoBodegaServices.Model
{
    public class EditProductoBodegaModel
    {
        public Guid AgencyId { get; set; }
        public Guid IdProducto { get; set; }
        public bool IsAvailable { get; set; }
        [Required]
        [Display(Name = "Categoría")]
        public Guid IdCategoria { get; set; }
        public List<ProductoBodegaCatalogItem> productoBodegaCatalogItems { get; set; }
        [Required]
        [Display(Name = "Unidad de Medida")]
        public Guid IdUnidadMedida { get; set; }
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
        [Display(Name = "Precio Compra Referencial")]
        public decimal? PrecioCompraReferencial { get; set; }
        [Display(Name = "Precio Venta Referencial")]
        public decimal? PrecioVentaReferencial { get; set; }
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }
        [Display(Name = "Ficha Técnica")]
        public string FichaTecnica { get; set; }
        [Display(Name = "Código EAN")]
        public string CodigoEan { get; set; }
        public Guid IdProveedor { get; set; } // Para el caso de Combos
        public Guid? Precio1MinoristaId { get; set; }
        public Guid? Precio2MinoristaId { get; set; }
        public Guid? Precio3MinoristaId { get; set; }

        public IFormFile ImageFile { get; set; }
        public Guid[] agenciasprecio1 { get; set; }
        public Guid[] agenciasprecio2 { get; set; }
        public Guid[] agenciasprecio3 { get; set; }
        public List<Guid> CatalogItems { get; set; }
    }
}
