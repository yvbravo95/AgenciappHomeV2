using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Models.ValueObject
{
    [Owned]
    public class ClientDocument
    {
        public string Type { get; set; }
        public string Number { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
