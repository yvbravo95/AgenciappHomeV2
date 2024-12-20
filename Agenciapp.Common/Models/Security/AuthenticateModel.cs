using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models.Security
{
    public class AuthenticateModel
    {
        public AuthenticateModel(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
        public string UserName { get; private set; }
        public string Password { get; private set; }
    }
}
