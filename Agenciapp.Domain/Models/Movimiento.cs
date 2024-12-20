using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{
    public class Movimiento
    {
        [NotMapped]
        public const string MOVIMIENTO_IN = "Ingreso Producto";
        [NotMapped]
        public const string MOVIMIENTO_OUT = "Salida Producto";
        [NotMapped]
        public const string MOVIMIENTO_ADJUST = "Ajustar Producto";
        [NotMapped]
        public const string MOVIMIENTO_TRASLATE = "Trasladar Producto";

        [Key]
        public Guid IdMovimiento { get; set; }

        public Guid IdAgency { get; set; }

        [Column("IdBodega_Origen")]
        [ForeignKey("BodegaOrigen")]
        [Display(Name="Bodega Origen")]
        public Guid? IdBodegaOrigen { get; set; }

        [Display(Name="Bodega Origen")]
        public Bodega BodegaOrigen { get; set; }

        [Column("IdBodega_Destino")]
        [ForeignKey("BodegaDestino")]
        [Display(Name="Bodega Destino")]
        public Guid? IdBodegaDestino { get; set; }

        [Display(Name="Bodega Destino")]
        public Bodega BodegaDestino { get; set; }

        [Required]
        [Column("IdTipo_Movimiento")]
        [Display(Name="Tipo de Movimiento")]
        public Guid IdTipoMovimiento { get; set; }

        [Display(Name="Tipo")]
        public TipoMovimiento TipoMovimiento { get; set; }

        [Display(Name="Proveedor")]
        public Guid? IdProveedor { get; set; }
        public Wholesaler Proveedor { get; set; }

        [StringLength(250)]
        [Display(Name="Observación")]
        public string Observacion { get; set; }

        [Column("No_Documento")]
        [StringLength(30)]
        [Display(Name="No. Documento")]
        public string NoDocumento { get; set; }

        [Column("Moneda_Original")]
        [StringLength(50)]
        [Display(Name="Moneda Original")]
        public string MonedaOriginal { get; set; }

        [Column("Unidad_Medida_Original")]
        [StringLength(50)]
        [Display(Name="Unidad de Medida Original")]
        public string UnidadMedidaOriginal { get; set; }

        [Column("IdEstado_Movimiento")]
        [Display(Name="Estado de Movimiento")]
        public Guid? IdEstadoMovimiento { get; set; }

        [Display(Name="Estado")]
        public EstadoMovimiento EstadoMovimiento { get; set; }

        [Column("IdTipo_Documento")]
        [Display(Name="Tipo de Documento")]
        public Guid? IdTipoDocumento { get; set; }
        
        [Display(Name="Tipo de Documento")]
        public TipoDocumento TipoDocumento { get; set; }
    }
}