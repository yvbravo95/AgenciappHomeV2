using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agenciapp.Domain.Models
{
    public class BagEM
    {
        public const string STATUS_INICIADO = "Iniciado";
        public const string STATUS_RECIBIDO = "Recibido";
        public const string STATUS_ENVIADO = "Enviado"; // Para Cuba es Transito Cuba-USA
        public const string STATUS_NACIONALIZANDO = "Nacionalizando"; // Para Cuba es Nacionalizando (PrincipalDistributor)
        public const string STATUS_TRANSITOCUBA = "TransitoCuba"; // Para Cuba es Transito Cuba (Almacen Habana, Camaguey, Holguin) (PrincipalDistributor)
        public const string STATUS_RECIBIDOALMACEN = "RecibidoAlmacen"; // Para Cuba es Recibido Almacen (Almacen Habana, Camaguey, Holguin) (DistributorCuba)
        public const string STATUS_DISTRIBUIDO = "Distribuido"; // Para Cuba es Distribuido (DistributorCuba)
        public const string STATUS_ENTREGADO = "Entregado"; // Para Cuba es Entregado (EmpleadoCuba)
        public const string STATUS_CANCELADO = "Cancelado"; // Para Cuba es Cancelado

        public BagEM()
        {
            RegistroEstados = new List<RegistroEstado>();
        }

        [Key]
        public Guid Id { get; set; }
        public Guid EnvioMaritimoId { get; set; }
        public DateTime CreateAt { get; set; }
        public string Number { get; set; }
        public string Status { get; set; }
        public string WarehouseLocation { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public decimal Weight { get; set; }
        public decimal Price { get; set; }

        // Para cuando no se puede completar la entrega
        public bool IsNotDelivered { get; private set; }
        public string DescriptionNotDelivered { get; set; }

        // Para cuando se entrega
        public bool IsDelivered { get; set; }
        public string SignatureDelivered { get; set; }
        public string ImageDelivered { get; set; }
        public DateTime? DateDelivered { get; set; }


        [ForeignKey("EnvioMaritimoId")]
        public EnvioMaritimo EnvioMaritimo { get; set; }

        public List<RegistroEstado> RegistroEstados { get; set; }

        public void ChangeStatus(string status, User user, string wharehouseLocation = null)
        {
            if(user == null) throw new Exception("User is required");
            switch (status)
            {
                case STATUS_RECIBIDO:
                    if (user.Type == "PrincipalDistributor" || user.Type == "DistributorCuba" || user.Type == "EmpleadoCuba")
                    {
                        throw new Exception("El usuario no tiene permisos para realizar esta acción");
                    }
                    if (Status == STATUS_INICIADO)
                        Status = STATUS_RECIBIDO;
                    else throw new Exception("Invalid status change");
                    break;
                case STATUS_ENVIADO:
                    if (user.Type == "PrincipalDistributor" || user.Type == "DistributorCuba" || user.Type == "EmpleadoCuba")
                    {
                        throw new Exception("El usuario no tiene permisos para realizar esta acción");
                    }
                    if (Status == STATUS_RECIBIDO)
                        Status = STATUS_ENVIADO;
                    else throw new Exception("Invalid status change");
                    break;
                case STATUS_NACIONALIZANDO:
                    if(user.Type != "PrincipalDistributor")
                    {
                        throw new Exception("El usuario no tiene permisos para realizar esta acción");
                    }
                    if (Status == STATUS_ENVIADO)
                        Status = STATUS_NACIONALIZANDO;
                    else throw new Exception("Invalid status change");
                    break;
                case STATUS_TRANSITOCUBA:
                    if(user.Type != "PrincipalDistributor")
                    {
                        throw new Exception("El usuario no tiene permisos para realizar esta acción");
                    }
                    if (Status == STATUS_NACIONALIZANDO)
                    {
                        Status = STATUS_TRANSITOCUBA;
                        WarehouseLocation = wharehouseLocation;
                        if(string.IsNullOrEmpty(WarehouseLocation))
                        {
                            throw new Exception("Warehouse location is required");
                        }
                        if(WarehouseLocation != "Habana" && WarehouseLocation != "Camaguey" && WarehouseLocation != "Holguin")
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
                BagEM = this,
                BagEMId = Id,
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
    }
}
