using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class ClientTemplate
    {
        [Required(ErrorMessage = "El campo de Nombre es requerido.")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El campo de Apellidos es requerido.")]
        [Display(Name = "Lastname")]
        public string Lastname { get; set; }

        [Required(ErrorMessage = "El campo de Nombre es requerido.")]
        [Display(Name = "Name 2")]
        public string Name2 { get; set; }

        [Required(ErrorMessage = "El campo de Apellidos es requerido.")]
        [Display(Name = "Lastname 2")]
        public string Lastname2 { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "El campo de Teléfono es requerido.")]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Display(Name = "ID")]
        public string ID { get; set; }

        [Display(Name = "phoneCuba")]
        public string phoneCuba { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }
        
        [Display(Name = "Country")]
        public string State { get; set; }
        
        [Display(Name = "City")]
        public string City { get; set; }
        
        [Display(Name = "Zip")]
        public string Zip { get; set; }
        
        [Display(Name = "Fecha de Nacimiento")]
        public string FechaNac { get; set; }

        [Display(Name = "Numero de Pasaporte")]
        public string PassportNumber { get; set; }

        [Display(Name = "Fecha de Expiracion")]
        public string PassportExpireDate { get; set; }
    }
}
