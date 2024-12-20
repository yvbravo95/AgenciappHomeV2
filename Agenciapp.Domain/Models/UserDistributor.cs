using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class UserDistributor
    {
        public UserDistributor()
        {

        }

        public UserDistributor(User employee, User distributor)
        {
            Employee = employee;
            Distributor = distributor;
        }
        [Key]public Guid Id { get; set; }
        public User Employee { get; set; }
        public User Distributor { get; set; }
    }
}
