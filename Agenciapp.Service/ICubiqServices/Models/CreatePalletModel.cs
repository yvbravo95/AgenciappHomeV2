using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Service.ICubiqServices.Models
{
    public class CreatePalletModel
    {
        [Required]
        [Range(1, 4, ErrorMessage = "El número de pallets debe estar entre 1 y 4.")]
        public int QtyPallets { get; set; }
    }
}
