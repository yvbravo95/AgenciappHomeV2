using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class AgencyContact
    {
        public Guid AgencyContactId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid ContactId { get; set; }
    }
}
