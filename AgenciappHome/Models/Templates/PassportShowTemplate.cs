using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public class PassportShowTemplate
    {
        public string fullname { get; set; }
        public string nationality { get; set; }
        public string country1 { get; set; }
        public string country2 { get; set; }
        public string numpassport1 { get; set; }
        public string numpassport2 { get; set; }
        public DateTime datebirth { get; set; }
    }
}
