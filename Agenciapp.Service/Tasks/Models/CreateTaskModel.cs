using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Service.Tasks.Models
{
    public class CreateTaskModel
    {
        [Required]
        [DisplayName("Cliente")]
        public Guid ClientId { get; set; }

        [Required]
        [DisplayName("Empleado")]
        public Guid EmployeeId { get; set; }

        [DisplayName("Nota")]
        public string Nota { get; set; }

        [Required]
        [DisplayName("Fecha de vencimiento")]
        public DateTime DueDate { get; set; }

        [Required]
        [DisplayName("Prioridad")]
        public string Priority { get; set; }

        [Required]
        [DisplayName("Asunto")]
        public string SubjectId { get; set; }
    }
}
