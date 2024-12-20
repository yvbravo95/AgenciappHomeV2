using Agenciapp.Common.Headers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Services
{
    public interface IWorkContext
    {
        bool UserIsAuthenticated { get; set; }
        string Token { get; set; }
        string TokenExpirationUtc { get; set; }
    }

    public class WorkContext: IWorkContext
    {
        public bool UserIsAuthenticated { get; set; }
        public string Token { get; set; }
        public string TokenExpirationUtc { get; set; }
    }
}
