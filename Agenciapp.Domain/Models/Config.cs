using System;

namespace AgenciappHome.Models
{
    public partial class Config
    {
        public Config()
        {
            
        }

        public Guid Id { get; set; }
        public string Email_Server { get; set; }
        public int Email_Port { get; set; }
        public string Email_User { get; set; }
        public string Email_Pass { get; set; }
        
    }
}
