using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class Country
    {
        [Key]public string Id { get; set; }
        public string Value { get; set; }
    }
}
