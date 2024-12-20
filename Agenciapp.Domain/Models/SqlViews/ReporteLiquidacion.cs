using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agenciapp.Domain.Models.SqlViews
{
    [NotMapped]
    public class ReporteLiquidacion
    {
        [Key] public Guid RegistroPagoId { get; set; }
        public decimal valorPagado { get; set; }
        public DateTime Date { get; set; }
        public Guid? FacturaId { get; set; }
        public Guid UserId { get; set; }
        public Guid? CuentaBancariaId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid? BillId { get; set; }
        public Guid? RechargueId { get; set; }
        public Guid? ServicioId { get; set; }
        public Guid? OrderCubiqId { get; set; }
        public Guid? PassportId { get; set; }
        public Guid? EnvioCaribeId { get; set; }
        public Guid? ClientId { get; set; }
        public Guid? EnvioMaritimoId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? TicketId { get; set; }
        public Guid? RemittanceId { get; set; }
        public Guid? MercadoId { get; set; }
        public Guid? PaqueteTuristicoId { get; set; }
        public Guid tipoPagoId { get; set; }
        public string Type { get; set; }
        public string User_Name { get; set; }
        public string User_LastName { get; set; }
        public decimal? Remesa_Amount { get; set; }
        public decimal? Remesa_Pagado { get; set; }
        public string Remesa_Number { get; set; }
        public string Remesa_Status { get; set; }
        public DateTime? Remesa_Date { get; set; }
        public string Order_Type { get; set; }
        public decimal? Order_Amount { get; set; }
        public decimal? Order_Pagado { get; set; }
        public decimal? Order_CantLb { get; set; }
        public decimal? Order_CantLbMedicina { get; set; }
        public string Order_Number { get; set; }
        public string Order_Status { get; set; }
        public Guid? Order_MinoristaId { get; set; }
        public DateTime? Order_Date { get; set; }
        public string Cubiq_Number { get; set; }
        public string Cubiq_Status { get; set; }
        public decimal? Cubiq_Amount { get; set; }
        public decimal? Cubiq_Pagado { get; set; }
        public DateTime? Cubiq_Date { get; set; }
        public decimal? Recarga_Import { get; set; }
        public decimal? Recarga_Pagado { get; set; }
        public string Recarga_Number { get; set; }
        public string Recarga_Status { get; set; }
        public DateTime? Recarga_Date { get; set; }
        public string Maritimo_Number { get; set; }
        public string Maritimo_Status { get; set; }
        public decimal? Maritimo_Amount { get; set; }
        public decimal? Maritimo_Pagado { get; set; }
        public DateTime? Maritimo_Date { get; set; }
        public string Caribe_Number { get; set; }
        public string Caribe_Status { get; set; }
        public decimal? Caribe_Amount { get; set; }
        public decimal? Caribe_Pagado { get; set; }
        public DateTime? Caribe_Date { get; set; }
        public string Passport_Number { get; set; }
        public decimal? Passport_Amount { get; set; }
        public decimal? Passport_Pagado { get; set; }
        public string Passport_Status { get; set; }
        public DateTime? Passport_Date { get; set; }
        public string Servicio_Number { get; set; }
        public string Servicio_Status { get; set; }
        public decimal? Servicio_Amount { get; set; }
        public decimal? Servicio_Pagado { get; set; }
        public DateTime? Servicio_Date { get; set; }
        public Guid? Servicio_PaqueteTuristicoId { get; set; }
        public Guid? TipoServicioId { get; set; }
        public string TipoServicio_Nombre { get; set; }
        public string Ticket_Number { get; set; }
        public string Ticket_Status { get; set; }
        public decimal? Ticket_Amount { get; set; }
        public decimal? Ticket_Pagado { get; set; }
        public DateTime? Ticket_Date { get; set; }
        public bool? ClientIsCarrier { get; set; }
        public Guid? Ticket_PaqueteTuristicoId { get; set; }
        public string PaqueteTuristico_Number { get; set; }
        public decimal? PaqueteTuristico_Amount { get; set; }
        public string PaqueteTuristico_Status { get; set; }
        public DateTime? PaqueteTuristico_Date { get; set; }
        public decimal? PaqueteTuristico_Pagado { get; set; }
        public string Mercado_Number { get; set; }
        public decimal? Mercado_Amount { get; set; }
        public string Mercado_Status { get; set; }
        public DateTime? Mercado_Date { get; set; }
        public decimal? Mercado_Pagado { get; set; }
    }
}