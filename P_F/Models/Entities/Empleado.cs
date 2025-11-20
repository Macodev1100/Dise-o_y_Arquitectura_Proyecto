using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P_F.Models.Entities
{
    public enum TipoEmpleado
    {
        Administrador,
        Mecanico,
        Recepcionista,
        Supervisor,
        Ayudante
    }

    public class Empleado
    {
        public int EmpleadoId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string DocumentoIdentidad { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [Required]
        public TipoEmpleado TipoEmpleado { get; set; }

        [StringLength(100)]
        public string? Especialidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalarioHora { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeComision { get; set; }

        public DateTime FechaContratacion { get; set; } = DateTime.Now;

        public DateTime? FechaTerminacion { get; set; }

        public bool Activo { get; set; } = true;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Para integración con Identity
        [StringLength(450)]
        public string? UserId { get; set; }

        // Propiedades de navegación
        public virtual ICollection<OrdenTrabajo> OrdenesAsignadas { get; set; } = new List<OrdenTrabajo>();
        public virtual ICollection<OrdenTrabajo> OrdenesRecibidas { get; set; } = new List<OrdenTrabajo>();
        public virtual ICollection<RegistroTiempo> RegistrosTiempo { get; set; } = new List<RegistroTiempo>();
    }
}