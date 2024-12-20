using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public abstract class ImageClient
    {
        [Key]
        public Guid ID { get; set; }
        public byte[] value { get; set; }
        public String type { get; set; }
        public Guid? ClientId { get; set; }
    }
}
