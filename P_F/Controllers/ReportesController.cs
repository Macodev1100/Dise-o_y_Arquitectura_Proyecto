using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P_F.Data;
using P_F.Models;
using P_F.Models.Entities;
using P_F.Services;

namespace P_F.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;

        public ReportesController(ApplicationDbContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult VentasPorPeriodo()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VentasPorPeriodo(DateTime fechaInicio, DateTime fechaFin)
        {
            var ventas = _context.Facturas
                .Where(f => f.FechaEmision >= fechaInicio && f.FechaEmision <= fechaFin && f.Estado == EstadoFactura.Pagada)
                .GroupBy(f => f.FechaEmision.Date)
                .Select(g => new VentaDiaria
                {
                    Fecha = g.Key,
                    Total = g.Sum(f => f.Total)
                })
                .OrderBy(v => v.Fecha)
                .ToList();

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            ViewBag.TotalPeriodo = ventas.Sum(v => v.Total);
            
            return View("VentasResultado", ventas);
        }

        [HttpGet]
        public IActionResult InventarioCritico()
        {
            var repuestosCriticos = _context.Repuestos
                .Where(r => r.StockActual <= r.StockMinimo)
                .OrderBy(r => r.StockActual)
                .ToList();

            return View(repuestosCriticos);
        }

        [HttpGet]
        public IActionResult ProductividadEmpleados(DateTime? fechaInicio, DateTime? fechaFin)
        {
            fechaInicio ??= DateTime.Now.AddDays(-30);
            fechaFin ??= DateTime.Now;

            var empleados = _context.Empleados.ToList();
            var estadisticasEmpleados = new List<EstadisticaEmpleado>();

            foreach (var empleado in empleados)
            {
                var ordenesCompletadas = _context.OrdenesTrabajo
                    .Count(o => o.EmpleadoAsignadoId == empleado.EmpleadoId && 
                              o.Estado == EstadoOrden.Completada &&
                              o.FechaIngreso >= fechaInicio && 
                              o.FechaIngreso <= fechaFin);

                var ordenesActivas = _context.OrdenesTrabajo
                    .Count(o => o.EmpleadoAsignadoId == empleado.EmpleadoId && 
                              o.Estado == EstadoOrden.EnProceso);

                var ventasGeneradas = _context.OrdenesTrabajo
                    .Where(o => o.EmpleadoAsignadoId == empleado.EmpleadoId && 
                              o.Estado == EstadoOrden.Completada &&
                              o.FechaIngreso >= fechaInicio && 
                              o.FechaIngreso <= fechaFin)
                    .Sum(o => (decimal?)o.Total) ?? 0;

                estadisticasEmpleados.Add(new EstadisticaEmpleado
                {
                    Nombre = $"{empleado.Nombre} {empleado.Apellido}",
                    OrdenesCompletadas = ordenesCompletadas,
                    OrdenesActivas = ordenesActivas
                });
            }

            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;
            
            return View(estadisticasEmpleados);
        }

        [HttpGet]
        public IActionResult OrdenesPorEstado()
        {
            var estadisticas = new
            {
                Pendientes = _context.OrdenesTrabajo.Count(o => o.Estado == EstadoOrden.Pendiente),
                EnProceso = _context.OrdenesTrabajo.Count(o => o.Estado == EstadoOrden.EnProceso),
                Completadas = _context.OrdenesTrabajo.Count(o => o.Estado == EstadoOrden.Completada),
                Canceladas = _context.OrdenesTrabajo.Count(o => o.Estado == EstadoOrden.Cancelada)
            };

            return View(estadisticas);
        }

        [HttpGet]
        public IActionResult ClientesFrecuentes()
        {
            var clientesFrecuentes = _context.Clientes
                .Select(c => new
                {
                    Cliente = c,
                    TotalOrdenes = _context.OrdenesTrabajo.Count(o => o.ClienteId == c.ClienteId),
                    UltimaVisita = _context.OrdenesTrabajo
                        .Where(o => o.ClienteId == c.ClienteId)
                        .OrderByDescending(o => o.FechaIngreso)
                        .Select(o => o.FechaIngreso)
                        .FirstOrDefault(),
                    TotalGastado = _context.OrdenesTrabajo
                        .Where(o => o.ClienteId == c.ClienteId && o.Estado == EstadoOrden.Completada)
                        .Sum(o => (decimal?)o.Total) ?? 0
                })
                .Where(x => x.TotalOrdenes > 0)
                .OrderByDescending(x => x.TotalOrdenes)
                .Take(20)
                .ToList();

            return View(clientesFrecuentes);
        }

        // Métodos para generar PDFs
        [HttpPost]
        public async Task<IActionResult> VentasPdf(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerarReporteVentasPdfAsync(fechaInicio, fechaFin);
                return File(pdfBytes, "application/pdf", $"ReporteVentas_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al generar PDF: {ex.Message}";
                return RedirectToAction("VentasPorPeriodo");
            }
        }

        [HttpGet]
        public async Task<IActionResult> InventarioPdf()
        {
            try
            {
                var pdfBytes = await _pdfService.GenerarReporteInventarioPdfAsync();
                return File(pdfBytes, "application/pdf", $"ReporteInventario_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al generar PDF: {ex.Message}";
                return RedirectToAction("InventarioCritico");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EmpleadosPdf(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerarReporteEmpleadosPdfAsync(fechaInicio, fechaFin);
                return File(pdfBytes, "application/pdf", $"ReporteEmpleados_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al generar PDF: {ex.Message}";
                return RedirectToAction("ProductividadEmpleados");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ClientesFrecuentesPdf()
        {
            try
            {
                var pdfBytes = await _pdfService.GenerarReporteClientesFrecuentesPdfAsync();
                return File(pdfBytes, "application/pdf", $"ReporteClientesFrecuentes_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al generar PDF: {ex.Message}";
                return RedirectToAction("ClientesFrecuentes");
            }
        }
    }
}