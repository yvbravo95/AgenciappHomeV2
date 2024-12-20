using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Models.ValueObject
{
    public class EmailAddress
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}
