using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public partial class AuxReserva
    {
        [Key]
        public Guid Id { get; set; }
        public CategoriaAuto Categoria { get; set; }
        public TransmisionAuto Transmision { get; set; }
        public TemporadaAuto Temporada { get; set; }

        public string Modelo { get; set; }
        public Rentadora Rentadora { get; set; }

        public decimal Precio1 { get; set; }
        public decimal Precio2 { get; set; }
        public decimal Precio3 { get; set; }
        public decimal Precio4 { get; set; }
    }
    public enum CategoriaAuto
    {
        [Description("")]
        None,
        [Description("Economico")]
        Eco,
        [Description("Estandar")]
        Estandar,
        [Description("Medio")]
        MedioAut,
        [Description("Premium")]
        Premiun,
        [Description("Premium Plus")]
        PremiunPlus,
        Jeep,
        Van,
        Camper,
        Motos,
        [Description("Jeep Alto Estandar")]
        JeepAE,
        SUV
    }
    public enum TransmisionAuto
    {
        [Description("")]
        None,
        Manual,
        Automatica
    }
    public enum TemporadaAuto
    {
        [Description("")]
        None,        
        [Description("Alta")]
        Alta,
        [Description("Media Alta")]
        MediaAlta,
        [Description("Media ALta II")]
        MediaAltaII,
        [Description("Extrema Alta")]
        ExtremaAlta
    }
}
