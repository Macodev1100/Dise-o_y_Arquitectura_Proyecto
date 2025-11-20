using Microsoft.EntityFrameworkCore;
using P_F.Data;
using P_F.Models.Entities;

namespace P_F.Services
{
    public class ClienteService : IClienteService
    {
        private readonly ApplicationDbContext _context;

        public ClienteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync()
        {
            return await _context.Clientes
                .Where(c => c.Activo)
                .Include(c => c.Vehiculos)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Cliente?> GetByIdAsync(int id)
        {
            return await _context.Clientes
                .Include(c => c.Vehiculos)
                .Include(c => c.OrdenesTrabajo)
                .FirstOrDefaultAsync(c => c.ClienteId == id && c.Activo);
        }

        public async Task<Cliente> CreateAsync(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task<Cliente> UpdateAsync(Cliente cliente)
        {
            _context.Entry(cliente).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return false;

            cliente.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Clientes.AnyAsync(c => c.ClienteId == id && c.Activo);
        }

        public async Task<IEnumerable<Cliente>> SearchAsync(string searchTerm)
        {
            return await _context.Clientes
                .Where(c => c.Activo && 
                    (c.Nombre.Contains(searchTerm) || 
                     c.Apellido.Contains(searchTerm) || 
                     c.DocumentoIdentidad!.Contains(searchTerm) ||
                     c.Telefono.Contains(searchTerm)))
                .Include(c => c.Vehiculos)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Cliente?> GetByDocumentoAsync(string documento)
        {
            return await _context.Clientes
                .Include(c => c.Vehiculos)
                .FirstOrDefaultAsync(c => c.DocumentoIdentidad == documento && c.Activo);
        }
    }
}