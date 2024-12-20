using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BodegaApp.ViewModels
{
    public class AjustarProductoInput
    {
        public string Bodega { set; get; }
        public List<AjustarProductoDetail> Productos { set; get; }
    }
}
