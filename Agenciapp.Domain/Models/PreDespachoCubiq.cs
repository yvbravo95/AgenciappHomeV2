using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class PreDespachoCubiq
    {
        public static string STATUS_INPREOGRESS = "En Proceso";
        public static string STATUS_ALMACENUSA = "Almacen USA";
        public static string STATUS_COMPLETADO = "Completado";

        [Key]
        public Guid Id { get; set; }
        public Guid AgencyId { get; set; }
        public Guid? AgencyTransferidaId { get; set; }
        public string Number { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public User User { get; set; }
        public Agency Agency { get; set; }
        public Agency AgencyTransferida { get; set; }
        public List<Paquete> Paquetes { get; set; }
        public List<PreDespachoVerifiedItemCubiq> Items { get; set; }
    }

    public class PreDespachoVerifiedItemCubiq
    {
        public Guid Id { get; set; }
        public Guid PreDespachoId { get; set; }
        public Guid OrderId { get; set; }
        public Guid PaqueteId { get; set; }
        public string OrderNumber { get; set; }
        public string PaqueteNumber { get; set; }
        public PreDespachoCubiq PreDespacho { get; set; }
        public Paquete Paquete { get; set; }
        public OrderCubiq OrderCubiq { get; set; }
    }
}
