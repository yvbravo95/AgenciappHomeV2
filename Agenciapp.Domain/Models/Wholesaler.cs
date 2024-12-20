using Agenciapp.Domain.Enums;
using RapidMultiservice.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class Wholesaler //Mayorista
    {
        public Wholesaler()
        {
            Tickets = new HashSet<Ticket>();
            EsVisible = true;
            tipoServicioHabana = new List<TipoServicioMayorista>();
            tipoServicioRestoProv = new List<TipoServicioMayorista>();
            CostByProvinces = new List<CostByProvince>();
            CreditWholesalers = new List<CreditWholesaler>();
        }

        [Key]
        public Guid IdWholesaler { get; set; }
        [Display(Name = "Nombre")]
        public string name { get; set; }
        [Display(Name = "Dirección")]
        public string address { get; set; }
        [Display(Name = "Teléfono")]
        public string phone { get; set; }
        [Display(Name = "Email")]
        public string email { get; set; }
        [Display(Name = "Cuenta de banco")]
        public string bankAccount { get; set; }
        [Display(Name = "Términos y condiciones")] //Descripcion servicio
        public string TermsConditions { get; set; }
        [Display(Name = "Políticas de Cancelaciones")] // Terminos y politicas de cancelación
        public string CancellationPolicies { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
        public Guid AgencyId { get; set; }
        [Display(Name = "Categoría")]
        public Category Category { get; set; }
        [Display(Name = "Subcategoría")]
        public Subcategory Subcategory { get; set; }
        public bool Default { get; set; }
        public bool byTransferencia { get; set; } //Para especificar si el mayorista es por transferencia
        public decimal CostoMayorista { get; set; }// Costo para el mayorista
        public decimal Costo2Mayorista { get; set; }// Costo para el mayorista(caso de remesa) - Costo Porciento
        public decimal Costo3Mayorista { get; set; }// Costo para el mayorista(caso de remesa) - Costo Fijo (USD)
        public decimal Costo4Mayorista { get; set; }// Costo para el mayorista(caso de remesa) - Costo Porciento (USD)
        public decimal Costo5Mayorista { get; set; }// Costo para el mayorista(caso de remesa) - Costo Fijo (USD_TARJETA)
        public decimal Costo6Mayorista { get; set; }// Costo para el mayorista(caso de remesa) - Costo Porciento (USD_TARJETA)
        public decimal Costo7Mayorista { get; set; }// Costo para el mayorista(caso de remesa excepto habana) - Costo Fijo (CUP)
        public decimal Costo8Mayorista { get; set; }// Costo para el mayorista(caso de remesa excepto habana) - Costo Porciento (CUP)
        public decimal Costo9Mayorista { get; set; }// Costo para el mayorista(caso de remesa excepto habana) - Costo Fijo (USD)
        public decimal Costo10Mayorista { get; set; }// Costo para el mayorista(caso de remesa excepto habana) - Costo Porciento (USD)
        public decimal Costo11Mayorista { get; set; }// Costo para el mayorista(caso de remesa excepto habana) - Costo Fijo (USD_TARJETA)
        public decimal Costo12Mayorista { get; set; }// Costo para el mayorista(caso de remesa excepto habana) - Costo Porciento (USD_TARJETA)
        public bool EsVisible { get; set; } //Si es false al crear un tramite no aprece el mayorista
        public decimal precioVenta { get; set; } 
        public decimal precioVenta2 { get; set; } 
        public decimal precioVentaMedicina { get; set; } // Precio de venta usado en Combos 

        public decimal PrecioCubacel { get; set; } // Si la categoría es recarga
        public decimal PrecioNauta { get; set; } // Si la categoria es recarga
        public decimal PrecioCelular { get; set; } // Si la categoría es recarga
        public decimal costoCubacel { get; set; } // Costo para cubacel
        public decimal costoNauta { get; set; } // Costo para nauta
        public decimal costoCelular { get; set; } //Costo para celular

        public bool Comodin { get; set; } //Al marcarse el tramite siempre va a usar este mayorista
        public virtual ICollection<Movimiento> movimientos { get; set; } //de Bodega
        public List<UserWholesaler> UserWholesalers { get; set; } //Relacion muchos a muchos con User

        public List<TipoServicioMayorista> tipoServicioHabana { get; set; } //Para envios caribe
        public List<TipoServicioMayorista> tipoServicioRestoProv { get; set; } //Para envios caribe

        public List<ServConsularMayorista> ServConsularMayoristas { get; set; } //Para pasaporte

        public decimal AddPrecioCombo { get; set; } //Para que los minoristas puedan aumentar el precio que le da su mayorista 

        public List<CostByProvince> CostByProvinces { get; set; }

        public List<CreditWholesaler> CreditWholesalers { get; private set; }
        public decimal creditUsd { get; private set; } //Puede ser negativo
        public decimal creditCup { get; private set; } //Puede ser negativo
        public decimal creditEur { get; private set; } //Puede ser negativo

        public decimal ExchangeRate { get; set; } //Tasa de cambio para remesas

        public Result<string> AddCredit(databaseContext context, decimal amount, MoneyType type, string referencia)
        {
            try
            {
                if(amount == 0)
                {
                    return new Result<string>
                    {
                        Success = false,
                        ErrorMessage = "El monto no puede ser igual a 0"
                    };
                }
                var credit = new CreditWholesaler
                {
                    MoneyType = type,
                    Referencia = referencia,
                    value = amount,
                    Date = DateTime.UtcNow,
                    WholesalerId = this.IdWholesaler
                };
                if (type == MoneyType.USD) 
                {
                    this.creditUsd += amount;
                }
                else if(type == MoneyType.CUP)
                {
                    this.creditCup += amount;
                }
                else if(type == MoneyType.USD_TARJETA)
                {
                    this.creditEur += amount;
                }
                
                context.CreditWholesalers.Add(credit);
                context.Update(this);
                return new Result<string>
                {
                    Success = true
                };
            }
            catch(Exception e)
            {
                return new Result<string>
                {
                    Success = false,
                    ErrorMessage = e.Message
                };
            }
        }
        public Result<string> DeleteCredit(databaseContext context, CreditWholesaler credit)
        {
            try
            {
                if (!context.CreditWholesalers.Any(x => x.Id == credit.Id))
                {
                    return new Result<string>
                    {
                        Success = false,
                        ErrorMessage = "El credito no existe."
                    };
                }
                if (credit.MoneyType == MoneyType.CUP)
                {
                    creditCup -= credit.value;
                }
                else if(credit.MoneyType == MoneyType.USD)
                {
                    creditUsd -= credit.value;
                }
                else if(credit.MoneyType == MoneyType.USD_TARJETA)
                {
                    creditEur -= credit.value;
                }

                context.CreditWholesalers.Remove(credit);
                return new Result<string>
                {
                    Success = true
                };
            }
            catch(Exception e)
            {
                return new Result<string>
                {
                    Success = false,
                    ErrorMessage = e.Message
                };
            }
        }
        public Result<string> GetCredit(databaseContext context, decimal amount, MoneyType type)
        {
            try
            {
                if (type == MoneyType.USD)
                {
                    this.creditUsd -= amount;
                }
                else if(type == MoneyType.CUP)
                {
                    this.creditCup -= amount;
                }
                else if(type == MoneyType.USD_TARJETA)
                {
                    this.creditEur -= amount;
                }
                context.Update(this);
                return new Result<string>
                {
                    Success = true,
                };
            }
            catch(Exception e)
            {
                return new Result<string>
                {
                    Success = false,
                    ErrorMessage = e.Message
                };
            }
        }

        public TypeMaritimo TypeMaritimo { get; set; }
        [Column(TypeName = "decimal(18,9)")]
        public decimal PriceTypePackage { get; set; }
        [Column(TypeName = "decimal(18,9)")]
        public decimal CostTypePackage { get; set; }
        public string Tags { get; set; }
    }

    public class TipoServicioMayorista //Para envios Caribe
    {
        public static String Trans_Cargo = "Trans Cargo";
        public static String Palco = "Palco";
        public static String Aereo_Varadero = "Aereo Varadero";
        public static String Cuba_Pack = "Cuba Pack";
        public static String Correo_Cuba = "Correo Cuba";

        public Guid TipoServicioMayoristaId { get; set; }
        public string Nombre { get; set; }

        public decimal costoAereo { get; set; }
        public decimal pventaAereo { get; set; }
        public decimal costoMaritimo { get; set; }
        public decimal pventaMaritimo { get; set; }

    }

    public class ServConsularMayorista
    {
        [Key]
        public Guid ServConsularMayoristaId { get; set; }
        public ServicioConsular servicio { get; set; }
        public decimal precio { get; set; }
        public decimal costo { get; set; }
        public Guid WholesalerId { get; set; }
        public Wholesaler Wholesaler { get; set; }
    }

    public enum TypeCostByProvince
    {
        Paquete,
        Medicina,
        Combo
    }

    public class CostByProvince
    {
        [Key]
        public Guid CostByProvinceId { get; set; }
        public decimal Cost { get; set; }
        public decimal Cost2 { get; set; }
        public decimal Cost3 { get; set; }
        public decimal Cost4 { get; set; }
        public Guid ProvinciaId { get; set; }
        [ForeignKey("ProvinciaId")]
        public Provincia Provincia { get; set; }
        public TypeCostByProvince Type { get; set; }
        public Guid WholesalerId { get; set; }
        [ForeignKey("WholesalerId")]
        public Wholesaler Wholesaler { get; set; }
    }

    public enum Subcategory
    {
        None,
        Maritimo,
        Aereo
    }

}
