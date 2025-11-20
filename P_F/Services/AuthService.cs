using Microsoft.AspNetCore.Identity;
using P_F.Services;
using System.Data;

namespace P_F.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> InicializarRolesAsync()
    {
        try
        {
            var roles = new[] { "Administrador", "Gerente", "Mecanico", "Recepcionista" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var resultado = await _roleManager.CreateAsync(new IdentityRole(role));
                    if (!resultado.Succeeded) return false;
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CrearUsuarioAdminAsync(string email, string password)
    {
        try
        {
            var usuario = await _userManager.FindByEmailAsync(email);
            if (usuario != null) return true; // Ya existe

            usuario = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var resultado = await _userManager.CreateAsync(usuario, password);
            if (resultado.Succeeded)
            {
                return await AsignarRolAsync(email, "Administrador");
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AsignarRolAsync(string email, string rol)
    {
        try
        {
            var usuario = await _userManager.FindByEmailAsync(email);
            if (usuario == null) return false;

            if (!await _userManager.IsInRoleAsync(usuario, rol))
            {
                var resultado = await _userManager.AddToRoleAsync(usuario, rol);
                return resultado.Succeeded;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<string>> ObtenerRolesUsuarioAsync(string email)
    {
        try
        {
            var usuario = await _userManager.FindByEmailAsync(email);
            if (usuario == null) return new List<string>();

            var roles = await _userManager.GetRolesAsync(usuario);
            return roles.ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<bool> TienePermisoAsync(string email, string permiso)
    {
        try
        {
            var roles = await ObtenerRolesUsuarioAsync(email);
            
            // Definir permisos por rol
            var permisos = new Dictionary<string, List<string>>
            {
                ["Administrador"] = new() { "todos", "usuarios", "reportes", "facturacion", "inventario", "ordenes", "vehiculos", "empleados" },
                ["Gerente"] = new() { "reportes", "facturacion", "inventario", "ordenes", "vehiculos", "empleados" },
                ["Mecanico"] = new() { "ordenes", "vehiculos", "inventario_lectura" },
                ["Recepcionista"] = new() { "ordenes", "vehiculos", "facturacion_lectura" }
            };

            foreach (var rol in roles)
            {
                if (permisos.ContainsKey(rol))
                {
                    if (permisos[rol].Contains("todos") || permisos[rol].Contains(permiso))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}