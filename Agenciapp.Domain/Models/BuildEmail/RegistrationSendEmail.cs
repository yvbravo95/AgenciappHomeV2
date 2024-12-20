using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models.BuildEmail
{
    public class RegistrationSendEmail
    {
        protected RegistrationSendEmail()
        {

        }
        public RegistrationSendEmail(Client client, EmailBody emailBody)
        {
            Client = client;
            EmailBody = emailBody;
            CreatedAt = DateTime.UtcNow;
            Description = "";
        }
        public RegistrationSendEmail(Client client, EmailBody emailBody, string description)
        {
            Client = client;
            EmailBody = emailBody;
            CreatedAt = DateTime.UtcNow;
            Description = description;
        }

        [Key] public Guid Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public Client Client { get; private set; }
        public EmailBody EmailBody { get; private set; }
        public string Description { get; set; }
    }
}
