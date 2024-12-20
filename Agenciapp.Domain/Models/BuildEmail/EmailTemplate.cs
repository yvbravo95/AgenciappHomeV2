using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models.BuildEmail
{
    public class EmailTemplate
    {
        public EmailTemplate()
        {
            EmailBodies = new List<EmailBody>();
        }
        [Key]public Guid Id { get; set; }
        public Guid AgencyId { get; set; }
        public string Name { get; set; }
        public string Template { get; set; }
        public Agency Agency { get; set; }
        public List<EmailBody> EmailBodies { get; set; }
    }
}
