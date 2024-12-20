using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Service.IAirShippingServices.Models
{
    public class AddAttachmentsModel
    {
        [Required] public Guid OrderId { get; set; }
        public string Description { get; set; }
        public IFormFileCollection Files { get; set; }
    }
}
