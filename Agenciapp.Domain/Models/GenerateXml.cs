using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class GenerateXml
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string RealName { get; set; }
        public STipo Tipo { get; set; }
        public int Oreder { get;  set; }
    }

    public class GenerateXmlAux
    {
        [Key]
        public Guid Id { get; set; }
        public string Categoria { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public STipo Tipo { get; set; }
        public string Provcode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public  string Name { get; set; }
    }
}
