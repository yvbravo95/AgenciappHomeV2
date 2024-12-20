using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AgenciappHome.Models;

namespace BodegaApp.ViewModels
{
    public class ProductInventoryViewModel
    {
        public Guid ProductId { get; set; }

        [Display(Name = "Bodega")]
        public string Bodega { get; set; }

        [Display(Name = "Producto")]
        public string Producto { get; set; }

        [Display(Name = "Cantidad")]
        public decimal Cantidad { get; set; }

        [Display(Name = "Precio")]
        public decimal Monto { get; set; }
        public string Descripcion { get; internal set; }

        [Display(Name = "Provincias")]
        public List<string> Provinces { get; set; }

        public bool ByTransferencia { get; set; }
        public Agency AgencyTransferida { get; set; } 
        public SettingMinoristaProduct SettingMinorista { get; set; }
    }
}
