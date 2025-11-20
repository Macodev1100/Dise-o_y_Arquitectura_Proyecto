using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using P_F.Models.Entities;
using P_F.Services;

namespace P_F.Controllers
{
    [Authorize]
    public class InventarioController : Controller
    {
        private readonly IInventarioService _inventarioService;

        public InventarioController(IInventarioService inventarioService)
        {
            _inventarioService = inventarioService;
        }

        // GET: Inventario
        public async Task<IActionResult> Index()
        {
            var repuestos = await _inventarioService.GetAllRepuestosAsync();
            return View(repuestos);
        }

        // GET: Inventario/StockBajo
        public async Task<IActionResult> StockBajo()
        {
            var repuestos = await _inventarioService.GetRepuestosBajoStockAsync();
            return View(repuestos);
        }

        // GET: Inventario/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var repuesto = await _inventarioService.GetRepuestoByIdAsync(id);
            if (repuesto == null)
                return NotFound();

            return View(repuesto);
        }

        // GET: Inventario/Create
        public IActionResult Create()
        {
            LoadCategorias();
            return View();
        }

        // POST: Inventario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Repuesto repuesto)
        {
            if (ModelState.IsValid)
            {
                await _inventarioService.CreateRepuestoAsync(repuesto);
                TempData["Success"] = "Repuesto creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            LoadCategorias();
            return View(repuesto);
        }

        // GET: Inventario/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var repuesto = await _inventarioService.GetRepuestoByIdAsync(id);
            if (repuesto == null)
                return NotFound();

            LoadCategorias();
            return View(repuesto);
        }

        // POST: Inventario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Repuesto repuesto)
        {
            if (id != repuesto.RepuestoId)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _inventarioService.UpdateRepuestoAsync(repuesto);
                TempData["Success"] = "Repuesto actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            LoadCategorias();
            return View(repuesto);
        }

        // POST: Inventario/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _inventarioService.DeleteRepuestoAsync(id);
            if (result)
                TempData["Success"] = "Repuesto eliminado exitosamente.";
            else
                TempData["Error"] = "No se pudo eliminar el repuesto.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Inventario/MovimientoStock/5
        public async Task<IActionResult> MovimientoStock(int id)
        {
            var repuesto = await _inventarioService.GetRepuestoByIdAsync(id);
            if (repuesto == null)
                return NotFound();

            ViewBag.Repuesto = repuesto;
            ViewBag.TiposMovimiento = Enum.GetValues<TipoMovimiento>().Select(t => new SelectListItem
            {
                Value = ((int)t).ToString(),
                Text = t.ToString()
            });

            return View();
        }

        // POST: Inventario/MovimientoStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MovimientoStock(int repuestoId, int cantidad, TipoMovimiento tipoMovimiento, string motivo)
        {
            // TODO: Obtener empleado actual del contexto
            var empleadoId = 1; // Por ahora hardcodeado

            var result = await _inventarioService.ActualizarStockAsync(repuestoId, cantidad, tipoMovimiento, motivo, empleadoId);
            
            if (result)
            {
                TempData["Success"] = "Movimiento de stock registrado exitosamente.";
                return RedirectToAction(nameof(Details), new { id = repuestoId });
            }
            else
            {
                TempData["Error"] = "Error al registrar el movimiento de stock.";
                return RedirectToAction(nameof(MovimientoStock), new { id = repuestoId });
            }
        }

        private void LoadCategorias()
        {
            // Por simplicidad, usando categorías hardcodeadas
            // En producción, esto vendría de la base de datos
            var categorias = new List<SelectListItem>
            {
                new() { Value = "1", Text = "Filtros" },
                new() { Value = "2", Text = "Lubricantes" },
                new() { Value = "3", Text = "Frenos" },
                new() { Value = "4", Text = "Motor" },
                new() { Value = "5", Text = "Eléctrico" }
            };

            ViewBag.CategoriaRepuestoId = categorias;
        }
    }
}