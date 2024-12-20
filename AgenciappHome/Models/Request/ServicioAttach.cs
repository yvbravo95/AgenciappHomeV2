using System;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models.Request
{
    public class ServicioAttach
    {
        [Required] public Guid ServicioId { get; set; }
        public string Description { get; set; }
    }
}