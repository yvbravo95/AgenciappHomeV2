using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Agenciapp.Domain.Models
{
    /**
     * Para gestionar los costos de envio por tipo de trámite
     */
    public class HMpaquetesPriceByProvince
    {
        public Guid Id { get; set; }
        public Guid AgencyId { get; set; }
        public Guid? RetailAgencyId { get; set; }
        public Guid? RetailId { get; set; }
        public Guid ProvinceId { get; set; }
        public Guid MunicipalityId { get; set; }
        public decimal Price { get; set; }
        public STipo Type { get; set; }
        public Agency Agency { get; set; }
        public Agency RetailAgency { get; set; }
        public Minorista Retail { get; set; }
        public Provincia Province { get; set; }
        public Municipio Municipality { get; set; }
    }
}
