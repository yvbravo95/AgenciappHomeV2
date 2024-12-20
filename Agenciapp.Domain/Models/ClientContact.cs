using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{
    public partial class ClientContact
    {
        public Guid CCId { get; set; }
        public Guid ClientId { get; set; }
        public Guid ContactId { get; set; }
        public Client Client { get; set; }
        public Contact Contact { get; set; }
    }
}
