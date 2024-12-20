using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class TramiteEmpleado
    {
        public static String tipo_ENVIO = "ENVIO";
        public static String tipo_REMESA = "REMESA";
        public static String tipo_RECARGA = "RECARGA";
        public static String tipo_RESERVA = "RESERVA";
        public static String tipo_ENVIOMARITIMO = "ENVIO MARITIMO";
        public static String tipo_OTROSSERVICIOS = "OTROS SERVICIOS";
        public static String tipo_CUBIQ = "Envio Cubiq";
        public static String tipo_ENVIOCARIBE = "ENVIO CARIBE";
        public static String tipo_PASSPORT = "PASSPORT";


        public Guid Id { get; set; }
        public Guid IdTramite { get; set; }
        public Guid IdEmpleado { get; set; }
        public Guid IdAgency { get; set; }
        public string tipoTramite { get; set; }
        public DateTime fecha { get; set; }
    }
}
