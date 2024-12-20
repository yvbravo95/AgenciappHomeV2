using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class UserOffice
    {
        public Guid UserOfficeId { get; set; }
        public Guid UserId { get; set; }
        public Guid OfficeId { get; set; }
    }
}
