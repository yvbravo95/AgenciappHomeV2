using System;
using System.Collections.Generic;

namespace AgenciappHome.Models.Response
{
    public class ListServicioModel{
        public ServicioTableName Type { get; set; }
        public List<ItemServicioModel> Items { get; set; }
    }

    public enum ServicioTableName{
        Pendiente,
        Consulado,
        Completada,
        Despachada,
        Recibida,
        Enviado,
        Entregada,
        Cancelado
    }
    public class ItemServicioModel
    {
        public Guid ServicioId { get; set; }
        public DateTime Date { get; set; }
        public string Number { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string TipoServicio { get; set; }
        public string WholesalerName { get; set; }
        public string Status { get; set; }
        public string NoDespacho { get; set; }
        public decimal Balance { get; set; }
        public string Minorista { get; set; }
        public Guid? PaqueteId { get; internal set; }
    }
}