using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P_F.Data;
using P_F.Models.Entities;
using P_F.Services;

namespace P_F.Controllers
{
    public class FacturasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;

        public FacturasController(ApplicationDbContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        // GET: Facturas
        public async Task<IActionResult> Index(string searchString, EstadoFactura? estado, DateTime? fechaInicio, DateTime? fechaFin)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["EstadoFilter"] = estado;
            ViewData["FechaInicioFilter"] = fechaInicio;
            ViewData["FechaFinFilter"] = fechaFin;

            var facturasQuery = _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.OrdenTrabajo)
                    .ThenInclude(o => o.Vehiculo)
                .Include(f => f.Pagos)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                facturasQuery = facturasQuery.Where(f =>
                    f.NumeroFactura.Contains(searchString) ||
                    f.Cliente.Nombre.Contains(searchString) ||
                    f.Cliente.Apellido.Contains(searchString) ||
                    f.OrdenTrabajo.NumeroOrden.Contains(searchString));
            }

            if (estado.HasValue)
            {
                facturasQuery = facturasQuery.Where(f => f.Estado == estado.Value);
            }

            if (fechaInicio.HasValue)
            {
                facturasQuery = facturasQuery.Where(f => f.FechaEmision >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                facturasQuery = facturasQuery.Where(f => f.FechaEmision <= fechaFin.Value);
            }

            var facturas = await facturasQuery
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync();

            return View(facturas);
        }

        // GET: Facturas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var factura = await _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.OrdenTrabajo)
                    .ThenInclude(o => o.Vehiculo)
                .Include(f => f.OrdenTrabajo)
                    .ThenInclude(o => o.EmpleadoAsignado)
                .Include(f => f.Pagos)
                    .ThenInclude(p => p.Empleado)
                .FirstOrDefaultAsync(f => f.FacturaId == id);

            if (factura == null) return NotFound();

            return View(factura);
        }

        // GET: Facturas/Create
        public async Task<IActionResult> Create(int? ordenTrabajoId)
        {
            if (ordenTrabajoId.HasValue)
            {
                var orden = await _context.OrdenesTrabajo
                    .Include(o => o.Cliente)
                    .Include(o => o.Vehiculo)
                    .FirstOrDefaultAsync(o => o.OrdenTrabajoId == ordenTrabajoId);

                if (orden == null) return NotFound();

                // Verificar si ya existe factura para esta orden
                var facturaExiste = await _context.Facturas
                    .AnyAsync(f => f.OrdenTrabajoId == ordenTrabajoId);

                if (facturaExiste)
                {
                    TempData["Error"] = "Esta orden de trabajo ya tiene una factura generada.";
                    return RedirectToAction("Details", "OrdenesTrabajo", new { id = ordenTrabajoId });
                }

                var factura = new Factura
                {
                    OrdenTrabajoId = orden.OrdenTrabajoId,
                    ClienteId = orden.ClienteId,
                    NumeroFactura = await GenerarNumeroFactura(),
                    FechaEmision = DateTime.Now,
                    SubTotal = orden.SubTotal,
                    Impuestos = orden.Impuestos,
                    Descuento = orden.Descuento,
                    Total = orden.Total,
                    Estado = EstadoFactura.Borrador
                };

                ViewBag.OrdenTrabajo = orden;
                return View(factura);
            }

            ViewBag.OrdenesDisponibles = await _context.OrdenesTrabajo
                .Include(o => o.Cliente)
                .Include(o => o.Vehiculo)
                .Where(o => o.Estado == EstadoOrden.Completada && !_context.Facturas.Any(f => f.OrdenTrabajoId == o.OrdenTrabajoId))
                .ToListAsync();

            return View(new Factura { FechaEmision = DateTime.Now });
        }

        // POST: Facturas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrdenTrabajoId,ClienteId,SubTotal,Impuestos,Descuento,Total,Observaciones")] Factura factura)
        {
            if (ModelState.IsValid)
            {
                factura.NumeroFactura = await GenerarNumeroFactura();
                factura.FechaEmision = DateTime.Now;
                factura.Estado = EstadoFactura.Emitida;

                _context.Add(factura);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Factura {factura.NumeroFactura} generada exitosamente.";
                return RedirectToAction(nameof(Details), new { id = factura.FacturaId });
            }

            var orden = await _context.OrdenesTrabajo
                .Include(o => o.Cliente)
                .Include(o => o.Vehiculo)
                .FirstOrDefaultAsync(o => o.OrdenTrabajoId == factura.OrdenTrabajoId);

            ViewBag.OrdenTrabajo = orden;
            return View(factura);
        }

        // POST: Facturas/RegistrarPago/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPago(int id, decimal monto, TipoPago tipoPago, string? referencia, string? observaciones)
        {
            var factura = await _context.Facturas
                .Include(f => f.Pagos)
                .FirstOrDefaultAsync(f => f.FacturaId == id);

            if (factura == null) return NotFound();

            if (monto <= 0)
            {
                TempData["Error"] = "El monto debe ser mayor a cero.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var totalPagado = factura.Pagos?.Sum(p => p.Monto) ?? 0;
            var pendiente = factura.Total - totalPagado;

            if (monto > pendiente)
            {
                TempData["Error"] = $"El monto no puede ser mayor al saldo pendiente ($${pendiente:N2}).";
                return RedirectToAction(nameof(Details), new { id });
            }

            var pago = new Pago
            {
                FacturaId = factura.FacturaId,
                Monto = monto,
                FechaPago = DateTime.Now,
                TipoPago = tipoPago,
                NumeroReferencia = referencia,
                Observaciones = observaciones,
                EmpleadoId = (await ObtenerEmpleadoActualId()) ?? 1 // En un escenario real sería del usuario logueado
            };

            _context.Add(pago);

            // Actualizar estado de la factura
            var nuevoTotalPagado = totalPagado + monto;
            if (nuevoTotalPagado >= factura.Total)
            {
                factura.Estado = EstadoFactura.Pagada;
            }
            else if (nuevoTotalPagado > 0)
            {
                factura.Estado = EstadoFactura.Emitida; // Parcialmente pagada
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Pago de $${monto:N2} registrado exitosamente.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Facturas/Imprimir/5
        public async Task<IActionResult> Imprimir(int? id)
        {
            if (id == null) return NotFound();

            var factura = await _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.OrdenTrabajo)
                    .ThenInclude(o => o.Vehiculo)
                .Include(f => f.OrdenTrabajo)
                    .ThenInclude(o => o.EmpleadoAsignado)
                .Include(f => f.Pagos)
                .FirstOrDefaultAsync(f => f.FacturaId == id);

            if (factura == null) return NotFound();

            return View(factura);
        }

        // GET: Facturas/Reporte
        public async Task<IActionResult> Reporte(DateTime? fechaInicio, DateTime? fechaFin, EstadoFactura? estado)
        {
            if (!fechaInicio.HasValue) fechaInicio = DateTime.Now.AddMonths(-1);
            if (!fechaFin.HasValue) fechaFin = DateTime.Now;

            ViewData["FechaInicio"] = fechaInicio;
            ViewData["FechaFin"] = fechaFin;
            ViewData["Estado"] = estado;

            var facturasQuery = _context.Facturas
                .Include(f => f.Cliente)
                .Where(f => f.FechaEmision >= fechaInicio && f.FechaEmision <= fechaFin);

            if (estado.HasValue)
            {
                facturasQuery = facturasQuery.Where(f => f.Estado == estado);
            }

            var facturas = await facturasQuery
                .OrderBy(f => f.FechaEmision)
                .ToListAsync();

            var resumen = new
            {
                TotalFacturas = facturas.Count,
                TotalFacturado = facturas.Sum(f => f.Total),
                TotalPagadas = facturas.Count(f => f.Estado == EstadoFactura.Pagada),
                TotalEmitidas = facturas.Count(f => f.Estado == EstadoFactura.Emitida),
                TotalBorrador = facturas.Count(f => f.Estado == EstadoFactura.Borrador)
            };

            ViewBag.Resumen = resumen;
            return View(facturas);
        }

        private async Task<string> GenerarNumeroFactura()
        {
            var fecha = DateTime.Now;
            var prefijo = $"F-{fecha:yyyyMM}";

            var ultimaFactura = await _context.Facturas
                .Where(f => f.NumeroFactura.StartsWith(prefijo))
                .OrderByDescending(f => f.NumeroFactura)
                .FirstOrDefaultAsync();

            int siguiente = 1;
            if (ultimaFactura != null)
            {
                var ultimoNumero = ultimaFactura.NumeroFactura.Substring(prefijo.Length + 1);
                if (int.TryParse(ultimoNumero, out int ultimo))
                {
                    siguiente = ultimo + 1;
                }
            }

            return $"{prefijo}-{siguiente:D4}";
        }

        private async Task<int?> ObtenerEmpleadoActualId()
        {
            // En un escenario real, obtendríamos el empleado del usuario autenticado
            var empleado = await _context.Empleados.FirstOrDefaultAsync();
            return empleado?.EmpleadoId;
        }

        private bool FacturaExists(int id)
        {
            return _context.Facturas.Any(e => e.FacturaId == id);
        }

        // GET: Facturas/DescargarPdf/5
        public async Task<IActionResult> DescargarPdf(int id)
        {
            var factura = await _context.Facturas.FirstOrDefaultAsync(f => f.FacturaId == id);
            if (factura == null)
            {
                return NotFound();
            }

            try
            {
                var pdfBytes = await _pdfService.GenerarFacturaPdfAsync(factura);
                return File(pdfBytes, "application/pdf", $"Factura_{factura.NumeroFactura}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al generar PDF: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }
    }
}