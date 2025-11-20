using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using P_F.Services;

namespace P_F.Controllers;

public class UsuariosController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAuthService _authService;

    public UsuariosController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IAuthService authService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios = _userManager.Users.ToList();
        var usuariosConRoles = new List<dynamic>();

        foreach (var usuario in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            usuariosConRoles.Add(new
            {
                Id = usuario.Id,
                Email = usuario.Email,
                UserName = usuario.UserName,
                Roles = string.Join(", ", roles),
                EmailConfirmed = usuario.EmailConfirmed
            });
        }

        ViewBag.Roles = await _roleManager.Roles.ToListAsync();
        return View(usuariosConRoles);
    }

    [HttpPost]
    public async Task<IActionResult> CrearUsuario(string email, string password, string rol)
    {
        try
        {
            var usuario = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var resultado = await _userManager.CreateAsync(usuario, password);
            if (resultado.Succeeded)
            {
                if (!string.IsNullOrEmpty(rol))
                {
                    await _userManager.AddToRoleAsync(usuario, rol);
                }
                TempData["SuccessMessage"] = "Usuario creado exitosamente.";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(", ", resultado.Errors.Select(e => e.Description));
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al crear usuario: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AsignarRol(string userId, string rol)
    {
        try
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario != null)
            {
                // Remover roles existentes
                var rolesActuales = await _userManager.GetRolesAsync(usuario);
                await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);

                // Asignar nuevo rol
                await _userManager.AddToRoleAsync(usuario, rol);
                TempData["SuccessMessage"] = "Rol asignado exitosamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Usuario no encontrado.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al asignar rol: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> EliminarUsuario(string userId)
    {
        try
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario != null)
            {
                var resultado = await _userManager.DeleteAsync(usuario);
                if (resultado.Succeeded)
                {
                    TempData["SuccessMessage"] = "Usuario eliminado exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", resultado.Errors.Select(e => e.Description));
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Usuario no encontrado.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al eliminar usuario: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> InicializarSistema()
    {
        try
        {
            // Inicializar roles
            await _authService.InicializarRolesAsync();

            // Crear usuario administrador por defecto
            await _authService.CrearUsuarioAdminAsync("admin@taller.com", "Admin123!");

            TempData["SuccessMessage"] = "Sistema inicializado exitosamente. Usuario: admin@taller.com, Contraseña: Admin123!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error al inicializar sistema: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}