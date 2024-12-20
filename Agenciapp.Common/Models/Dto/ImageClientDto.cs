using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models.Dto
{
    public class ImageClientDto
    {
        public Guid Id { get; set; }
        public byte[] Value { get; set; }
        public string Type { get; set; }
        public Guid? ClientId { get; set; }
    }
}
