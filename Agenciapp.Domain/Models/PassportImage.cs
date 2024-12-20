using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class PassportImage:ImageClient
    {
        
        public DateTime ExpirationDate { get; set; }
    }
}
