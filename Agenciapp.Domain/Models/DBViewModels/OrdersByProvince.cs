using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models.DBViewModels
{
    public class OrdersByProvince
    {
        [Key] public Guid OrderId { get; set; }
        public Guid PrincipalDistributorId { get; set; }
        public string Estado { get; set; }
        public DateTime Date { get; set; }
        public string City { get; set; }
    }
}
