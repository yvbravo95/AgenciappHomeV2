using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{
    public class Bodega
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }
        public Guid IdOffice { get; set; }
        public Office Office { get; set; }
        public Guid idAgency { get; set; }
    }
}