using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using P_F.Models;
using P_F.Data;
using P_F.Models.Entities;

namespace P_F.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = await GenerarDashboardCompleto();
            return View(dashboard);
        }

        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await GenerarDashboardCompleto();
            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<DashboardViewModel> GenerarDashboardCompleto()
        {
            var hoy = DateTime.Now.Date;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            // Métricas principales
            var totalClientes = await _context.Clientes.CountAsync(c => c.Activo);
            var totalVehiculos = await _context.Vehiculos.CountAsync(v => v.Activo);
            var totalEmpleados = await _context.Empleados.CountAsync(e => e.Activo);
            
            // Órdenes de trabajo
            var ordenesHoy = await _context.OrdenesTrabajo
                .CountAsync(o => o.FechaIngreso.Date == hoy && o.Activo);
            
            var ordenesPendientes = await _context.OrdenesTrabajo
                .CountAsync(o => o.Estado == EstadoOrden.Pendiente && o.Activo);
            
            var ordenesEnProceso = await _context.OrdenesTrabajo
                .CountAsync(o => o.Estado == EstadoOrden.EnProceso && o.Activo);

            // Ventas del mes
            var ventasDelMes = await _context.Facturas
                .Where(f => f.FechaEmision >= inicioMes && f.Estado == EstadoFactura.Pagada)
                .SumAsync(f => f.Total);

            var ventasDelMesAnterior = await _context.Facturas
                .Where(f => f.FechaEmision >= inicioMes.AddMonths(-1) && f.FechaEmision < inicioMes && f.Estado == EstadoFactura.Pagada)
                .SumAsync(f => f.Total);

            // Inventario crítico
            var repuestosBajoStock = await _context.Repuestos
                .CountAsync(r => r.StockActual <= r.StockMinimo && r.Activo);

            // Datos para gráficos
            var ventasUltimos7Dias = await _context.Facturas
                .Where(f => f.FechaEmision >= hoy.AddDays(-7) && f.Estado == EstadoFactura.Pagada)
                .GroupBy(f => f.FechaEmision.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(f => f.Total) })
                .OrderBy(x => x.Fecha)
                .ToListAsync();

            var estadisticasEmpleados = await _context.Empleados
                .Where(e => e.Activo && e.TipoEmpleado == TipoEmpleado.Mecanico)
                .Select(e => new
                {
                    e.Nombre,
                    e.Apellido,
                    OrdenesActivas = e.OrdenesAsignadas.Count(o => o.Estado == EstadoOrden.EnProceso),
                    OrdenesCompletadas = e.OrdenesAsignadas.Count(o => o.Estado == EstadoOrden.Completada || o.Estado == EstadoOrden.Entregada)
                })
                .ToListAsync();

            var topRepuestos = await _context.Repuestos
                .Where(r => r.Activo)
                .OrderBy(r => r.StockActual)
                .Take(10)
                .Select(r => new { r.Nombre, r.StockActual, r.StockMinimo })
                .ToListAsync();

            return new DashboardViewModel
            {
                // Métricas principales
                TotalClientes = totalClientes,
                TotalVehiculos = totalVehiculos,
                TotalEmpleados = totalEmpleados,
                OrdenesHoy = ordenesHoy,
                OrdenesPendientes = ordenesPendientes,
                OrdenesEnProceso = ordenesEnProceso,
                
                // Ventas
                VentasDelMes = ventasDelMes,
                VentasDelMesAnterior = ventasDelMesAnterior,
                CrecimientoVentas = ventasDelMesAnterior > 0 ? ((ventasDelMes - ventasDelMesAnterior) / ventasDelMesAnterior) * 100 : 0,
                
                // Inventario
                RepuestosBajoStock = repuestosBajoStock,
                
                // Datos para gráficos
                VentasUltimos7Dias = ventasUltimos7Dias.Select(v => new VentaDiaria 
                { 
                    Fecha = v.Fecha, 
                    Total = v.Total 
                }).ToList(),
                
                EstadisticasEmpleados = estadisticasEmpleados.Select(e => new EstadisticaEmpleado
                {
                    Nombre = $"{e.Nombre} {e.Apellido}",
                    OrdenesActivas = e.OrdenesActivas,
                    OrdenesCompletadas = e.OrdenesCompletadas
                }).ToList(),
                
                TopRepuestosCriticos = topRepuestos.Select(r => new RepuestoCritico
                {
                    Nombre = r.Nombre,
                    StockActual = r.StockActual,
                    StockMinimo = r.StockMinimo
                }).ToList()
            };
        }
    }
}
