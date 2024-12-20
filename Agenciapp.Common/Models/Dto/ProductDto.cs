using System;

namespace Agenciapp.Common.Models.Dto
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public Guid AgencyId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Tipo { get; set; }
        public string Color { get; set; }
        public string TallaMarca { get; set; }
        public bool esDespachado { get; set; }
    }
}