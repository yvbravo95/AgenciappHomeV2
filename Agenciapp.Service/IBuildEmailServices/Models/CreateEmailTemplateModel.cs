using Agenciapp.Domain.Models.BuildEmail;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Service.IBuildEmailServices.Models
{
    public class CreateEmailTemplateModel
    {
        [Required]
        [DisplayName("Nombre")]
        public string Name { get; set; }
        [Required]
        [DisplayName("PLantilla")]
        public string Template { get; set; }
    }
}
