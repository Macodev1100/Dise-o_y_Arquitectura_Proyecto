using P_F.Models.Entities;

namespace P_F.Services
{
    public interface IClienteService
    {
        Task<IEnumerable<Cliente>> GetAllAsync();
        Task<Cliente?> GetByIdAsync(int id);
        Task<Cliente> CreateAsync(Cliente cliente);
        Task<Cliente> UpdateAsync(Cliente cliente);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Cliente>> SearchAsync(string searchTerm);
        Task<Cliente?> GetByDocumentoAsync(string documento);
    }

    public interface IVehiculoService
    {
        Task<IEnumerable<Vehiculo>> GetAllAsync();
        Task<Vehiculo?> GetByIdAsync(int id);
        Task<IEnumerable<Vehiculo>> GetByClienteIdAsync(int clienteId);
        Task<Vehiculo> CreateAsync(Vehiculo vehiculo);
        Task<Vehiculo> UpdateAsync(Vehiculo vehiculo);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<Vehiculo?> GetByPlacaAsync(string placa);
    }

    public interface IOrdenTrabajoService
    {
        Task<IEnumerable<OrdenTrabajo>> GetAllAsync();
        Task<OrdenTrabajo?> GetByIdAsync(int id);
        Task<IEnumerable<OrdenTrabajo>> GetByEstadoAsync(EstadoOrden estado);
        Task<IEnumerable<OrdenTrabajo>> GetByEmpleadoAsync(int empleadoId);
        Task<OrdenTrabajo> CreateAsync(OrdenTrabajo orden);
        Task<OrdenTrabajo> UpdateAsync(OrdenTrabajo orden);
        Task<bool> DeleteAsync(int id);
        Task<bool> CambiarEstadoAsync(int id, EstadoOrden nuevoEstado, int empleadoId, string? observaciones = null);
        Task<string> GenerarNumeroOrdenAsync();
        Task<decimal> CalcularTotalAsync(int ordenId);
    }

    public interface IEmpleadoService
    {
        Task<IEnumerable<Empleado>> GetAllAsync();
        Task<Empleado?> GetByIdAsync(int id);
        Task<IEnumerable<Empleado>> GetByTipoAsync(TipoEmpleado tipo);
        Task<Empleado> CreateAsync(Empleado empleado);
        Task<Empleado> UpdateAsync(Empleado empleado);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<Empleado?> GetByUserIdAsync(string userId);
    }

    public interface IServicioService
    {
        Task<IEnumerable<Servicio>> GetAllAsync();
        Task<Servicio?> GetByIdAsync(int id);
        Task<IEnumerable<Servicio>> GetByCategoriaAsync(int categoriaId);
        Task<Servicio> CreateAsync(Servicio servicio);
        Task<Servicio> UpdateAsync(Servicio servicio);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<CategoriaServicio>> GetCategoriasAsync();
    }

    public interface IInventarioService
    {
        Task<IEnumerable<Repuesto>> GetAllRepuestosAsync();
        Task<Repuesto?> GetRepuestoByIdAsync(int id);
        Task<Repuesto> CreateRepuestoAsync(Repuesto repuesto);
        Task<Repuesto> UpdateRepuestoAsync(Repuesto repuesto);
        Task<bool> DeleteRepuestoAsync(int id);
        Task<bool> RegistrarMovimientoAsync(MovimientoInventario movimiento);
        Task<IEnumerable<Repuesto>> GetRepuestosBajoStockAsync();
        Task<bool> ActualizarStockAsync(int repuestoId, int cantidad, TipoMovimiento tipo, string motivo, int empleadoId);
    }

    public interface IFacturaService
    {
        Task<IEnumerable<Factura>> GetAllAsync();
        Task<Factura?> GetByIdAsync(int id);
        Task<Factura> CreateAsync(Factura factura);
        Task<Factura> UpdateAsync(Factura factura);
        Task<bool> DeleteAsync(int id);
        Task<string> GenerarNumeroFacturaAsync();
        Task<bool> RegistrarPagoAsync(Pago pago);
        Task<decimal> GetSaldoPendienteAsync(int facturaId);
    }

    public interface IReporteService
    {
        Task<byte[]> GenerarReporteVentasAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<byte[]> GenerarReporteInventarioAsync();
        Task<byte[]> GenerarReporteProductividadAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<object> GetDashboardDataAsync();
        Task<IEnumerable<object>> GetVentasPorMesAsync(int año);
        Task<IEnumerable<object>> GetServiciosMasVendidosAsync(int top = 10);
    }

    public interface INotificacionService
    {
        Task EnviarNotificacionAsync(string mensaje, string tipo, string? userId = null);
        Task NotificarOrdenCompletadaAsync(int ordenId);
        Task NotificarStockBajoAsync(int repuestoId);
        Task NotificarVencimientoOrdenAsync(int ordenId);
        Task EnviarEmailAsync(string destinatario, string asunto, string mensaje);
    }
}