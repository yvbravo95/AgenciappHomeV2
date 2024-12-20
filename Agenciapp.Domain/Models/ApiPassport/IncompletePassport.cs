using AgenciappHome.Models.ApiModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Agenciapp.Domain.Models.ApiPassport
{
    public class IncompletePassport
    {
        public IncompletePassport()
        {
            CreateAt = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreateAt { get; set; }
        public int ServicioConsular { get; set; }
        public string Data { get; set; }

        [ForeignKey("UserId")]
        public UserClient User { get; set; }
    }
}
