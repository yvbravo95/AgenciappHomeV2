using System;

namespace Agenciapp.Common.Models.Dto
{
    public class BagDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string OrderNumber { get; set; }
        public bool IsComplete { get; set; }
        public string CheckedNote { get; set; }
    }
}