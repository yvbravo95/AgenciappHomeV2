using Agenciapp.Domain.Models;
using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AgenciappHome.Models
{
    public partial class User
    {
        public User()
        {
            Order = new HashSet<Order>();
            UserAgencyTransferreds = new List<UserAgencyTransferred>();
        }

        [Key]
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string otroUsername { get; set; }
        public string UsernameTicket { get; set; }
        public string UsernameTicket2 { get; set; }
        public string UsernameTicket3 { get; set; }
        public string UsernameTicket4 { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }

        public object FirstOrDefault()
        {
            throw new NotImplementedException();
        }

        public string FullName { get { return Name + " " + LastName; } }
        public string ImagenProfile { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string firmaname { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string SecureCode { get; set; }
        public DateTime? ExpiresSecureCode { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string WharehouseLocation { get; set; }
        public Guid AgencyId { get; set; }
        public Guid? AgencyRefId { get; set; } //Se utiliza para los administradores de agencias mayoristas para que puedan visitar las agencias minoristas
        public DateTime Timestamp { get; set; }
        public virtual ICollection<AccessListUser> AccessListUsers { get; set; }
        public ICollection<Order> Order { get; set; }
        public bool viewAdministracion { get; set; } //Para restringir el acceso al panel administración
        public bool viewcuentas { get; set; } //Para restringir el acceso al panel cuentas
        public bool viewindexadmin { get; set; } //Para restringir el acceso al panel index
        public List<UserWholesaler> UserWholesalers { get; set; } //Relacion muchos a muchos con wholesaler
        public List<Task_> Tasks { get; set; }

        /// <summary>
        /// Agencias donde el empleado puede ser visible
        /// </summary>
        public List<UserAgencyTransferred> UserAgencyTransferreds { get; set; }
    }
}
