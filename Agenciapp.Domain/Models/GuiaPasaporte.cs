using Agenciapp.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class GuiaPasaporte
    {
        public static string Status_Iniciada = "Iniciada";
        public static string Status_Consulado = "Consulado";
        public static string Status_Recibida = "Recibida";

        [Key]
        public Guid GuiaId { get; set; }
        public Agency Agency { get; set; }

        public User User { get; set; }

        public DateTime Date { get; set; } //Usado para el reporte HAWB
        public DateTime FechaEnvio { get; set; } 
        public string NoGuia { get; set; }
        public int CantidadPasaportes { get { return ManifiestosPasaporte.Sum(x => x.Passports.Count); } }
        public List<ManifiestoPasaporte> ManifiestosPasaporte { get; set; }
        public string Status { get; set; }
        public int codservagencia { get; set; }
    }

    public class ManifiestoPasaporte
    {
        [Key]
        public Guid Id { get; set; }
        public List<Passport> Passports { get; set; }
        public int Numero { get; set; }

        public Guid GuiaPasaporteGuiaId { get; set; }
        public GuiaPasaporte GuiaPasaporte { get; set; }
        public List<Cheque> Cheques { get; set; }
    }
}
