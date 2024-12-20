using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models.Security
{
    public class AuthenticateResponse
    {
        public string Token { get; set; }
        public DateTime ExpirationUtc { get; set; }
        public string Name { get; set; }
    }
}
