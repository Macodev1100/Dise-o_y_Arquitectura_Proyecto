using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using P_F.Data;
using P_F.Models.Entities;

namespace P_F.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Asegurar que la base de datos esté creada
            await context.Database.EnsureCreatedAsync();

            // Crear roles si no existen
            await CreateRoles(roleManager);

            // Crear usuario administrador si no existe
            await CreateAdminUser(userManager, context);

            // Agregar datos de ejemplo si la base está vacía
            await SeedSampleData(context);
        }

        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Administrador", "Mecanico", "Recepcionista", "Supervisor" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task CreateAdminUser(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            var adminEmail = "admin@tallerpyf.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Administrador");

                    // Crear empleado asociado
                    var empleado = new Empleado
                    {
                        Nombre = "Administrador",
                        Apellido = "Sistema",
                        DocumentoIdentidad = "ADMIN001",
                        Telefono = "555-0000",
                        Email = adminEmail,
                        TipoEmpleado = TipoEmpleado.Administrador,
                        SalarioHora = 0,
                        PorcentajeComision = 0,
                        UserId = user.Id,
                        Activo = true
                    };

                    context.Empleados.Add(empleado);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedSampleData(ApplicationDbContext context)
        {
            // Verificar si ya hay datos
            if (await context.Clientes.AnyAsync())
                return;

            // Crear algunos repuestos de ejemplo
            var repuestos = new List<Repuesto>
            {
                new() { Codigo = "ACE001", Nombre = "Aceite Motor 5W30", CategoriaRepuestoId = 2, PrecioCosto = 8.50m, PrecioVenta = 12.00m, StockActual = 50, StockMinimo = 10, StockMaximo = 100, Marca = "Mobil1" },
                new() { Codigo = "FIL001", Nombre = "Filtro Aceite", CategoriaRepuestoId = 1, PrecioCosto = 3.20m, PrecioVenta = 5.50m, StockActual = 30, StockMinimo = 5, StockMaximo = 50, Marca = "Mann" },
                new() { Codigo = "PAS001", Nombre = "Pastillas Freno Delanteras", CategoriaRepuestoId = 3, PrecioCosto = 25.00m, PrecioVenta = 35.00m, StockActual = 20, StockMinimo = 5, StockMaximo = 30, Marca = "Brembo" }
            };

            context.Repuestos.AddRange(repuestos);

            // Crear algunos clientes de ejemplo
            var clientes = new List<Cliente>
            {
                new() { Nombre = "Juan", Apellido = "Pérez", DocumentoIdentidad = "1234567890", Telefono = "555-1234", Email = "juan.perez@email.com", Direccion = "Calle 1 #123" },
                new() { Nombre = "María", Apellido = "González", DocumentoIdentidad = "0987654321", Telefono = "555-5678", Email = "maria.gonzalez@email.com", Direccion = "Avenida 2 #456" },
                new() { Nombre = "Carlos", Apellido = "Rodríguez", DocumentoIdentidad = "1122334455", Telefono = "555-9012", Email = "carlos.rodriguez@email.com", Direccion = "Plaza 3 #789" }
            };

            context.Clientes.AddRange(clientes);
            await context.SaveChangesAsync();

            // Crear algunos vehículos de ejemplo
            var vehiculos = new List<Vehiculo>
            {
                new() { Placa = "ABC-123", Marca = "Toyota", Modelo = "Corolla", Anio = 2018, Color = "Blanco", Kilometraje = 75000, ClienteId = 1 },
                new() { Placa = "DEF-456", Marca = "Honda", Modelo = "Civic", Anio = 2020, Color = "Negro", Kilometraje = 45000, ClienteId = 2 },
                new() { Placa = "GHI-789", Marca = "Nissan", Modelo = "Sentra", Anio = 2019, Color = "Azul", Kilometraje = 62000, ClienteId = 3 }
            };

            context.Vehiculos.AddRange(vehiculos);

            // Crear algunos empleados de ejemplo
            var empleados = new List<Empleado>
            {
                new() { Nombre = "Pedro", Apellido = "Martínez", DocumentoIdentidad = "EMP001", Telefono = "555-1111", TipoEmpleado = TipoEmpleado.Mecanico, Especialidad = "Motor", SalarioHora = 15.00m },
                new() { Nombre = "Ana", Apellido = "López", DocumentoIdentidad = "EMP002", Telefono = "555-2222", TipoEmpleado = TipoEmpleado.Recepcionista, SalarioHora = 12.00m },
                new() { Nombre = "Luis", Apellido = "García", DocumentoIdentidad = "EMP003", Telefono = "555-3333", TipoEmpleado = TipoEmpleado.Mecanico, Especialidad = "Frenos", SalarioHora = 16.00m }
            };

            context.Empleados.AddRange(empleados);
            await context.SaveChangesAsync();
        }
    }
}