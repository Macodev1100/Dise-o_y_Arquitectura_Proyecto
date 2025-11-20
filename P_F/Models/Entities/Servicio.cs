using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P_F.Models.Entities
{
    public class Servicio
    {
        public int ServicioId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioBase { get; set; }

        public int TiempoEstimadoMinutos { get; set; }

        [Required]
        public int CategoriaServicioId { get; set; }

        public bool RequiereAutorizacion { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Propiedades de navegación
        [ForeignKey("CategoriaServicioId")]
        public virtual CategoriaServicio CategoriaServicio { get; set; } = null!;
        public virtual ICollection<OrdenTrabajoServicio> OrdenTrabajoServicios { get; set; } = new List<OrdenTrabajoServicio>();
    }

    public class CategoriaServicio
    {
        public int CategoriaServicioId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
    }

    public class OrdenTrabajoServicio
    {
        public int OrdenTrabajoServicioId { get; set; }

        public int OrdenTrabajoId { get; set; }
        public int ServicioId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        public int Cantidad { get; set; } = 1;

        [Column(TypeName = "decimal(5,2)")]
        public decimal Descuento { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public bool Completado { get; set; }

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        // Propiedades de navegación
        [ForeignKey("OrdenTrabajoId")]
        public virtual OrdenTrabajo OrdenTrabajo { get; set; } = null!;

        [ForeignKey("ServicioId")]
        public virtual Servicio Servicio { get; set; } = null!;
    }
}