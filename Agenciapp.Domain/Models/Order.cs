using Agenciapp.Domain.Enums;
using Agenciapp.Domain.Models;
using AgenciappHome.Models.ApiModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AgenciappHome.Models
{

    public partial class Order : OrderBase
    {
        public const string STATUS_ENPROGESO = "En Progreso";
        public const string STATUS_PROCESADA = "Procesada";
        public const string STATUS_PENDIENTE = "Pendiente";
        public const string STATUS_INICIADA = "Iniciada";
        public const string STATUS_COMPLETADA = "Completada";
        public const string STATUS_ENVIADA = "Enviada";
        public const string STATUS_REVISADA = "Revisada";
        public const string STATUS_ENTREGADA = "Entregada";
        public const string STATUS_DESPACHADA = "Despachada";
        public const string STATUS_CANCELADA = "Cancelada";
        public const string STATUS_NOENTREGADA = "No Entregada";
        public const string STATUS_RECIBIDA = "Recibida";
        public const string STATUS_RECIBIENDO = "Recibiendo";

        public Order()
        {
            ValorAduanalItem = new HashSet<ValorAduanalItem>();
            Bag = new HashSet<Bag>();
            RegistroEstados = new List<RegistroEstado>();
            Pagos = new List<RegistroPago>();
            ShippingOrders = new List<ShippingOrder>();
            UpdatedDate = DateTime.Now;
            Attachments = new List<AttachmentOrder>();
        }

        /// <summary>
        /// Actualiza el estado del tramite
        /// </summary>
        /// <param name="status"></param>
        /// <param name="user"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void UpdateStatus(string status, User user)
        {
            string oldStatus = Status;
            switch (status)
            {
                case STATUS_RECIBIDA:
                    UpdateStatusRecived();
                    ReceivedDate = DateTime.Now;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (Status != oldStatus)
            {
                RegistroEstados.Add(new RegistroEstado
                {
                    User = user,
                    Estado = Status,
                });
                UpdatedDate = DateTime.Now;
            }
        }

        private void UpdateStatusRecived()
        {
            if(Status.Equals(STATUS_ENVIADA) || Status.Equals(STATUS_DESPACHADA) || Status.Equals(STATUS_RECIBIENDO))
            {
                Status = STATUS_RECIBIDA;
                if (Bag.Any(x => !x.IsComplete || x.BagItems.Any(y => !y.IsRecived)))
                {
                    Status = STATUS_RECIBIENDO;
                }
            }
        }

        /// <summary>
        /// Get Phone Number Client by user authenticated
        /// </summary>
        /// <param name="agencyId">Agency of user authenticated</param>
        public string GetClientPhone(Guid agencyId)
        { 
            if(agencyTransferida?.AgencyId == agencyId)
            {
                return Agency?.Phone?.Number ?? string.Empty;
            }

            return Client?.Phone?.Number ?? string.Empty;
        }

        public Guid OrderId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid OfficeId { get; set; }
        public Guid ClientId { get; set; }
        public Guid IdAuthorizationCard { get; set; }
        public Guid IdShipping { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime? DistributedDate { get; set; }
        public decimal Amount { get; set; }
        public decimal FeeService { get; set; } //Fee Cobrado por la app de rapid
        public string Status { get; set; }
        public decimal PriceLb { get; set; }
        public decimal PriceLbMedicina { get; set; }
        public decimal CantLb { get; set; }
        public decimal CantLbMedicina { get; set; }
        public bool isUsd { get; set; } //Para las remesas en USD
        public decimal addCosto { get; set; } // Para sumar o restar al costo del producto (Usado en Combos)
        public decimal addPrecio { get; set; } // Para sumar  o restar al precio del producto (Usado en Combos)
        public decimal CustomsTax { get; set; }
        public decimal AditionalCharge { get; set; }
        public Guid TipoPagoId { get; set; }
        public Guid ContactId { get; set; }
        public Guid UserId { get; set; }
        public decimal ValorPagado { get; set; }
        public decimal TotalPagado { get; set; } //Para las remesas
        public decimal Balance { get; set; }
        public decimal OtrosCostos { get; set; }
        public decimal ValorAduanal { get; set; }
        public decimal Delivery { get; set; }
        public StoreType StoreType { get; set; }
        public bool express { get; set; } //Para marcar el envio como express
        public bool Editable { get; set; } = true; // Para saber si el tramite se puede editar
        public string Nota { get; set; }
        public string NoOrden { get; set; } // Para tipo envio tienda
        public string tarifa { get; set; } // Para las remesas (Tarifa fijo o porciento)
        public decimal feeCobro { get; set; } // Para las remesas
        public decimal costoMayorista { get; set; } //Si el mayorista no es por transferencia define un costo
        public decimal costoProductosBodega { get; set; } //Costo para cuando se elijan productos de la bodega
        public DateTime fechaEntrega { get; set; } // Para las remesas
        public OrderRevisada orderRevisada { get; set; } // Para revisar una orden y para entregar remesa
        public OrderRevisada orderEntregada { get; set; } // Para entregar order

        public Office OfficeTransferida { get; set; }
        public Agency agencyTransferida { get; set; } // Para transferir el tramite a un mayorista
        public Agency Agency { get; set; }
        public Client Client { get; set; }
        public Contact Contact { get; set; }
        public Office Office { get; set; }
        public TipoPago TipoPago { get; set; }
        public User User { get; set; }
        public Package Package { get; set; }
        public ICollection<ValorAduanalItem> ValorAduanalItem { get; set; }
        public ICollection<Bag> Bag { get; set; }
        public AuthorizationCard authorizationCard { get; set; }
        public Shipping shipping { get; set; }
        public Wholesaler wholesaler { get; set; }
        public Wholesaler despachadaA { get; set; } //Para cuando una agencia mayorista envia el tramite a otra agencia mayorista
        public decimal costoporDespacho { get; set; } //Cuando es despachada se guarda el costo aplicado por el mayorista
        public int cantidad { get; set; } // Cantidad para tipo servicio Combo
        public DateTime fechadespacho { get; set; }
        public string numerodespacho { get; set; }
        public string NumeroDespachoDistribuidor { get; set; }

        public decimal credito { get; set; }

        public List<RegistroEstado> RegistroEstados { get; set; }
        public List<RegistroPago> Pagos { get; set; }

        public decimal costoDeProveedor { get; set; }

        public bool iswebapi { get; set; } //Para saber si la orden fué creada desde la api
        public string order_key { get; set; } //Id de ordenes de la api

        public string NoTarjeta { get; set; } //Para remesas
        public string NotaTarjeta { get; set; } //Para remesas

        public decimal pagoMayorista { get; set; } //Cuando es por transferencia el mayorista debe pagarle a su proveedor
        public bool IsCreatedMovileApp { get; set; } //Para saber cuando una orden se creo por la app movil de Rapid
        public Guid? InvoiceId { get; set; } // Factura para los tramites que se compren desde la app movil de Rapid
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }
        [NotMapped]
        public List<ShippingOrder> ShippingOrders { get; set; } //Usado para el listar equipaje

        public bool LackSend { get; set; }  // es true cuando se envia un equipaje y el paquete  existe en otros equipajes que todavia no se han enviado
        public bool LackReview { get; set; }  // es true cuando se envia un es true cuando se revisa un equipaje y el paquete  existe en otros equipajes que todavia no se han reviado

        public Discount Discount { get; set; }
        public Minorista Minorista { get; set; }

        public Guid? RepartidorId { get; set; }
        [ForeignKey("RepartidorId")]
        public User Repartidor { get; set; }

        public Guid? DistributorId { get; set; }
        /// <summary>
        /// El distribudor es el encargado de entregar los paquetes en Cuba
        /// </summary>
        [ForeignKey("DistributorId")]
        public User Distributor { get; set; }

        public Guid? PrincipalDistributorId { get; set; }
        /// <summary>
        /// Es el encargado de distribuir los paquetes al rol Distribuidor
        /// </summary>
        [ForeignKey("PrincipalDistributorId")]
        public User PrincipalDistributor { get; set; }

        public string FileNameAttachment { get; set; }
        public decimal ProductsShipping { get; set; }

        public string NumberWholesaler { get; set; }

        public List<AttachmentOrder> Attachments { get; set; }
    }

    public class ShippingOrder
    {
        public Guid ShippingId { get; set; }
        public string ShippingNumber { get; set; }
    }
}
