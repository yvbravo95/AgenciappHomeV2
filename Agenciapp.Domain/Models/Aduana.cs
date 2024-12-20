using Agenciapp.Domain.Models;
using Agenciapp.Domain.Models.ValueObject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AgenciappHome.Models
{
    public partial class Aduana
    {
        public Aduana()
        {
        }
        public Guid Id { get; set; }
        public Guid? AgencyId { get; set; }
        public Agency Agency { get; set; }
        public bool Enable { get; set; }
        public string Articulo { get; set; }
        public string UM { get; set; }
        public string Cantidad { get; set; }
        public string Valor { get; set; }
        public string Observaciones { get; set; }
        public bool EnvioAduana { get; set; }


        public string SelectName { get
            {
                string articuloRecortado = Articulo.Length <= 50 ? Articulo : Articulo.Substring(0, 50) + "...";
                return $"{articuloRecortado} - {UM} - {Cantidad} - ${Valor}";
            }
        }
    }
}
