using System;
using System.Collections.Generic;
using System.Linq;

namespace Agenciapp.Service.HttpServices.Models
{
    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }
    }
}
