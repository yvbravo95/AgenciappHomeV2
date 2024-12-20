using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public class PassportTemplate
    {
        [Required(ErrorMessage = "El campo de Cliente es requerido.")]
        public Guid ClientId { get; set; }
        
        [Required(ErrorMessage = "El campo de Nacionalidad es requerido.")]
        [Display(Name = "Nacionalidad")]
        public string Nationality { get; set; }
        
        [Required(ErrorMessage = "El campo de Fecha de Nacimiento es requerido.")]
        [Display(Name = "DateBirth")]
        public decimal DateBirth { get; set; }
        
        [Required(ErrorMessage = "El campo de Sexo es requerido.")]
        [Display(Name = "Sex")]
        public decimal Sex { get; set; }
        
        [Required(ErrorMessage = "El campo de OFAC es requerido.")]
        [Display(Name = "OFAC")]
        public decimal OFAC { get; set; }
        
        [Required(ErrorMessage = "El campo de Número de Pasaporte1 es requerido.")]
        [Display(Name = "NumPassport1")]
        public decimal NumPassport1 { get; set; }
        
        [Required(ErrorMessage = "El campo de Número de Pasaporte2 es requerido.")]
        [Display(Name = "NumPassport2")]
        public decimal NumPassport2 { get; set; }
        
        [Required(ErrorMessage = "El campo de Pais de Pasaporte1 es requerido.")]
        [Display(Name = "CountryPassport1")]
        public decimal CountryPassport1 { get; set; }
        
        [Required(ErrorMessage = "El campo de Pais de Pasaporte2 es requerido.")]
        [Display(Name = "CountryPassport2")]
        public decimal CountryPassport2 { get; set; }
        
        [Required(ErrorMessage = "El campo de Fecha de Vencimiento de Pasaporte1 es requerido.")]
        [Display(Name = "ExpirePassport1")]
        public decimal ExpirePassport1 { get; set; }
        
        [Required(ErrorMessage = "El campo de Fecha de Vencimiento de Pasaporte2 es requerido.")]
        [Display(Name = "ExpirePassport2")]
        public decimal ExpirePassport2 { get; set; }
    }
}
