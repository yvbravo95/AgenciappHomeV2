using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.IBuildEmailServices.Models
{
    public class CreateEmailBodyModel
    {
        public string Name { get; set; }
        public string Body { get; set; }
        public Guid IdEmailTemplate { get; set; }
        public IFormFileCollection Files { get; set; }
    }
}
