using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class formBuilder
    {
        public formBuilder()
        {
            responseForms = new List<ResponseForm>();
        }
        
        [Key]
        public Guid Id { get; set; }
        public string name { get; set; }
        public string Identifier { get; set; }
        public string data { get; set; }
        public List<ResponseForm> responseForms { get; set; }
        public Office Office { get; set; }
    }
}
