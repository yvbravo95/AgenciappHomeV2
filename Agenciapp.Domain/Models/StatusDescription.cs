using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class StatusDescription
    {

        public Guid Id { get; set; }

        public string Staus { get; set; }

        public Guid? AgencyId { get; set; }

        public string Description { get; set; }

    }
}
