using System;
using System.ComponentModel.DataAnnotations;
using AgenciappHome.Models;

namespace Agenciapp.Domain.Models
{
    public class Note
    {
        protected Note()
        {
            
        }
        public Note(User user, Client client, string note)
        {
            CreatedByUser = user;
            Value = note;
            Client = client;
            CreatedAt = DateTime.UtcNow;
        }
        [Key] public Guid id {get; set;}
        public DateTime CreatedAt { get; private set; }
        public string Value { get; private set; }
        public User CreatedByUser { get; private set; }
        public Client Client { get; private set; }
    }
}