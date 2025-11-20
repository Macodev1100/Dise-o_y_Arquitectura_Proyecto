using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using P_F.Models.Entities;

namespace P_F.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets para las entidades del dominio
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<OrdenTrabajo> OrdenesTrabajo { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<CategoriaServicio> CategoriasServicio { get; set; }
        public DbSet<OrdenTrabajoServicio> OrdenTrabajoServicios { get; set; }
        public DbSet<Repuesto> Repuestos { get; set; }
        public DbSet<CategoriaRepuesto> CategoriasRepuesto { get; set; }
        public DbSet<OrdenTrabajoRepuesto> OrdenTrabajoRepuestos { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<HistorialMantenimiento> HistorialMantenimientos { get; set; }
        public DbSet<HistorialOrden> HistorialOrdenes { get; set; }
        public DbSet<RegistroTiempo> RegistrosTiempo { get; set; }
        public DbSet<Configuracion> Configuraciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones para Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.ClienteId);
                entity.HasIndex(e => e.DocumentoIdentidad).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configuraciones para Vehiculo
            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.HasKey(e => e.VehiculoId);
                entity.HasIndex(e => e.Placa).IsUnique();
                entity.HasOne(e => e.Cliente)
                      .WithMany(c => c.Vehiculos)
                      .HasForeignKey(e => e.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuraciones para OrdenTrabajo
            modelBuilder.Entity<OrdenTrabajo>(entity =>
            {
                entity.HasKey(e => e.OrdenTrabajoId);
                entity.HasIndex(e => e.NumeroOrden).IsUnique();
                
                entity.HasOne(e => e.Cliente)
                      .WithMany(c => c.OrdenesTrabajo)
                      .HasForeignKey(e => e.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Vehiculo)
                      .WithMany(v => v.OrdenesTrabajo)
                      .HasForeignKey(e => e.VehiculoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EmpleadoAsignado)
                      .WithMany(emp => emp.OrdenesAsignadas)
                      .HasForeignKey(e => e.EmpleadoAsignadoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EmpleadoRecepcion)
                      .WithMany(emp => emp.OrdenesRecibidas)
                      .HasForeignKey(e => e.EmpleadoRecepcionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuraciones para Empleado
            modelBuilder.Entity<Empleado>(entity =>
            {
                entity.HasKey(e => e.EmpleadoId);
                entity.HasIndex(e => e.DocumentoIdentidad).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configuraciones para Servicio
            modelBuilder.Entity<Servicio>(entity =>
            {
                entity.HasKey(e => e.ServicioId);
                entity.HasOne(e => e.CategoriaServicio)
                      .WithMany(c => c.Servicios)
                      .HasForeignKey(e => e.CategoriaServicioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuraciones para OrdenTrabajoServicio
            modelBuilder.Entity<OrdenTrabajoServicio>(entity =>
            {
                entity.HasKey(e => e.OrdenTrabajoServicioId);
                entity.HasOne(e => e.OrdenTrabajo)
                      .WithMany(o => o.Servicios)
                      .HasForeignKey(e => e.OrdenTrabajoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Servicio)
                      .WithMany(s => s.OrdenTrabajoServicios)
                      .HasForeignKey(e => e.ServicioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuraciones para Repuesto
            modelBuilder.Entity<Repuesto>(entity =>
            {
                entity.HasKey(e => e.RepuestoId);
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.HasOne(e => e.CategoriaRepuesto)
                      .WithMany(c => c.Repuestos)
                      .HasForeignKey(e => e.CategoriaRepuestoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuraciones para OrdenTrabajoRepuesto
            modelBuilder.Entity<OrdenTrabajoRepuesto>(entity =>
            {
                entity.HasKey(e => e.OrdenTrabajoRepuestoId);
                entity.HasOne(e => e.OrdenTrabajo)
                      .WithMany(o => o.Repuestos)
                      .HasForeignKey(e => e.OrdenTrabajoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Repuesto)
                      .WithMany(r => r.OrdenTrabajoRepuestos)
                      .HasForeignKey(e => e.RepuestoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuraciones para MovimientoInventario
            modelBuilder.Entity<MovimientoInventario>(entity =>
            {
                entity.HasKey(e => e.MovimientoInventarioId);
                entity.HasOne(e => e.Repuesto)
                      .WithMany(r => r.MovimientosInventario)
                      .HasForeignKey(e => e.RepuestoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Empleado)
                      .WithMany()
                      .HasForeignKey(e => e.EmpleadoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuraciones para Factura
            modelBuilder.Entity<Factura>(entity =>
            {
                entity.HasKey(e => e.FacturaId);
                entity.HasIndex(e => e.NumeroFactura).IsUnique();
                
                entity.HasOne(e => e.Cliente)
                      .WithMany()
                      .HasForeignKey(e => e.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.OrdenTrabajo)
                      .WithMany(o => o.Facturas)
                      .HasForeignKey(e => e.OrdenTrabajoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Empleado)
                      .WithMany()
                      .HasForeignKey(e => e.EmpleadoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuraciones para Pago
            modelBuilder.Entity<Pago>(entity =>
            {
                entity.HasKey(e => e.PagoId);
                entity.HasOne(e => e.Factura)
                      .WithMany(f => f.Pagos)
                      .HasForeignKey(e => e.FacturaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Empleado)
                      .WithMany()
                      .HasForeignKey(e => e.EmpleadoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuraciones para HistorialMantenimiento
            modelBuilder.Entity<HistorialMantenimiento>(entity =>
            {
                entity.HasKey(e => e.HistorialMantenimientoId);
                entity.HasOne(e => e.Vehiculo)
                      .WithMany(v => v.HistorialMantenimientos)
                      .HasForeignKey(e => e.VehiculoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.OrdenTrabajo)
                      .WithMany()
                      .HasForeignKey(e => e.OrdenTrabajoId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Empleado)
                      .WithMany()
                      .HasForeignKey(e => e.EmpleadoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuraciones para HistorialOrden
            modelBuilder.Entity<HistorialOrden>(entity =>
            {
                entity.HasKey(e => e.HistorialOrdenId);
                entity.HasOne(e => e.OrdenTrabajo)
                      .WithMany(o => o.HistorialEstados)
                      .HasForeignKey(e => e.OrdenTrabajoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Empleado)
                      .WithMany()
                      .HasForeignKey(e => e.EmpleadoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuraciones para RegistroTiempo
            modelBuilder.Entity<RegistroTiempo>(entity =>
            {
                entity.HasKey(e => e.RegistroTiempoId);
                entity.HasOne(e => e.Empleado)
                      .WithMany(emp => emp.RegistrosTiempo)
                      .HasForeignKey(e => e.EmpleadoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.OrdenTrabajo)
                      .WithMany()
                      .HasForeignKey(e => e.OrdenTrabajoId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuraciones para Configuracion
            modelBuilder.Entity<Configuracion>(entity =>
            {
                entity.HasKey(e => e.ConfiguracionId);
                entity.HasIndex(e => e.Clave).IsUnique();
            });

            // Datos semilla
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Categorías de Servicios
            modelBuilder.Entity<CategoriaServicio>().HasData(
                new CategoriaServicio { CategoriaServicioId = 1, Nombre = "Mantenimiento Preventivo", Descripcion = "Servicios de mantenimiento regular" },
                new CategoriaServicio { CategoriaServicioId = 2, Nombre = "Reparaciones Motor", Descripcion = "Reparaciones del sistema motor" },
                new CategoriaServicio { CategoriaServicioId = 3, Nombre = "Sistema Eléctrico", Descripcion = "Reparaciones sistema eléctrico" },
                new CategoriaServicio { CategoriaServicioId = 4, Nombre = "Frenos", Descripcion = "Mantenimiento y reparación frenos" },
                new CategoriaServicio { CategoriaServicioId = 5, Nombre = "Transmisión", Descripcion = "Reparaciones transmisión" }
            );

            // Servicios básicos
            modelBuilder.Entity<Servicio>().HasData(
                new Servicio { ServicioId = 1, Nombre = "Cambio de Aceite", Descripcion = "Cambio de aceite motor", PrecioBase = 25.00m, TiempoEstimadoMinutos = 30, CategoriaServicioId = 1 },
                new Servicio { ServicioId = 2, Nombre = "Alineación", Descripcion = "Alineación de ruedas", PrecioBase = 40.00m, TiempoEstimadoMinutos = 60, CategoriaServicioId = 1 },
                new Servicio { ServicioId = 3, Nombre = "Balanceo", Descripcion = "Balanceo de ruedas", PrecioBase = 30.00m, TiempoEstimadoMinutos = 45, CategoriaServicioId = 1 },
                new Servicio { ServicioId = 4, Nombre = "Revisión Motor", Descripcion = "Diagnóstico completo motor", PrecioBase = 80.00m, TiempoEstimadoMinutos = 120, CategoriaServicioId = 2, RequiereAutorizacion = true },
                new Servicio { ServicioId = 5, Nombre = "Cambio de Pastillas", Descripcion = "Cambio pastillas de freno", PrecioBase = 60.00m, TiempoEstimadoMinutos = 90, CategoriaServicioId = 4 }
            );

            // Categorías de Repuestos
            modelBuilder.Entity<CategoriaRepuesto>().HasData(
                new CategoriaRepuesto { CategoriaRepuestoId = 1, Nombre = "Filtros", Descripcion = "Filtros de aire, aceite, combustible" },
                new CategoriaRepuesto { CategoriaRepuestoId = 2, Nombre = "Lubricantes", Descripcion = "Aceites y lubricantes" },
                new CategoriaRepuesto { CategoriaRepuestoId = 3, Nombre = "Frenos", Descripcion = "Pastillas, discos, líquido frenos" },
                new CategoriaRepuesto { CategoriaRepuestoId = 4, Nombre = "Motor", Descripcion = "Repuestos del motor" },
                new CategoriaRepuesto { CategoriaRepuestoId = 5, Nombre = "Eléctrico", Descripcion = "Componentes eléctricos" }
            );

            // Configuraciones del sistema
            modelBuilder.Entity<Configuracion>().HasData(
                new Configuracion { ConfiguracionId = 1, Clave = "NombreTaller", Valor = "Taller Mecánico P&F", Descripcion = "Nombre del taller", Tipo = "string" },
                new Configuracion { ConfiguracionId = 2, Clave = "TelefonoTaller", Valor = "555-0123", Descripcion = "Teléfono del taller", Tipo = "string" },
                new Configuracion { ConfiguracionId = 3, Clave = "EmailTaller", Valor = "info@tallerpyf.com", Descripcion = "Email del taller", Tipo = "string" },
                new Configuracion { ConfiguracionId = 4, Clave = "DireccionTaller", Valor = "Calle Principal 123", Descripcion = "Dirección del taller", Tipo = "string" },
                new Configuracion { ConfiguracionId = 5, Clave = "PorcentajeImpuesto", Valor = "12.0", Descripcion = "Porcentaje de impuesto", Tipo = "decimal" },
                new Configuracion { ConfiguracionId = 6, Clave = "MonedaPrincipal", Valor = "USD", Descripcion = "Moneda principal", Tipo = "string" }
            );
        }
    }
}