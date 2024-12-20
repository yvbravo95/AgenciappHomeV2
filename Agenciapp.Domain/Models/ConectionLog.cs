using System;
using AgenciappHome.Models;

namespace Agenciapp.Domain.Models
{
    public class ConnectionLog
    {
        public Guid Id { get; set; }    
        public DateTime CreatedAt { get; set; }
        public string IpdAddress { get; set; }
        public User User { get; set; }
    }
}