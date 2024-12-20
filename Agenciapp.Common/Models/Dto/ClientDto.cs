using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Agenciapp.Common.Models.Dto
{
    public class ClientDto
    {
        public Guid Id { get; set; }
        public Guid AgencyId { get; set; }
        public string ClientNumber { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Surname { get; set; }
        public string SecondSurname { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Email { get; set; }
        public string Identification { get; set; }
        public string PhoneCuba { get; set; }
        public PhoneDto Phone { get; set; }
        public AddressDto Address { get; set; }
        public AgencyDto Agency { get; set; }
        public bool CheckNotifications { get; set; }
        public bool CreateByTicketCode { get; set; }
        public List<ImageClientDto> ImageClients { get; set; }
        public DateTime BirthDate { get; set; }
        public string Nota { get; set; }
        public bool Conflictivo { get; set; }
        public bool IsCarrier { get; set; }

        public string FullData
        {
            get
            {
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);
                string fullData = $"{FirstName} {SecondName} {Surname} {SecondSurname}".Trim();
                return regex.Replace(fullData, " ");
            }
        }
    }
}
