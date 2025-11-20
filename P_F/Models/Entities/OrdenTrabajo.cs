using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P_F.Models.Entities
{
    public enum EstadoOrden
    {
        Pendiente,
        EnProceso,
        Pausada,
        Completada,
        Cancelada,
        Entregada
    }

    public enum Prioridad
    {
        Baja,
        Normal,
        Alta,
        Urgente
    }

    public class OrdenTrabajo
    {
        public int OrdenTrabajoId { get; set; }

        [Required]
        [StringLength(20)]
        public string NumeroOrden { get; set; } = string.Empty;

        [Required]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        public DateTime? FechaPromesaEntrega { get; set; }

        public DateTime? FechaEntrega { get; set; }

        [Required]
        public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;

        public Prioridad Prioridad { get; set; } = Prioridad.Normal;

        [StringLength(1000)]
        public string? DescripcionProblema { get; set; }

        [StringLength(1000)]
        public string? ObservacionesCliente { get; set; }

        [StringLength(1000)]
        public string? DiagnosticoTecnico { get; set; }

        [StringLength(500)]
        public string? RecomendacionesTecnico { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Impuestos { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Descuento { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public bool RequiereAutorizacion { get; set; }

        public bool AutorizadoPorCliente { get; set; }

        public DateTime? FechaAutorizacion { get; set; }

        public bool Activo { get; set; } = true;

        // Llaves foráneas
        public int ClienteId { get; set; }
        public int VehiculoId { get; set; }
        public int? EmpleadoAsignadoId { get; set; }
        public int? EmpleadoRecepcionId { get; set; }

        // Propiedades de navegación
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; } = null!;

        [ForeignKey("VehiculoId")]
        public virtual Vehiculo Vehiculo { get; set; } = null!;

        [ForeignKey("EmpleadoAsignadoId")]
        public virtual Empleado? EmpleadoAsignado { get; set; }

        [ForeignKey("EmpleadoRecepcionId")]
        public virtual Empleado? EmpleadoRecepcion { get; set; }

        public virtual ICollection<OrdenTrabajoServicio> Servicios { get; set; } = new List<OrdenTrabajoServicio>();
        public virtual ICollection<OrdenTrabajoRepuesto> Repuestos { get; set; } = new List<OrdenTrabajoRepuesto>();
        public virtual ICollection<HistorialOrden> HistorialEstados { get; set; } = new List<HistorialOrden>();
        public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    }
}