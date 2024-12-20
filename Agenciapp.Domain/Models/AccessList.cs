using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class AccessList
    {
        [Key]
        public Guid AccessListId { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public virtual ICollection<AccessListUser> AccessListUsers { get; set; }

    }

    public class AccessListUser { 
    
        [Key]
        public Guid AccessListUserId { get; set; }
        public Guid AccessListId { get; set; }
        public Guid UserId { get; set; }
        public AccessList accessList { get; set; }
        public User user { get; set; }
    }
}
