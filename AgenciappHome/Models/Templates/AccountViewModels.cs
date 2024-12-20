using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{

    public class RegisterViewModel
    {
        //datos personales
        [Required(ErrorMessage = "El campo de Nombre es requerido.")]
        [Display(Name = "Firstname")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "El campo de Apellidos es requerido.")]
        [Display(Name = "Lastname")]
        public string Lastname { get; set; }

        [Required(ErrorMessage = "El campo de Usuario es requerido.")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "El campo de Correo es requerido.")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo de Contraseña es requerido.")]
        [StringLength(100, ErrorMessage = "El {0} debe tener al menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El campo de confirmación de Contraseña es requerido.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "La contraseña y la contraseña de confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }

        //datos de la agencia
        [Required(ErrorMessage = "El campo de Nombre es requerido.")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El campo de Nombre Legal es requerido.")]
        [Display(Name = "LegalName")]
        public string LegalName { get; set; }

        [Required(ErrorMessage = "El campo de Tipo es requerido.")]
        [Display(Name = "Type")]
        public string Type { get; set; }

        [Required(ErrorMessage = "El campo de Teléfono es requerido.")]
        [Display(Name = "PhoneNumber")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "El campo de Tipo de Teléfono es requerido.")]
        [Display(Name = "TypePhone")]
        public string TypePhone { get; set; }

        [Required(ErrorMessage = "El campo de País es requerido.")]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required(ErrorMessage = "El campo de Ciudad es requerido.")]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessage = "El campo de Estado es requerido.")]
        [Display(Name = "State")]
        public string State { get; set; }

        [Required(ErrorMessage = "El campo de Zip es requerido.")]
        [Display(Name = "Zip")]
        public string Zip { get; set; }

        [Required(ErrorMessage = "El campo de Dirección es requerido.")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "El campo de Tipo de Dirección es requerido.")]
        [Display(Name = "TypeAddress")]
        public string TypeAddress { get; set; }

        public List<Guid> Mayoristas { get; set; }

        [Display(Name = "Usuario Reserva")]
        public string UserTicket { get; set; }

        [Display(Name = "Usuario Reserva 2")]
        public string UserTicket2 { get; set; }

        [Display(Name = "Usuario Reserva 3")]
        public string UserTicket3 { get; set; }

        [Display(Name = "Usuario Reserva 4")]
        public string UserTicket4 { get; set; }

        public List<Guid> UserDeliveryId { get; set; }
        public List<Guid> UserDistributorsId { get; set; }
        public List<Guid> UserAgencies { get; set; }
        public string Wharehouse { get; set; }

    }

    public class EditUserViewModel
    {
        [Required(ErrorMessage = "El campo de Nombre es requerido.")]
        [Display(Name = "Firstname")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "El campo de Apellidos es requerido.")]
        [Display(Name = "Lastname")]
        public string Lastname { get; set; }

        [Required(ErrorMessage = "El campo de Usuario es requerido.")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "El campo de Correo es requerido.")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "El {0} debe tener al menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "La contraseña y la contraseña de confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }

        public string type { get; set; }
        public List<Guid> Mayoristas { get; set; }

        [Display(Name = "Usuario Reserva")]
        public string UserTicket { get; set; }

        [Display(Name = "Usuario Reserva 2")]
        public string UserTicket2 { get; set; }

        [Display(Name = "Usuario Reserva 3")]
        public string UserTicket3 { get; set; }

        [Display(Name = "Usuario Reserva 4")]
        public string UserTicket4 { get; set; }

        public List<Guid> UserDeliveryId { get; set; }
        public List<Guid> UserDistributorsId { get; set; }
        public List<Guid> UserAgencies { get; set; }
        public string Wharehouse { get; set; }

    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "El campo de Usuario es requerido.")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "El campo de Contraseña es requerido.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
