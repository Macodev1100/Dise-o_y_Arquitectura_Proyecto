using Microsoft.EntityFrameworkCore;
using P_F.Data;
using P_F.Models.Entities;

namespace P_F.Services
{
    public class VehiculoService : IVehiculoService
    {
        private readonly ApplicationDbContext _context;

        public VehiculoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehiculo>> GetAllAsync()
        {
            return await _context.Vehiculos
                .Where(v => v.Activo)
                .Include(v => v.Cliente)
                .OrderBy(v => v.Placa)
                .ToListAsync();
        }

        public async Task<Vehiculo?> GetByIdAsync(int id)
        {
            return await _context.Vehiculos
                .Include(v => v.Cliente)
                .Include(v => v.OrdenesTrabajo)
                .Include(v => v.HistorialMantenimientos)
                .FirstOrDefaultAsync(v => v.VehiculoId == id && v.Activo);
        }

        public async Task<IEnumerable<Vehiculo>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.Vehiculos
                .Where(v => v.ClienteId == clienteId && v.Activo)
                .Include(v => v.Cliente)
                .OrderBy(v => v.Placa)
                .ToListAsync();
        }

        public async Task<Vehiculo> CreateAsync(Vehiculo vehiculo)
        {
            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();
            return vehiculo;
        }

        public async Task<Vehiculo> UpdateAsync(Vehiculo vehiculo)
        {
            _context.Entry(vehiculo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return vehiculo;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null) return false;

            vehiculo.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Vehiculos.AnyAsync(v => v.VehiculoId == id && v.Activo);
        }

        public async Task<Vehiculo?> GetByPlacaAsync(string placa)
        {
            return await _context.Vehiculos
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Placa == placa && v.Activo);
        }
    }
}