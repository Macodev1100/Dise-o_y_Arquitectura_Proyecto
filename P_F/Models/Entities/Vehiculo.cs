using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P_F.Models.Entities
{
    public class Vehiculo
    {
        public int VehiculoId { get; set; }

        [Required]
        [StringLength(10)]
        public string Placa { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Marca { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Modelo { get; set; } = string.Empty;

        [Range(1900, 2030)]
        public int Anio { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(17)]
        public string? NumeroChasis { get; set; }

        [StringLength(17)]
        public string? NumeroMotor { get; set; }

        [StringLength(50)]
        public string? TipoCombustible { get; set; }

        [StringLength(50)]
        public string? Transmision { get; set; }

        public int Kilometraje { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;

        // Llave foránea
        public int ClienteId { get; set; }

        // Propiedades de navegación
        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; } = null!;
        public virtual ICollection<OrdenTrabajo> OrdenesTrabajo { get; set; } = new List<OrdenTrabajo>();
        public virtual ICollection<HistorialMantenimiento> HistorialMantenimientos { get; set; } = new List<HistorialMantenimiento>();
    }
}