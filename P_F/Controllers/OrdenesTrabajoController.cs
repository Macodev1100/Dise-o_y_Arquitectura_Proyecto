using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using P_F.Models.Entities;
using P_F.Services;

namespace P_F.Controllers
{
    [Authorize]
    public class OrdenesTrabajoController : Controller
    {
        private readonly IOrdenTrabajoService _ordenTrabajoService;
        private readonly IClienteService _clienteService;
        private readonly IVehiculoService _vehiculoService;
        private readonly IEmpleadoService _empleadoService;
        private readonly IServicioService _servicioService;
        private readonly IPdfService _pdfService;

        public OrdenesTrabajoController(
            IOrdenTrabajoService ordenTrabajoService,
            IClienteService clienteService,
            IVehiculoService vehiculoService,
            IEmpleadoService empleadoService,
            IServicioService servicioService,
            IPdfService pdfService)
        {
            _ordenTrabajoService = ordenTrabajoService;
            _clienteService = clienteService;
            _vehiculoService = vehiculoService;
            _empleadoService = empleadoService;
            _servicioService = servicioService;
            _pdfService = pdfService;
        }

        // GET: OrdenesTrabajoService
        public async Task<IActionResult> Index(EstadoOrden? estado)
        {
            IEnumerable<OrdenTrabajo> ordenes;

            if (estado.HasValue)
                ordenes = await _ordenTrabajoService.GetByEstadoAsync(estado.Value);
            else
                ordenes = await _ordenTrabajoService.GetAllAsync();

            ViewBag.EstadoFiltro = estado;
            ViewBag.Estados = Enum.GetValues<EstadoOrden>().Select(e => new SelectListItem 
            { 
                Value = ((int)e).ToString(), 
                Text = e.ToString() 
            });

            return View(ordenes);
        }

        // GET: OrdenesTrabajoService/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var orden = await _ordenTrabajoService.GetByIdAsync(id);
            if (orden == null)
                return NotFound();

            return View(orden);
        }

        // GET: OrdenesTrabajoService/Create
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View();
        }

        // POST: OrdenesTrabajoService/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrdenTrabajo orden)
        {
            if (ModelState.IsValid)
            {
                await _ordenTrabajoService.CreateAsync(orden);
                TempData["Success"] = "Orden de trabajo creada exitosamente.";
                return RedirectToAction(nameof(Details), new { id = orden.OrdenTrabajoId });
            }

            await LoadViewData();
            return View(orden);
        }

        // GET: OrdenesTrabajoService/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var orden = await _ordenTrabajoService.GetByIdAsync(id);
            if (orden == null)
                return NotFound();

            await LoadViewData();
            return View(orden);
        }

        // POST: OrdenesTrabajoService/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrdenTrabajo orden)
        {
            if (id != orden.OrdenTrabajoId)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _ordenTrabajoService.UpdateAsync(orden);
                TempData["Success"] = "Orden de trabajo actualizada exitosamente.";
                return RedirectToAction(nameof(Details), new { id = orden.OrdenTrabajoId });
            }

            await LoadViewData();
            return View(orden);
        }

        // POST: Cambiar estado de orden
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, EstadoOrden nuevoEstado, string? observaciones)
        {
            // TODO: Obtener empleado actual del contexto de usuario
            var empleadoId = 1; // Por ahora hardcodeado

            var result = await _ordenTrabajoService.CambiarEstadoAsync(id, nuevoEstado, empleadoId, observaciones);
            
            if (result)
                return Json(new { success = true, message = "Estado cambiado exitosamente" });
            else
                return Json(new { success = false, message = "Error al cambiar el estado" });
        }

        // GET: Obtener vehículos por cliente
        [HttpGet]
        public async Task<IActionResult> GetVehiculosPorCliente(int clienteId)
        {
            var vehiculos = await _vehiculoService.GetByClienteIdAsync(clienteId);
            var resultado = vehiculos.Select(v => new
            {
                id = v.VehiculoId,
                texto = $"{v.Placa} - {v.Marca} {v.Modelo} ({v.Anio})"
            });
            return Json(resultado);
        }

        private async Task LoadViewData()
        {
            var clientes = await _clienteService.GetAllAsync();
            var empleados = await _empleadoService.GetByTipoAsync(TipoEmpleado.Mecanico);
            var recepcionistas = await _empleadoService.GetByTipoAsync(TipoEmpleado.Recepcionista);

            ViewBag.Clientes = new SelectList(clientes, "ClienteId", "Nombre");
            ViewBag.Mecanicos = new SelectList(empleados, "EmpleadoId", "Nombre");
            ViewBag.Recepcionistas = new SelectList(recepcionistas, "EmpleadoId", "Nombre");
            ViewBag.Estados = Enum.GetValues<EstadoOrden>().Select(e => new SelectListItem 
            { 
                Value = ((int)e).ToString(), 
                Text = e.ToString() 
            });
            ViewBag.Prioridades = Enum.GetValues<Prioridad>().Select(p => new SelectListItem 
            { 
                Value = ((int)p).ToString(), 
                Text = p.ToString() 
            });
        }

        // GET: OrdenesTrabajo/DescargarPdf/5
        public async Task<IActionResult> DescargarPdf(int id)
        {
            var orden = await _ordenTrabajoService.GetByIdAsync(id);
            if (orden == null)
            {
                return NotFound();
            }

            try
            {
                var pdfBytes = await _pdfService.GenerarOrdenTrabajoPdfAsync(orden);
                return File(pdfBytes, "application/pdf", $"OrdenTrabajo_{orden.NumeroOrden}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al generar PDF: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }
    }
}