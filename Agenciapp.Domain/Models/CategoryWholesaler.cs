using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class CategoryWholesaler
    {
        [Key]
        public Guid IdWholesaler { get; set; }
        [Key]
        public Guid IdCategory { get; set; }
        public Wholesaler wholesaler { get; set; }
        public Category category { get; set; }
    }
}
