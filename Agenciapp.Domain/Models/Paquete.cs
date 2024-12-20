using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agenciapp.Domain.Enums;
using Agenciapp.Domain.Models;

namespace AgenciappHome.Models
{
    public class Paquete
    {
        public const String STATUS_INICIADO = "Iniciado";
        public const String STATUS_PREDESPACHADO = "Pre Despachado";
        public const String STATUS_VERIFICADO = "Verificado";
        //public static String STATUS_ALMACENUSA = "Almacen USA";
        public const String STATUS_PALLET = "Pallet";
        public const String STATUS_ENGUIA = "En Guia";
        public const String STATUS_ENVIADA = "Enviada";

        #region Status Cuba
        public const String STATUS_NACIONALIZANDO = "Nacionalizando";
        public const String STATUS_TRANSITOCUBA = "TransitoCuba";
        public const String STATUS_RECIBIDOALMACEN = "RecibidoAlmacen";
        public const String STATUS_DISTRIBUIDO = "Distribuido";

        #endregion

        public const String STATUS_RECIBIDO = "Recibido";
        public const String STATUS_ADUANA = "Aduana";
        public const String STATUS_ENTREGADO = "Entregado";
        public const String STATUS_CANCELADO = "Cancelado";

        public Paquete()
        {
            RegistroEstados = new List<RegistroEstado>();
        }

        public void ChangeStatus(string status, User user, string wharehouseLocation = null)
        {
            if (user == null) throw new Exception("User is required");
            switch (status)
            {
                case STATUS_ENVIADA:
                    if (user.Type == "PrincipalDistributor" || user.Type == "DistributorCuba" || user.Type == "EmpleadoCuba")
                    {
                        throw new Exception("El usuario no tiene permisos para realizar esta acción");
                    }
                    if (Status == STATUS_ENGUIA)
                        Status = STATUS_ENVIADA;
                    else throw new Exception("Invalid status change");
                    break;
                case STATUS_NACIONALIZANDO:
                    if (user.Type != "PrincipalDistributor")
                    {
                        throw new Exception("El usuario no tiene permisos para realizar esta acción");
                    }
                    if (Status == STATUS_ENVIADA)
                        Status = STATUS_NACIONALIZANDO;
                    else throw new Exception("Invalid status change");
                    break;
                case STATUS_TRANSITOCUBA:
                    if (user.Type != "PrincipalDistributor")
                    {
                        throw new Exception("El usuario no tiene permisos para realizar esta acción");
                    }
                    if (Status == STATUS_NACIONALIZANDO)
                    {
                        Status = STATUS_TRANSITOCUBA;
                        WarehouseLocation = wharehouseLocation;
                        if (string.IsNullOrEmpty(WarehouseLocation))
                        {
                            throw new Exception("Warehouse location is required");
                        }
                        if (WarehouseLocation != "Habana" && WarehouseLocation != "Camaguey" && WarehouseLocation != "Holguin")
                        {
                            throw new Exception("Invalid warehouse location");
                        }
                    }
                    else throw new Exception("Invalid status change");
                    break;
                case STATUS_RECIBIDOALMACEN:
                    if (user.Type != "DistributorCuba")
                    {
                        throw new Exception("El usuario no tiene permisos para realizar esta acción");
                    }
                    if (Status == STATUS_TRANSITOCUBA)
                    {
                        Status = STATUS_RECIBIDOALMACEN;
                        if (WarehouseLocation != user.WharehouseLocation)
                            throw new Exception("El paquete no pertenece al almacen del empleado");
                    }
                    else throw new Exception("Invalid status change");
                    break;
                case STATUS_DISTRIBUIDO:
                    if (user.Type != "DistributorCuba")
                    {
                        throw new Exception("El usuario no tiene permisos para realizar esta acción");
                    }
                    if (Status == STATUS_RECIBIDOALMACEN)
                    {
                        Status = STATUS_DISTRIBUIDO;
                        if (WarehouseLocation != user.WharehouseLocation)
                            throw new Exception("El paquete no pertenece al almacen del empleado");
                    }
                    else throw new Exception("Invalid status change");
                    break;
                case STATUS_ENTREGADO:
                    if (user.Type != "DistributorCuba" && user.Type != "EmpleadoCuba")
                    {
                        throw new Exception("El usuario no tiene permisos para realizar esta acción");
                    }
                    if (Status == STATUS_DISTRIBUIDO)
                    {
                        Status = STATUS_ENTREGADO;
                        if (WarehouseLocation != user.WharehouseLocation)
                            throw new Exception("El paquete no pertenece al almacen del empleado");
                    }
                    else throw new Exception("Invalid status change");
                    break;
                case STATUS_CANCELADO:
                    if (user.Type == "PrincipalDistributor" || user.Type == "DistributorCuba" || user.Type == "EmpleadoCuba")
                    {
                        throw new Exception("El usuario no tiene permisos para realizar esta acción");
                    }
                    if (Status == STATUS_INICIADO || Status == STATUS_RECIBIDO)
                    {
                        Status = STATUS_CANCELADO;
                    }
                    else throw new Exception("No se puede cancelar la bolsa en el estado actual");
                    break;
                default:
                    throw new Exception("Invalid status");
            }

            RegistroEstados.Add(new RegistroEstado
            {
                Paquete = this,
                PaqueteId = PaqueteId,
                Estado = status,
                Date = DateTime.Now,
                UserId = user.UserId
            });
        }

        public void Delivered(string signature, string image, User user)
        {
            if (Status == STATUS_DISTRIBUIDO)
            {
                IsDelivered = true;
                SignatureDelivered = signature;
                ImageDelivered = image;
                DateDelivered = DateTime.Now;

                ChangeStatus(STATUS_ENTREGADO, user);
            }
            else throw new Exception("Invalid status");
        }

        public void MarkNotDelivered(string description)
        {
            IsNotDelivered = true;
            DescriptionNotDelivered = description;
        }

        public Guid PaqueteId { get; set; }
        public DateTime Date { get; set; }
        public Guid? PreDespachoId { get; set; }
        public Guid? GuiaAereaId { get; set; }
        public Guid? PalletId { get; set; }
        public Guid? AduanaId { get; set; }
        public string Descripcion { get; set; }
        public decimal PesoLb { get; set; }
        public decimal PesoKg { get; set; }
        public decimal Largo { get; set; }
        public decimal Alto { get; set; }
        public decimal Ancho { get; set; }
        public string Numero { get; set; }
        public string Suffix { get; set; }
        public decimal Precio { get; set; }
        public decimal Costo { get; set; }
        public decimal CostoCubiq { get; set; }
        public decimal Aduanal { get; set; }
        public string AduanaDescription { get; set; }
        public decimal RawMaterial { get; set; }
        public bool IsAutomatico { get; set; }
        public Guid OrderCubiqId { get; set; }
        public OrderCubiq OrderCubiq { get; set; }

        [ForeignKey("GuiaAereaId")]
        public GuiaAerea GuiaAerea { get; set; }

        [ForeignKey("PreDespachoId")]
        public PreDespachoCubiq PreDespacho { get; set; }

        public Pallet Pallet { get; set; }

        [ForeignKey("AduanaId")]
        public Aduana Aduana { get; set; }

        public string Status { get; set; }
        public string WarehouseLocation { get; set; }
        public CubiqPackageType Type { get; set; }

        // Para cuando no se puede completar la entrega
        public bool IsNotDelivered { get; private set; }
        public string DescriptionNotDelivered { get; set; }

        // Para cuando se entrega
        public bool IsDelivered { get; set; }
        public string SignatureDelivered { get; set; }
        public string ImageDelivered { get; set; }
        public DateTime? DateDelivered { get; set; }

        public List<RegistroEstado> RegistroEstados { get; set; }
    }
}