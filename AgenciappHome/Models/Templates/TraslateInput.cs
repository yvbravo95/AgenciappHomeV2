using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BodegaApp.ViewModels
{
    public class TraslateInput
    {
        public string Bodega { set; get; }
        public string Bodega2 { set; get; }
        public List<TraslateProductoInput> Productos { set; get; }
    }
}
