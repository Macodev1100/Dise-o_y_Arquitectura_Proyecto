using P_F.Models.Entities;

namespace P_F.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerarFacturaPdfAsync(Factura factura);
        Task<byte[]> GenerarOrdenTrabajoPdfAsync(OrdenTrabajo orden);
        Task<byte[]> GenerarReporteVentasPdfAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<byte[]> GenerarReporteInventarioPdfAsync();
        Task<byte[]> GenerarReporteEmpleadosPdfAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<byte[]> GenerarReporteClientesFrecuentesPdfAsync();
    }
}