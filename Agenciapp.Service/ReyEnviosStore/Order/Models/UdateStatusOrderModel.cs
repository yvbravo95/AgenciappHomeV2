using Agenciapp.Domain.Enums;

namespace Agenciapp.Service.ReyEnviosStore.Order.Models
{
    public class UdateStatusOrderModel
    {
        public string OrderNumber { get; set; }
        //public OrderType Type { get; set; }
        public OrderStatus Status { get; set; }
    }

    public enum OrderStatus{
        Pendiente,
        Iniciada,
        Cancelada
    }
}