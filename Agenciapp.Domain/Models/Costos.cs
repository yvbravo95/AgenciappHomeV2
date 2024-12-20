using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    /**
     * clase para almacenar los costos de cada trámite para una agencia
     * Este objeto se crea al momento de crear una agencia
     * @by Yunier
     **/
    public class CostosxModulo // Para que las agencias definan sus precios
    {
        public static String ModuloReservasAuto = "Auto";
        public static String ModuloReservasPasaje = "Pasaje";
        public static String ModuloReservasHotel = "Hotel/Vacaciones";
        public static String ModuloEnviosAereos = "Aereo";
        public static String ModuloEnviosMaritimos = "Maritimo";
        public static String ModuloRemesas = "Remesa";
        public static String ModuloCombos = "Combos";
        public static String ModuloCubiq = "Cubiq";
        public static String ModuloTienda = "Tienda";

        public Guid CostosxModuloId { get; set; }
        public Guid AgencyId { get; set; }
        public virtual ICollection<ValoresxTramite> valoresTramites { get; set; }
    }

    /**
     * Para cuando un mayorista establesca a una agencia como minorista
     * */
    public class CostoxModuloMayorista: CostosxModulo
    {
        public bool Visibility { get; set; }
        public Guid AgencyMayoristaId { get; set; }
        public virtual ICollection<ModuloAsignado> modAsignados { get; set; } // Se añade un elemento cuando el minorista establece a una agencia como mayorista
    }

    public class ValoresxTramite
    {
        public Guid ValoresxTramiteId { get; set; }
        public string Tramite { get; set; }
        public CostosxModulo CostosxModulo { get; set; }
        public virtual ICollection<ValorProvincia> valores { get; set; }
    }

    public class ValorProvincia
    {
        public Guid ValorProvinciaId { get; set; }
        public string provincia { get; set; }
        public decimal valor { get; set; }
        public decimal valor2 { get; set; } //Para el caso de las remesas va a definirse cuando el valor a pagar el mayor de 99
        public decimal valor3 { get; set; } //Para el caso de las remesas (USD)
        public decimal valor4 { get; set; } //Para el caso de las remesas (USD) va a definirse cuando el valor a pagar el mayor de 99
        public decimal valor5 { get; set; } //Para el caso de las remesas (USD_TARJETA)
        public decimal valor6 { get; set; } //Para el caso de las remesas (USD_TARJETA) va a definirse cuando el valor a pagar el mayor de 99
        public ValoresxTramite listValores { get; set; }
    }

    /**
    * Esta clase ha sido realizada con el objetivo de relacionar los modulos asignados
    * de una agencia Estandar a una Mayorista. La agencia mayorista es la encargada de 
    * procesar los tramites, mientras la estandar es la encargada de captarlos 
    * @by Yunier
    **/
    public class ModuloAsignado
    {
        /*public const String ModuloReservasAuto = "Auto";
        public const String ModuloReservasPasaje = "Pasaje";
        public const String ModuloReservasHotel = "Hotel/Vacaciones";
        public const String ModuloEnviosAereos = "Paquete Aereo";
        public const String ModuloEnviosMaritimos = "Maritimo";
        public const String ModuloRemesas = "Remesa";
        public const String ModuloRecargas = "Recarga";
        public const String ModuloCombos = "Combos";
        public const String ModuloTienda = "Tienda";
        public const String ModuloCubiq = "Cubiq";
        public const String ModuloCaribe = "Maritimo-Aereo";*/

        public Guid ModuloAsignadoId { get; set; }
        public Guid IdWholesaler { get; set; }// Cuando el minorista cree la relacion se añade este identificador
        public string Tramite { get; set; }
        public bool Hidden { get; set; }
        public CostoxModuloMayorista costoxModulo { get; set; }
    }
}
