using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class Category
    {
        public Category()
        {
            
        }
        [Key]
        public Guid IdCategory { get; set; }
        public string category { get; set; }
        
        

    }
}
