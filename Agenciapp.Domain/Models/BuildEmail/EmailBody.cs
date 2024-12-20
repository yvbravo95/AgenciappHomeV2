using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models.BuildEmail
{
    public class EmailBody
    {
        public EmailBody()
        {
            EmailAttacheds = new List<EmailAttached>();
            Number = $"EB{DateTime.Now.ToString("yyyyMMddHHmmss")}";
        }
        [Key]public Guid Id { get; set; }
        public string Number { get; private set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public EmailTemplate EmailTemplate { get; set; }
        public List<EmailAttached> EmailAttacheds { get; set; }
    }
}
