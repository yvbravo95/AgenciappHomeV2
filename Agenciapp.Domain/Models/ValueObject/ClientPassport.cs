using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Models.ValueObject
{
    [Owned]
    public class ClientPassport
    {
        public string Number { get; set; }
        public DateTime ExpireDate { get; set; }
        public DateTime DateNotify { get; set; }
    }
}
