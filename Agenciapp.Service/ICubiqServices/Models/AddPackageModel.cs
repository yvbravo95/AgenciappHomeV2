using System;
using System.ComponentModel.DataAnnotations;

namespace Agenciapp.Service.ICubiqServices.Models
{
    public class AddPackageModel
    {
        [Required]
        public Guid PalletId { get; set; }
        [Required]
        public string PackageNumber { get; set; }
    }
}