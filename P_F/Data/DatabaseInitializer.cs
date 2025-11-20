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
            try
            {
                // Verificar conexión a la base de datos
                Console.WriteLine("Verificando conexión a la base de datos...");
                var canConnect = await context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    Console.WriteLine("No se puede conectar a la base de datos. Verificando configuración...");
                    throw new Exception("No se puede establecer conexión con la base de datos SQL Server.");
                }

                Console.WriteLine("Conexión exitosa. Creando base de datos si es necesario...");
                
                // Crear la base de datos si no existe
                await context.Database.EnsureCreatedAsync();
                
                // Aplicar migraciones pendientes si existen
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    Console.WriteLine($"Aplicando {pendingMigrations.Count()} migraciones pendientes...");
                    await context.Database.MigrateAsync();
                }

                // Crear roles del sistema
                await CreateRoles(roleManager);

                // Crear usuario administrador por defecto
                await CreateDefaultAdmin(userManager, context);

                // Crear datos semilla adicionales
                await SeedAdditionalData(context);

                Console.WriteLine("Inicialización de base de datos completada exitosamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante la inicialización: {ex.Message}");
                
                // Información adicional para debugging
                Console.WriteLine("Cadena de conexión utilizada:");
                Console.WriteLine(context.Database.GetConnectionString());
                
                throw;
            }
        }

        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            Console.WriteLine("Creando roles del sistema...");
            
            string[] roles = { "Administrador", "Mecanico", "Recepcionista", "Supervisor" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"Rol '{role}' creado exitosamente.");
                    }
                    else
                    {
                        Console.WriteLine($"Error al crear rol '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private static async Task CreateDefaultAdmin(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            Console.WriteLine("Creando usuario administrador por defecto...");
            
            var adminEmail = "admin@tallerpyf.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrador");
                    Console.WriteLine("Usuario administrador creado exitosamente.");
                    Console.WriteLine($"Email: {adminEmail}");
                    Console.WriteLine("Password: Admin123!");

                    // Crear empleado asociado al usuario
                    var empleadoAdmin = new Empleado
                    {
                        Nombre = "Administrador",
                        Apellido = "Sistema",
                        DocumentoIdentidad = "00000000",
                        Telefono = "555-0000",
                        Email = adminEmail,
                        TipoEmpleado = TipoEmpleado.Administrador,
                        SalarioHora = 0,
                        PorcentajeComision = 0,
                        Activo = true,
                        UserId = adminUser.Id
                    };

                    context.Empleados.Add(empleadoAdmin);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Empleado administrador asociado creado.");
                }
                else
                {
                    Console.WriteLine($"Error al crear usuario administrador: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine("Usuario administrador ya existe.");
            }
        }

        private static async Task SeedAdditionalData(ApplicationDbContext context)
        {
            // Verificar si ya existen datos
            if (await context.Clientes.AnyAsync())
            {
                Console.WriteLine("La base de datos ya contiene datos. Omitiendo creación de datos de ejemplo.");
                return;
            }

            Console.WriteLine("Creando datos de ejemplo...");

            // Crear algunos clientes de ejemplo
            var clientesEjemplo = new List<Cliente>
            {
                new Cliente
                {
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    DocumentoIdentidad = "12345678",
                    Telefono = "555-1234",
                    Email = "juan.perez@email.com",
                    Direccion = "Av. Principal 123"
                },
                new Cliente
                {
                    Nombre = "María",
                    Apellido = "García",
                    DocumentoIdentidad = "87654321",
                    Telefono = "555-5678",
                    Email = "maria.garcia@email.com",
                    Direccion = "Calle Secundaria 456"
                },
                new Cliente
                {
                    Nombre = "Pedro",
                    Apellido = "Martínez",
                    DocumentoIdentidad = "11223344",
                    Telefono = "555-9999",
                    Email = "pedro.martinez@email.com",
                    Direccion = "Carrera 10 #20-30"
                }
            };

            context.Clientes.AddRange(clientesEjemplo);
            await context.SaveChangesAsync();

            // Crear algunos vehículos de ejemplo
            var vehiculosEjemplo = new List<Vehiculo>
            {
                new Vehiculo
                {
                    Placa = "ABC123",
                    Marca = "Toyota",
                    Modelo = "Corolla",
                    Anio = 2020,
                    Color = "Blanco",
                    TipoCombustible = "Gasolina",
                    Transmision = "Manual",
                    Kilometraje = 45000,
                    ClienteId = clientesEjemplo[0].ClienteId
                },
                new Vehiculo
                {
                    Placa = "XYZ789",
                    Marca = "Honda",
                    Modelo = "Civic",
                    Anio = 2019,
                    Color = "Azul",
                    TipoCombustible = "Gasolina",
                    Transmision = "Automática",
                    Kilometraje = 62000,
                    ClienteId = clientesEjemplo[1].ClienteId
                },
                new Vehiculo
                {
                    Placa = "DEF456",
                    Marca = "Chevrolet",
                    Modelo = "Spark",
                    Anio = 2021,
                    Color = "Rojo",
                    TipoCombustible = "Gasolina",
                    Transmision = "Manual",
                    Kilometraje = 25000,
                    ClienteId = clientesEjemplo[2].ClienteId
                }
            };

            context.Vehiculos.AddRange(vehiculosEjemplo);
            await context.SaveChangesAsync();

            // Crear algunos repuestos de ejemplo
            var repuestosEjemplo = new List<Repuesto>
            {
                new Repuesto
                {
                    Codigo = "FIL001",
                    Nombre = "Filtro de Aceite",
                    Descripcion = "Filtro de aceite universal",
                    CategoriaRepuestoId = 1,
                    Marca = "Mann",
                    PrecioCosto = 8.50m,
                    PrecioVenta = 15.00m,
                    PorcentajeGanancia = 76.47m,
                    StockMinimo = 10,
                    StockMaximo = 50,
                    StockActual = 25,
                    Ubicacion = "Estante A-1",
                    Proveedor = "Repuestos Central"
                },
                new Repuesto
                {
                    Codigo = "ACE001",
                    Nombre = "Aceite Motor 5W-30",
                    Descripcion = "Aceite sintético para motor",
                    CategoriaRepuestoId = 2,
                    Marca = "Mobil 1",
                    PrecioCosto = 25.00m,
                    PrecioVenta = 40.00m,
                    PorcentajeGanancia = 60.00m,
                    StockMinimo = 20,
                    StockMaximo = 100,
                    StockActual = 60,
                    Ubicacion = "Bodega B-2",
                    Proveedor = "Lubricantes del Sur"
                },
                new Repuesto
                {
                    Codigo = "PAS001",
                    Nombre = "Pastillas Freno Delanteras",
                    Descripcion = "Pastillas de freno para llanta delantera",
                    CategoriaRepuestoId = 3,
                    Marca = "Brembo",
                    PrecioCosto = 45.00m,
                    PrecioVenta = 75.00m,
                    PorcentajeGanancia = 66.67m,
                    StockMinimo = 8,
                    StockMaximo = 30,
                    StockActual = 15,
                    Ubicacion = "Estante C-3",
                    Proveedor = "Frenos y Más"
                }
            };

            context.Repuestos.AddRange(repuestosEjemplo);
            await context.SaveChangesAsync();

            // Crear algunos empleados de ejemplo
            var empleadosEjemplo = new List<Empleado>
            {
                new Empleado
                {
                    Nombre = "Carlos",
                    Apellido = "Mendoza",
                    DocumentoIdentidad = "11111111",
                    Telefono = "555-1111",
                    Email = "carlos.mendoza@tallerpyf.com",
                    Direccion = "Calle 15 #8-25",
                    TipoEmpleado = TipoEmpleado.Mecanico,
                    Especialidad = "Motor y Transmisión",
                    SalarioHora = 15.00m,
                    PorcentajeComision = 5.00m
                },
                new Empleado
                {
                    Nombre = "Ana",
                    Apellido = "López",
                    DocumentoIdentidad = "22222222",
                    Telefono = "555-2222",
                    Email = "ana.lopez@tallerpyf.com",
                    Direccion = "Avenida 20 #12-34",
                    TipoEmpleado = TipoEmpleado.Recepcionista,
                    SalarioHora = 12.00m,
                    PorcentajeComision = 2.00m
                },
                new Empleado
                {
                    Nombre = "Luis",
                    Apellido = "Hernández",
                    DocumentoIdentidad = "33333333",
                    Telefono = "555-3333",
                    Email = "luis.hernandez@tallerpyf.com",
                    Direccion = "Carrera 5 #18-90",
                    TipoEmpleado = TipoEmpleado.Supervisor,
                    Especialidad = "Supervisor General",
                    SalarioHora = 18.00m,
                    PorcentajeComision = 3.00m
                }
            };

            context.Empleados.AddRange(empleadosEjemplo);
            await context.SaveChangesAsync();

            Console.WriteLine("Datos de ejemplo creados exitosamente:");
            Console.WriteLine($"- {clientesEjemplo.Count} clientes");
            Console.WriteLine($"- {vehiculosEjemplo.Count} vehículos");
            Console.WriteLine($"- {repuestosEjemplo.Count} repuestos");
            Console.WriteLine($"- {empleadosEjemplo.Count} empleados");
        }
    }
}