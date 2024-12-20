using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Request.ProductoBodega
{
    public class CreateSettingMinoristaProduct
    {
        public Guid ProductId { get; set; }
        public bool Visibility { get; set; }
        public string AliasName { get; set; }
        public decimal Price { get; set; }
    }
}
