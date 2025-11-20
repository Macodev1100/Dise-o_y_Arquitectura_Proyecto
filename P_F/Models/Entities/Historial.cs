using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P_F.Models.Entities
{
    public class HistorialMantenimiento
    {
        public int HistorialMantenimientoId { get; set; }

        public int VehiculoId { get; set; }

        public DateTime FechaServicio { get; set; }

        public int KilometrajeServicio { get; set; }

        [StringLength(200)]
        public string TipoServicio { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Costo { get; set; }

        public int? OrdenTrabajoId { get; set; }

        public int EmpleadoId { get; set; }

        // Propiedades de navegación
        [ForeignKey("VehiculoId")]
        public virtual Vehiculo Vehiculo { get; set; } = null!;

        [ForeignKey("OrdenTrabajoId")]
        public virtual OrdenTrabajo? OrdenTrabajo { get; set; }

        [ForeignKey("EmpleadoId")]
        public virtual Empleado Empleado { get; set; } = null!;
    }

    public class HistorialOrden
    {
        public int HistorialOrdenId { get; set; }

        public int OrdenTrabajoId { get; set; }

        public EstadoOrden EstadoAnterior { get; set; }

        public EstadoOrden EstadoNuevo { get; set; }

        public DateTime FechaCambio { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public int EmpleadoId { get; set; }

        // Propiedades de navegación
        [ForeignKey("OrdenTrabajoId")]
        public virtual OrdenTrabajo OrdenTrabajo { get; set; } = null!;

        [ForeignKey("EmpleadoId")]
        public virtual Empleado Empleado { get; set; } = null!;
    }

    public class RegistroTiempo
    {
        public int RegistroTiempoId { get; set; }

        public int EmpleadoId { get; set; }

        public int? OrdenTrabajoId { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public int MinutosTrabajados { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostoHora { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostoTotal { get; set; }

        // Propiedades de navegación
        [ForeignKey("EmpleadoId")]
        public virtual Empleado Empleado { get; set; } = null!;

        [ForeignKey("OrdenTrabajoId")]
        public virtual OrdenTrabajo? OrdenTrabajo { get; set; }
    }

    public class Configuracion
    {
        public int ConfiguracionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Valor { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [StringLength(50)]
        public string? Tipo { get; set; }

        public DateTime FechaModificacion { get; set; } = DateTime.Now;
    }
}