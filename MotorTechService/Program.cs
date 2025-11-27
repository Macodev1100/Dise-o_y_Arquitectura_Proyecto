using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotorTechService.Data;
using MotorTechService.Services;
using MotorTechService.Authorization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configuración de conexión a base de datos

var connectionString = builder.Configuration.GetConnectionString("SQLCadena");

// Entity Framework con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
    
    // Habilitar logging detallado en desarrollo
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    // Configuración de inicio de sesión
    options.SignIn.RequireConfirmedAccount = false; // Cambiar a 'true' para requerir verificación de email
    
    // Configuración de contraseñas
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;
    
    // Configuración de bloqueo de cuenta (opcional - descomentarlo para activar)
    // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    // options.Lockout.MaxFailedAccessAttempts = 5;
    // options.Lockout.AllowedForNewUsers = true;
    
    // Configuración de usuario (opcional)
    // options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configuración de Cookies de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    // Rutas de autenticación
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    
    // Configuración de cookies
    options.Cookie.Name = "TallerPyF.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    
    // Tiempo de expiración
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true; // Renueva automáticamente si el usuario está activo
});

// Políticas de Autorización
builder.Services.AddAuthorization(options =>
{
    // Políticas por rol específico
    options.AddPolicy(Policies.RequireAdministrador, 
        policy => policy.RequireRole(Roles.Administrador));
    
    options.AddPolicy(Policies.RequireSupervisor, 
        policy => policy.RequireRole(Roles.Supervisor));
    
    options.AddPolicy(Policies.RequireMecanico, 
        policy => policy.RequireRole(Roles.Mecanico));
    
    options.AddPolicy(Policies.RequireRecepcionista, 
        policy => policy.RequireRole(Roles.Recepcionista));

    // Políticas combinadas
    options.AddPolicy(Policies.RequireAdminOrSupervisor, 
        policy => policy.RequireRole(Roles.Administrador, Roles.Supervisor));
    
    options.AddPolicy(Policies.RequireAdminOrRecepcionista, 
        policy => policy.RequireRole(Roles.Administrador, Roles.Recepcionista));
    
    options.AddPolicy(Policies.RequireAnyEmployee, 
        policy => policy.RequireRole(Roles.Administrador, Roles.Supervisor, Roles.Mecanico, Roles.Recepcionista));

    // Políticas por módulo
    options.AddPolicy(Policies.CanManageClientes, 
        policy => policy.RequireRole(PermissionMatrix.CanManageClientes));
    
    options.AddPolicy(Policies.CanManageVehiculos, 
        policy => policy.RequireRole(PermissionMatrix.CanManageVehiculos));
    
    options.AddPolicy(Policies.CanManageOrdenes, 
        policy => policy.RequireRole(PermissionMatrix.CanManageOrdenes));
    
    options.AddPolicy(Policies.CanViewOrdenes, 
        policy => policy.RequireRole(PermissionMatrix.CanViewOrdenes));
    
    options.AddPolicy(Policies.CanManageInventario, 
        policy => policy.RequireRole(PermissionMatrix.CanManageInventario));
    
    options.AddPolicy(Policies.CanViewInventario, 
        policy => policy.RequireRole(PermissionMatrix.CanViewInventario));
    
    options.AddPolicy(Policies.CanManageEmpleados, 
        policy => policy.RequireRole(PermissionMatrix.CanManageEmpleados));
    
    options.AddPolicy(Policies.CanManageFacturas, 
        policy => policy.RequireRole(PermissionMatrix.CanManageFacturas));
    
    options.AddPolicy(Policies.CanViewReportes, 
        policy => policy.RequireRole(PermissionMatrix.CanViewReportes));
    
    options.AddPolicy(Policies.CanManageSystem, 
        policy => policy.RequireRole(PermissionMatrix.CanManageSystem));
});

// Controllers y Views
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// AutoMapper - Configuración de mapeo entre Entidades y DTOs
builder.Services.AddAutoMapper(typeof(Program));

// Repositorios - Capa de acceso a datos
builder.Services.AddScoped(typeof(MotorTechService.Repositories.IRepository<>), typeof(MotorTechService.Repositories.Repository<>) );
builder.Services.AddScoped<MotorTechService.Repositories.IClienteRepository, MotorTechService.Repositories.ClienteRepository>();
builder.Services.AddScoped<MotorTechService.Repositories.IVehiculoRepository, MotorTechService.Repositories.VehiculoRepository>();
builder.Services.AddScoped<MotorTechService.Repositories.IOrdenTrabajoRepository, MotorTechService.Repositories.OrdenTrabajoRepository>();
builder.Services.AddScoped<MotorTechService.Repositories.IEmpleadoRepository, MotorTechService.Repositories.EmpleadoRepository>();
builder.Services.AddScoped<MotorTechService.Repositories.IFacturaRepository, MotorTechService.Repositories.FacturaRepository>();
builder.Services.AddScoped<MotorTechService.Repositories.IRepuestoRepository, MotorTechService.Repositories.RepuestoRepository>();
builder.Services.AddScoped<MotorTechService.Repositories.IServicioRepository, MotorTechService.Repositories.ServicioRepository>();

// Servicios de negocio - Capa de lógica de negocio
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IVehiculoService, VehiculoService>();
builder.Services.AddScoped<IOrdenTrabajoService, OrdenTrabajoService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IFacturaService, FacturaService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Inicializar datos y roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await DbInitializer.Initialize(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar la base de datos");
    }
}

app.Run();

// --- [sig:MT1100-1125-26] ---
