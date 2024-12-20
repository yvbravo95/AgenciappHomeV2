
using Agenciapp.Domain.Models;
using Agenciapp.Domain.Models.BuildEmail;
using Agenciapp.Domain.Models.ValueObject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace AgenciappHome.Models
{
    public partial class Client
    {
        public Client()
        {
            Order = new HashSet<Order>();
            ImageClients = new List<ImageClient>();
            RegistroPagos = new HashSet<RegistroPago>();
            Notes = new List<Note>();
            Passport = new ClientPassport();
            OtherDocument = new ClientDocument();
            MarketingStatus = "Active";
        }
        public Guid ClientId { get; set; }
        public Guid AgencyId { get; set; }
        public string ClientNumber { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string LastName { get; set; }
        public string LastName2 { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Email { get; set; }
        public string ID { get; set; }
        public string phoneCuba { get; set; }
        public string PhoneNumber { get; set; }
        public Phone Phone { get; set; }
        public Address Address { get; set; }
        public Agency Agency { get; set; }
        public ClientPassport Passport { get; set; }
        public ClientDocument OtherDocument { get; set; }
        public bool checkNotifications { get; set; }
        public bool createByTicketCode { get; set; }
        [DefaultValue("Active")]
        public string MarketingStatus  { get; set; }
        public ICollection<Order> Order { get; set; }
        public List<ImageClient> ImageClients { get; set; }
        public ICollection<RegistroPago> RegistroPagos { get; set; }
        public ICollection<Credito> Creditos { get; set; }
        public DateTime FechaNac { get; set; }
        public string Nota { get; set; }
        public List<Task_> Tasks { get; set; }
        public List<RegistrationSendEmail> RegistrationSendEmail { get; set; }
        public List<Note> Notes { get; set; }
        public bool Conflictivo { get; set; }
        public bool IsCarrier { get; set; }

        public decimal GetCredito { get { return Creditos != null ? Creditos.Sum(x => x.value) : 0; } }
        public string FullData
        {
            get
            {
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);
                string fullData = $"{Name} {Name2} {LastName} {LastName2}".Trim();
                return regex.Replace(fullData, " ");
            }
        }
    }
}
