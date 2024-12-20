using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class OfficeTemplate
    {
        [Required(ErrorMessage = "El campo de Nombre es requerido.")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El campo de Tipo de Teléfono es requerido.")]
        [Display(Name = "TypePhone")]
        public string TypePhone { get; set; }

        [Required(ErrorMessage = "El campo de Teléfono es requerido.")]
        [Display(Name = "Phone")]
        public string Phone { get; set; }
        
        [Display(Name = "AgencyId")]
        public Guid AgencyId { get; set; }

        [Required(ErrorMessage = "El campo de Tipo de Dirección es requerido.")]
        [Display(Name = "TypeAddress")]
        public string TypeAddress { get; set; }

        [Required(ErrorMessage = "El campo de Dirección es requerido.")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "El campo de Ciudad es requerido.")]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessage = "El campo de Pais es requerido.")]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required(ErrorMessage = "El campo de Estado es requerido.")]
        [Display(Name = "State")]
        public string State { get; set; }

        [Required(ErrorMessage = "El campo de Zip es requerido.")]
        [Display(Name = "Zip")]
        public string Zip { get; set; }
    }
}
