using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class ImagenPromotion
    {
        [Key]
        public Guid ImagenPromotionId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public Guid AgencyId { get; set; }
        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }
    }
    public class ImagenPromotionRapidApp: ImagenPromotion
    {
        public TypeImagenPromotion Type { get; set; }
        public Guid? TypeIdReference { get; set; }
    }

    public class ImagenPromotionPassport: ImagenPromotion
    {

    }

    public enum TypeImagenPromotion
    {

        None,
        Landing,
        Catalog,
        Product
    }
}
