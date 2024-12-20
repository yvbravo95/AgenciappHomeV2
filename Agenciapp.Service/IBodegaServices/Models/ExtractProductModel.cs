using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.IBodegaServices.Models
{
    public class ExtractProductModel
    {
        public ProductoBodega Product { get; set; }
        public int Qty { get; set; }
    }
}
