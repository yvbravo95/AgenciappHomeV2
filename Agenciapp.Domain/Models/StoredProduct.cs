using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class StoredProduct
    {
        [Key]
        public string BarcodeNumber { get; set; }
        public string BarcodeFormats { get; set; }
        public string Mpn { get; set; }
        public string Model { get; set; }
        public string Title { get; set; }
        public string Title_es { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Weight { get; set; }
        public bool IsTranslated { get; set; }
        public User User { get; set; }
        public Guid? UserId { get; set; }
    }
}
