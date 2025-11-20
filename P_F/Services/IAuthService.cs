using Microsoft.AspNetCore.Identity;
using P_F.Models;

namespace P_F.Services;

public interface IAuthService
{
    Task<bool> InicializarRolesAsync();
    Task<bool> CrearUsuarioAdminAsync(string email, string password);
    Task<bool> AsignarRolAsync(string email, string rol);
    Task<List<string>> ObtenerRolesUsuarioAsync(string email);
    Task<bool> TienePermisoAsync(string email, string permiso);
}