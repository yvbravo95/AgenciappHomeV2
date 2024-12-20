using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class ResponseForm
    {
        [Key]
        public Guid Id { get; set; }
        public string data { get; set; }
        public string Identifier { get; set; }
        public DateTime date { get; set; }
        public formBuilder formBuilder { get; set; }
    }
}
