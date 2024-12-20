using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.IMerchantElavonServices.Models
{
    public class AuthenticateResponse
    {
        public string token { get; set; }
        public int expiresInSec { get; set; }
        public string status { get; set; }
        public List<Failure> failures { get; set; }
    }

    public class Failure
    {
        public string code { get; set; }
        public string description { get; set; }
    }
}
