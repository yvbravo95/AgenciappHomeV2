using System;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public class Minorista
    {
        protected Minorista()
        {
            
        }
        public Minorista(string name, string email, string phone, string identifier, Agency agency, STipo type)
        {
            Name = name;
            Email = email;
            Agency = agency;
            Type = type;
            Phone = phone;
            Identifier = identifier;
        }

        [Key] public Guid Id { get; set; }
        [Required] public string Name { get; private set; }
        [Required] public string Email { get; private set; }
        public string Phone { get; private set; }
        public string Identifier { get; private set; }
        [Required] public Agency Agency { get; private set; }
        [Required] public STipo Type { get; private set; }
    }
}
