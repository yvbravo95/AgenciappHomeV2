using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.ApiModel.Responses
{
    public class Carousel
    {
        public string Url { get; set; }
        public Guid? ReferenceId { get; set; }
        public TypeImagenPromotion Type { get; set; }
    }
}
