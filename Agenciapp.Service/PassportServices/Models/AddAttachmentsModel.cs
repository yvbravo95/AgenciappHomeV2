using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Service.PassportServices.Models
{
    public class AddAttachmentsModel
    {
        [Required]public Guid PassportId { get; set; }
        public string Description { get; set; }
        public IFormFileCollection Files { get; set; }
    }
}
