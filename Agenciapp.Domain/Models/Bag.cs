using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{
    public partial class Bag
    {
        public Bag()
        {
        }

        public Guid BagId { get; set; }

        /// <summary>
        /// Codigo de Bolsa
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Indica si la bolsa esta completa
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Nota para cuando se revisa la bolsa
        /// </summary>
        public string CheckedNote { get; set; }

        public int PhysicalBags { get; set; }
        public Guid? OrderId { get; set; }
        [ForeignKey("OrderId")]public Order Order { get; set; }
        public ICollection<BagItem> BagItems { get; set; }


    }
}
