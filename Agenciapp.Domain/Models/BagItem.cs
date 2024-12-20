using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class BagItem
    {        
        public Guid BagItemId { get; set; }

        public Guid BagId { get; set; }
        public Bag Bag { get; set; }

        public Guid ProductId { get; set; }

        public Product Product { get; set; }

        public int Qty { get; set; }

        public int Order { get; set; }

        /// <summary>
        /// Indica si la bolsa fue recibida
        /// </summary>
        public bool IsRecived { get { return Qty == QtyReceived; } }

        /// <summary>
        /// Cantidad que ha sido recibida
        /// </summary>
        public int QtyReceived { get; set; }
    }
}
