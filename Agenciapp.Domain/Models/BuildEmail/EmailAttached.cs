using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models.BuildEmail
{
    public class EmailAttached
    {
        [Key]public Guid Id { get; set; }
        public string Name { get; set; }
        public string OriginalName { get; set; }
        public EmailBody EmailBody { get; set; }
    }
}
