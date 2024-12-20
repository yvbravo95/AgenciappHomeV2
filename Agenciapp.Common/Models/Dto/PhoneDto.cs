using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models.Dto
{
    public class PhoneDto
    {
        public Guid Id { get; set; }
        public Guid ReferenceId { get; set; }
        public string Type { get; set; }
        public bool Current { get; set; }
        public string Number { get; set; }
        public string Carrier { get; set; }
        public string Sms_email { get; set; }
    }
}
