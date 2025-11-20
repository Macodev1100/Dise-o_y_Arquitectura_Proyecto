using Microsoft.EntityFrameworkCore;
using P_F.Data;
using P_F.Models.Entities;

namespace P_F.Services
{
    public class OrdenTrabajoService : IOrdenTrabajoService
    {
        private readonly ApplicationDbContext _context;

        public OrdenTrabajoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrdenTrabajo>> GetAllAsync()
        {
            return await _context.OrdenesTrabajo
                .Include(o => o.Cliente)
                .Include(o => o.Vehiculo)
                .Include(o => o.EmpleadoAsignado)
                .OrderByDescending(o => o.FechaIngreso)
                .ToListAsync();
        }

        public async Task<OrdenTrabajo?> GetByIdAsync(int id)
        {
            return await _context.OrdenesTrabajo
                .Include(o => o.Cliente)
                .Include(o => o.Vehiculo)
                .Include(o => o.EmpleadoAsignado)
                .Include(o => o.EmpleadoRecepcion)
                .Include(o => o.Servicios)
                    .ThenInclude(s => s.Servicio)
                .Include(o => o.Repuestos)
                    .ThenInclude(r => r.Repuesto)
                .FirstOrDefaultAsync(o => o.OrdenTrabajoId == id);
        }

        public async Task<IEnumerable<OrdenTrabajo>> GetByEstadoAsync(EstadoOrden estado)
        {
            return await _context.OrdenesTrabajo
                .Where(o => o.Estado == estado)
                .Include(o => o.Cliente)
                .Include(o => o.Vehiculo)
                .Include(o => o.EmpleadoAsignado)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrdenTrabajo>> GetByEmpleadoAsync(int empleadoId)
        {
            return await _context.OrdenesTrabajo
                .Where(o => o.EmpleadoAsignadoId == empleadoId)
                .Include(o => o.Cliente)
                .Include(o => o.Vehiculo)
                .ToListAsync();
        }

        public async Task<OrdenTrabajo> CreateAsync(OrdenTrabajo orden)
        {
            orden.NumeroOrden = await GenerarNumeroOrdenAsync();
            _context.OrdenesTrabajo.Add(orden);
            await _context.SaveChangesAsync();
            return orden;
        }

        public async Task<OrdenTrabajo> UpdateAsync(OrdenTrabajo orden)
        {
            _context.Entry(orden).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return orden;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var orden = await _context.OrdenesTrabajo.FindAsync(id);
            if (orden == null) return false;

            orden.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CambiarEstadoAsync(int id, EstadoOrden nuevoEstado, int empleadoId, string? observaciones = null)
        {
            var orden = await _context.OrdenesTrabajo.FindAsync(id);
            if (orden == null) return false;

            var estadoAnterior = orden.Estado;
            orden.Estado = nuevoEstado;

            if (nuevoEstado == EstadoOrden.Entregada)
                orden.FechaEntrega = DateTime.Now;

            // Registrar cambio en historial
            var historial = new HistorialOrden
            {
                OrdenTrabajoId = id,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = nuevoEstado,
                EmpleadoId = empleadoId,
                Observaciones = observaciones
            };

            _context.HistorialOrdenes.Add(historial);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerarNumeroOrdenAsync()
        {
            var ultimaOrden = await _context.OrdenesTrabajo
                .OrderByDescending(o => o.OrdenTrabajoId)
                .FirstOrDefaultAsync();

            var numero = (ultimaOrden?.OrdenTrabajoId ?? 0) + 1;
            return $"ORD-{DateTime.Now:yyyyMM}-{numero:D6}";
        }

        public async Task<decimal> CalcularTotalAsync(int ordenId)
        {
            var servicios = await _context.OrdenTrabajoServicios
                .Where(s => s.OrdenTrabajoId == ordenId)
                .SumAsync(s => s.Precio * s.Cantidad * (1 - s.Descuento / 100));

            var repuestos = await _context.OrdenTrabajoRepuestos
                .Where(r => r.OrdenTrabajoId == ordenId)
                .SumAsync(r => r.PrecioUnitario * r.Cantidad * (1 - r.Descuento / 100));

            return servicios + repuestos;
        }
    }

    public class EmpleadoService : IEmpleadoService
    {
        private readonly ApplicationDbContext _context;

        public EmpleadoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Empleado>> GetAllAsync()
        {
            return await _context.Empleados
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .ToListAsync();
        }

        public async Task<Empleado?> GetByIdAsync(int id)
        {
            return await _context.Empleados
                .Include(e => e.OrdenesAsignadas)
                .Include(e => e.RegistrosTiempo)
                .FirstOrDefaultAsync(e => e.EmpleadoId == id && e.Activo);
        }

        public async Task<IEnumerable<Empleado>> GetByTipoAsync(TipoEmpleado tipo)
        {
            return await _context.Empleados
                .Where(e => e.TipoEmpleado == tipo && e.Activo)
                .ToListAsync();
        }

        public async Task<Empleado> CreateAsync(Empleado empleado)
        {
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();
            return empleado;
        }

        public async Task<Empleado> UpdateAsync(Empleado empleado)
        {
            _context.Entry(empleado).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return empleado;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return false;

            empleado.Activo = false;
            empleado.FechaTerminacion = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Empleados.AnyAsync(e => e.EmpleadoId == id && e.Activo);
        }

        public async Task<Empleado?> GetByUserIdAsync(string userId)
        {
            return await _context.Empleados
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Activo);
        }
    }

    // Servicios adicionales implementados de forma básica
    public class ServicioService : IServicioService
    {
        private readonly ApplicationDbContext _context;

        public ServicioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Servicio>> GetAllAsync() =>
            await _context.Servicios.Include(s => s.CategoriaServicio).ToListAsync();

        public async Task<Servicio?> GetByIdAsync(int id) =>
            await _context.Servicios.Include(s => s.CategoriaServicio).FirstOrDefaultAsync(s => s.ServicioId == id);

        public async Task<IEnumerable<Servicio>> GetByCategoriaAsync(int categoriaId) =>
            await _context.Servicios.Where(s => s.CategoriaServicioId == categoriaId).ToListAsync();

        public async Task<Servicio> CreateAsync(Servicio servicio)
        {
            _context.Servicios.Add(servicio);
            await _context.SaveChangesAsync();
            return servicio;
        }

        public async Task<Servicio> UpdateAsync(Servicio servicio)
        {
            _context.Entry(servicio).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return servicio;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var servicio = await _context.Servicios.FindAsync(id);
            if (servicio == null) return false;
            servicio.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CategoriaServicio>> GetCategoriasAsync() =>
            await _context.CategoriasServicio.ToListAsync();
    }

    public class InventarioService : IInventarioService
    {
        private readonly ApplicationDbContext _context;

        public InventarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Repuesto>> GetAllRepuestosAsync() =>
            await _context.Repuestos.Include(r => r.CategoriaRepuesto).ToListAsync();

        public async Task<Repuesto?> GetRepuestoByIdAsync(int id) =>
            await _context.Repuestos.Include(r => r.CategoriaRepuesto).FirstOrDefaultAsync(r => r.RepuestoId == id);

        public async Task<Repuesto> CreateRepuestoAsync(Repuesto repuesto)
        {
            _context.Repuestos.Add(repuesto);
            await _context.SaveChangesAsync();
            return repuesto;
        }

        public async Task<Repuesto> UpdateRepuestoAsync(Repuesto repuesto)
        {
            _context.Entry(repuesto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return repuesto;
        }

        public async Task<bool> DeleteRepuestoAsync(int id)
        {
            var repuesto = await _context.Repuestos.FindAsync(id);
            if (repuesto == null) return false;
            repuesto.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegistrarMovimientoAsync(MovimientoInventario movimiento)
        {
            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Repuesto>> GetRepuestosBajoStockAsync() =>
            await _context.Repuestos.Where(r => r.StockActual <= r.StockMinimo && r.Activo).ToListAsync();

        public async Task<bool> ActualizarStockAsync(int repuestoId, int cantidad, TipoMovimiento tipo, string motivo, int empleadoId)
        {
            var repuesto = await _context.Repuestos.FindAsync(repuestoId);
            if (repuesto == null) return false;

            var stockAnterior = repuesto.StockActual;
            var stockNuevo = tipo == TipoMovimiento.Entrada ? stockAnterior + cantidad : stockAnterior - cantidad;

            repuesto.StockActual = stockNuevo;

            var movimiento = new MovimientoInventario
            {
                RepuestoId = repuestoId,
                TipoMovimiento = tipo,
                Cantidad = cantidad,
                StockAnterior = stockAnterior,
                StockNuevo = stockNuevo,
                Motivo = motivo,
                EmpleadoId = empleadoId
            };

            _context.MovimientosInventario.Add(movimiento);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class FacturaService : IFacturaService
    {
        private readonly ApplicationDbContext _context;

        public FacturaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Factura>> GetAllAsync() =>
            await _context.Facturas.Include(f => f.Cliente).Include(f => f.OrdenTrabajo).ToListAsync();

        public async Task<Factura?> GetByIdAsync(int id) =>
            await _context.Facturas.Include(f => f.Cliente).Include(f => f.Pagos).FirstOrDefaultAsync(f => f.FacturaId == id);

        public async Task<Factura> CreateAsync(Factura factura)
        {
            factura.NumeroFactura = await GenerarNumeroFacturaAsync();
            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();
            return factura;
        }

        public async Task<Factura> UpdateAsync(Factura factura)
        {
            _context.Entry(factura).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return factura;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null) return false;
            factura.Estado = EstadoFactura.Anulada;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerarNumeroFacturaAsync()
        {
            var ultimaFactura = await _context.Facturas.OrderByDescending(f => f.FacturaId).FirstOrDefaultAsync();
            var numero = (ultimaFactura?.FacturaId ?? 0) + 1;
            return $"FAC-{DateTime.Now:yyyyMM}-{numero:D6}";
        }

        public async Task<bool> RegistrarPagoAsync(Pago pago)
        {
            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetSaldoPendienteAsync(int facturaId)
        {
            var factura = await _context.Facturas.FindAsync(facturaId);
            var pagos = await _context.Pagos.Where(p => p.FacturaId == facturaId).SumAsync(p => p.Monto);
            return (factura?.Total ?? 0) - pagos;
        }
    }

    public class ReporteService : IReporteService
    {
        private readonly ApplicationDbContext _context;

        public ReporteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerarReporteVentasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            // Implementación básica - en producción usaríamos iTextSharp o similar
            return await Task.FromResult(new byte[0]);
        }

        public async Task<byte[]> GenerarReporteInventarioAsync()
        {
            return await Task.FromResult(new byte[0]);
        }

        public async Task<byte[]> GenerarReporteProductividadAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await Task.FromResult(new byte[0]);
        }

        public async Task<object> GetDashboardDataAsync()
        {
            var ordenesPendientes = await _context.OrdenesTrabajo.CountAsync(o => o.Estado == EstadoOrden.Pendiente);
            var ordenesEnProceso = await _context.OrdenesTrabajo.CountAsync(o => o.Estado == EstadoOrden.EnProceso);
            var ventasHoy = await _context.Facturas.Where(f => f.FechaEmision.Date == DateTime.Today).SumAsync(f => f.Total);
            var clientesTotal = await _context.Clientes.CountAsync(c => c.Activo);

            return new
            {
                OrdenesPendientes = ordenesPendientes,
                OrdenesEnProceso = ordenesEnProceso,
                VentasHoy = ventasHoy,
                ClientesTotal = clientesTotal
            };
        }

        public async Task<IEnumerable<object>> GetVentasPorMesAsync(int año)
        {
            return await _context.Facturas
                .Where(f => f.FechaEmision.Year == año)
                .GroupBy(f => f.FechaEmision.Month)
                .Select(g => new { Mes = g.Key, Total = g.Sum(f => f.Total) })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetServiciosMasVendidosAsync(int top = 10)
        {
            return await _context.OrdenTrabajoServicios
                .Include(o => o.Servicio)
                .GroupBy(o => o.Servicio.Nombre)
                .Select(g => new { Servicio = g.Key, Cantidad = g.Sum(o => o.Cantidad) })
                .OrderByDescending(x => x.Cantidad)
                .Take(top)
                .ToListAsync();
        }
    }

    public class NotificacionService : INotificacionService
    {
        private readonly ApplicationDbContext _context;

        public NotificacionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task EnviarNotificacionAsync(string mensaje, string tipo, string? userId = null)
        {
            // Implementación básica - en producción integraríamos SignalR
            await Task.CompletedTask;
        }

        public async Task NotificarOrdenCompletadaAsync(int ordenId)
        {
            await EnviarNotificacionAsync($"Orden #{ordenId} completada", "success");
        }

        public async Task NotificarStockBajoAsync(int repuestoId)
        {
            var repuesto = await _context.Repuestos.FindAsync(repuestoId);
            if (repuesto != null)
                await EnviarNotificacionAsync($"Stock bajo: {repuesto.Nombre}", "warning");
        }

        public async Task NotificarVencimientoOrdenAsync(int ordenId)
        {
            await EnviarNotificacionAsync($"Orden #{ordenId} próxima a vencer", "info");
        }

        public async Task EnviarEmailAsync(string destinatario, string asunto, string mensaje)
        {
            // Implementación básica - integrar con servicio de email
            await Task.CompletedTask;
        }
    }
}