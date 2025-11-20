using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P_F.Models.Entities
{
    public class Repuesto
    {
        public int RepuestoId { get; set; }

        [Required]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required]
        public int CategoriaRepuestoId { get; set; }

        [StringLength(100)]
        public string? Marca { get; set; }

        [StringLength(100)]
        public string? Modelo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioCosto { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeGanancia { get; set; }

        public int StockMinimo { get; set; }

        public int StockMaximo { get; set; }

        public int StockActual { get; set; }

        [StringLength(50)]
        public string? Ubicacion { get; set; }

        [StringLength(100)]
        public string? Proveedor { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Propiedades de navegación
        [ForeignKey("CategoriaRepuestoId")]
        public virtual CategoriaRepuesto CategoriaRepuesto { get; set; } = null!;
        public virtual ICollection<OrdenTrabajoRepuesto> OrdenTrabajoRepuestos { get; set; } = new List<OrdenTrabajoRepuesto>();
        public virtual ICollection<MovimientoInventario> MovimientosInventario { get; set; } = new List<MovimientoInventario>();
    }

    public class CategoriaRepuesto
    {
        public int CategoriaRepuestoId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        public virtual ICollection<Repuesto> Repuestos { get; set; } = new List<Repuesto>();
    }

    public class OrdenTrabajoRepuesto
    {
        public int OrdenTrabajoRepuestoId { get; set; }

        public int OrdenTrabajoId { get; set; }
        public int RepuestoId { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Descuento { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public bool Entregado { get; set; }

        // Propiedades de navegación
        [ForeignKey("OrdenTrabajoId")]
        public virtual OrdenTrabajo OrdenTrabajo { get; set; } = null!;

        [ForeignKey("RepuestoId")]
        public virtual Repuesto Repuesto { get; set; } = null!;
    }

    public enum TipoMovimiento
    {
        Entrada,
        Salida,
        Ajuste,
        Devolucion
    }

    public class MovimientoInventario
    {
        public int MovimientoInventarioId { get; set; }

        public int RepuestoId { get; set; }

        public TipoMovimiento TipoMovimiento { get; set; }

        public int Cantidad { get; set; }

        public int StockAnterior { get; set; }

        public int StockNuevo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Costo { get; set; }

        [StringLength(500)]
        public string? Motivo { get; set; }

        [StringLength(100)]
        public string? DocumentoReferencia { get; set; }

        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        public int? EmpleadoId { get; set; }

        // Propiedades de navegación
        [ForeignKey("RepuestoId")]
        public virtual Repuesto Repuesto { get; set; } = null!;

        [ForeignKey("EmpleadoId")]
        public virtual Empleado? Empleado { get; set; }
    }
}