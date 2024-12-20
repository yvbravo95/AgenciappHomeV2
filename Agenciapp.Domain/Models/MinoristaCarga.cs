using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class MinoristaCarga
    {
        public Guid Id { get; set; }
        public Guid MinoristaId { get; set; }
        public Guid MayoristaId { get; set; }
    }
}
