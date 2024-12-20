using AgenciappHome.Models.ModelsEnvioCaribe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.ModelsApiReservas
{
    public class ReservaApi
    {
        public AuxECClient Client { get; set; }
        public string Category { get; set; } // idavuelta ó ida, para cuando es un pasaje
        public string TicketBy { get; set; } // número de reserva
        public decimal Charges { get; set; } // costo por servicio
        public decimal Price { get; set; } // Precio del trámite
        public decimal Costo { get; set; } // Costo
        public decimal Discount { get; set; } // Descuento
        public string TypePayment { get; set; } // //Cash, Zelle, Cheque, Crédito o Débito, Transferencia Bancaria, Web, Money Order
        public decimal Total { get; set; } // Total a pagar
        public decimal Payment { get; set; } // Pagado
        public decimal Debit { get; set; } // Balance
        public string Description { get; set; } // Nota
        public string type { get; set; } //pasaje, auto, hotel
        public string Flight { get; set; } // Número de Vuelo de Salida (si es pasaje idavuelta)
        public DateTime DateOut { get; set; } // Fecha de salida (Pasaje si es idavuelta), Fecha de recogida (Auto), Fecha de Salida (Hotel)
        public DateTime HoraSalida { get; set; } // Hora de Salida (si es pasaje idavuelta)
        public string NoVueloRegreso { get; set; } // 
    }
}
