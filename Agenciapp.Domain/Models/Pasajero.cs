using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class Pasajero
    {
        [Key]
        public Guid PasajeroId { get; set; }
        [Description("Nombre de Pasajero")]
        public string Name { get; set; }
        [Description("Apellido de Pasajero")]
        public string LastName { get; set; }
        [Description("Teléfono de Pasajero")]
        public string Phone { get; set; }
        [Description("Fecha de Nacimiento")]
        public DateTime FechaNac { get; set; }
        [Description("Número de Pasaporte")]
        public string PasspotNumber { get; set; }
        public DateTime PasspotExpDate { get; set; }

        public string OtroDocumento { get; set; }
        public string OtroDocumentoNo { get; set; }
        public DateTime OtroDocumentoExpDate { get; set; }

        public string PaisOtroDocumento { get; set; }

        public DocumentPassenger UsaArrivalDoc { get; set; }
        public DocumentPassenger ForArrivalDoc { get; set; }

        //App movil
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public string Nationality { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }


        public bool IsMenor { get; set; }
        public string AuthorizesName { get; set; }
        public string AuthorizesRelation { get; set; }
        public string AuthorizedName { get; set; }
        public string AuthorizedRelation { get; set; }
        public string NotaryName { get; set; }
        public string PersonallyKnown { get; set; }
        public string ProdIdentification { get; set; }
        public string TypeIdentification { get; set; }
        public string IdentificationNo { get; set; }

        public Ticket Ticket { get; set; }
    }

    public class DocumentPassenger
    {
        [Key] public Guid Id { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public DateTime ExpDate { get; set; }
        public string Country { get; set; }
    }
}
