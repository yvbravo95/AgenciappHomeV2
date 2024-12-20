using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.IMarketing.Models
{
    public class ResponseTwilio
    {
        public string fromNumber { get; set; }
        public string toNumber { get; set; }
        public string Sms { get; set; }
        public string Sid { get; set; }
        public string Status { get; set; }
        public string ErrorCode { get; set; }

    }
}
