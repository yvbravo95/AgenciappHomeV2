using System;
using AgenciappHome.Models;

namespace Agenciapp.Domain.Models
{
    public class CostoTraslado
    {
        public Guid Id { get; set; }
        public Agency Agency { get; set; }
        public string Type { get; set; }
        public decimal Cost { get; set; }
    }
}