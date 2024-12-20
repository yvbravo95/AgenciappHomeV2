using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agenciapp.Domain.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace AgenciappHome.Models
{
    public class GuiaAerea
    {
        public GuiaAerea()
        {
        }
        public Guid GuiaAereaId { get; set; }
        public Agency Agency { get; set; }
        public DateTime FechaVuelo { get; set; } //Usado para el reporte HAWB
        public string NoVuelo { get; set; }
        public string NoGuia { get; set; }

        #region Datos para el contenedor maritimo
        public string SMLU { get; set; }
        public string SEAL { get; set; }
        public string CAT { get; set; }
        public string Manifiesto { get; set; }
        public DateTime? FechaRecogida { get; set; }
        public DateTime? FechaViaje { get; set; }
        #endregion

        public int Bultos { get; set; }
        /// <summary>
        /// Para saber con el es siguiente numero de paquete
        /// </summary>
        public int CountPackage { get; set; }
        public decimal PesoKg { get; set; }
        public decimal PriceAv1 { get; set; }
        public decimal PriceAv2 { get; set; }
        public decimal PriceAv3 { get; set; }
        public decimal PriceAv4 { get; set; }
        public decimal CostAv1 { get; set; }
        public decimal CostAv2 { get; set; }
        public decimal CostAv3 { get; set; }
        public decimal CostAv4 { get; set; }
        public List<Paquete> Paquetes { get; set; }
        public string Status { get; set; }
        public string Agente { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }

        /// <summary>
        /// Para saber si es una guia de tipo aerea o maritima
        /// </summary>
        public string GuideType { get; set; }
        public bool EnableHandlingAndTransportation { get; set; }
        public List<AccessGuiaAereaAgency> AccessGuiaAereaAgencies { get; set; }  
    }
}
