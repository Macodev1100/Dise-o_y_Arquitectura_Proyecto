using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P_F.Models.Entities
{
    public enum EstadoFactura
    {
        Borrador,
        Emitida,
        Pagada,
        Cancelada,
        Anulada
    }

    public enum TipoPago
    {
        Efectivo,
        Tarjeta,
        Transferencia,
        Cheque,
        Credito
    }

    public class Factura
    {
        public int FacturaId { get; set; }

        [Required]
        [StringLength(20)]
        public string NumeroFactura { get; set; } = string.Empty;

        public DateTime FechaEmision { get; set; } = DateTime.Now;

        public DateTime? FechaVencimiento { get; set; }

        public EstadoFactura Estado { get; set; } = EstadoFactura.Borrador;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Impuestos { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Descuento { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Llaves foráneas
        public int ClienteId { get; set; }
        public int? OrdenTrabajoId { get; set; }
        public int EmpleadoId { get; set; }

        // Propiedades de navegación
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; } = null!;

        [ForeignKey("OrdenTrabajoId")]
        public virtual OrdenTrabajo? OrdenTrabajo { get; set; }

        [ForeignKey("EmpleadoId")]
        public virtual Empleado Empleado { get; set; } = null!;

        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }

    public class Pago
    {
        public int PagoId { get; set; }

        public int FacturaId { get; set; }

        public TipoPago TipoPago { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? NumeroReferencia { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public int EmpleadoId { get; set; }

        // Propiedades de navegación
        [ForeignKey("FacturaId")]
        public virtual Factura Factura { get; set; } = null!;

        [ForeignKey("EmpleadoId")]
        public virtual Empleado Empleado { get; set; } = null!;
    }
}