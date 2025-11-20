using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P_F.Data;
using P_F.Models.Entities;

namespace P_F.Controllers
{
    public class RepuestosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RepuestosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Repuestos
        public async Task<IActionResult> Index(string searchString, int? categoriaId, bool? stockBajo, string sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CategoriaFilter"] = categoriaId;
            ViewData["StockBajoFilter"] = stockBajo;
            ViewData["CodigoSortParm"] = string.IsNullOrEmpty(sortOrder) ? "codigo_desc" : "";
            ViewData["NombreSortParm"] = sortOrder == "Nombre" ? "nombre_desc" : "Nombre";
            ViewData["StockSortParm"] = sortOrder == "Stock" ? "stock_desc" : "Stock";
            ViewData["PrecioSortParm"] = sortOrder == "Precio" ? "precio_desc" : "Precio";

            var repuestosQuery = _context.Repuestos
                .Include(r => r.CategoriaRepuesto)
                .Where(r => r.Activo);

            if (!string.IsNullOrEmpty(searchString))
            {
                repuestosQuery = repuestosQuery.Where(r =>
                    r.Codigo.Contains(searchString) ||
                    r.Nombre.Contains(searchString) ||
                    (r.Marca != null && r.Marca.Contains(searchString)) ||
                    (r.Descripcion != null && r.Descripcion.Contains(searchString)));
            }

            if (categoriaId.HasValue)
            {
                repuestosQuery = repuestosQuery.Where(r => r.CategoriaRepuestoId == categoriaId.Value);
            }

            if (stockBajo == true)
            {
                repuestosQuery = repuestosQuery.Where(r => r.StockActual <= r.StockMinimo);
            }

            repuestosQuery = sortOrder switch
            {
                "codigo_desc" => repuestosQuery.OrderByDescending(r => r.Codigo),
                "Nombre" => repuestosQuery.OrderBy(r => r.Nombre),
                "nombre_desc" => repuestosQuery.OrderByDescending(r => r.Nombre),
                "Stock" => repuestosQuery.OrderBy(r => r.StockActual),
                "stock_desc" => repuestosQuery.OrderByDescending(r => r.StockActual),
                "Precio" => repuestosQuery.OrderBy(r => r.PrecioVenta),
                "precio_desc" => repuestosQuery.OrderByDescending(r => r.PrecioVenta),
                _ => repuestosQuery.OrderBy(r => r.Codigo)
            };

            ViewBag.Categorias = await _context.CategoriasRepuesto
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            var repuestos = await repuestosQuery.ToListAsync();
            return View(repuestos);
        }

        // GET: Repuestos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var repuesto = await _context.Repuestos
                .Include(r => r.CategoriaRepuesto)
                .Include(r => r.MovimientosInventario)
                .FirstOrDefaultAsync(r => r.RepuestoId == id);

            if (repuesto == null) return NotFound();

            return View(repuesto);
        }

        // GET: Repuestos/Create
        public async Task<IActionResult> Create()
        {
            await CargarViewBags();
            
            var repuesto = new Repuesto
            {
                FechaCreacion = DateTime.Now,
                Activo = true,
                PorcentajeGanancia = 25.00m // 25% por defecto
            };

            return View(repuesto);
        }

        // POST: Repuestos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoriaRepuestoId,Codigo,Nombre,Descripcion,Marca,Modelo,PrecioCosto,PorcentajeGanancia,PrecioVenta,StockActual,StockMinimo,StockMaximo,Ubicacion,Proveedor,Activo")] Repuesto repuesto)
        {
            if (ModelState.IsValid)
            {
                // Verificar código único
                var existeCodigo = await _context.Repuestos
                    .AnyAsync(r => r.Codigo == repuesto.Codigo);

                if (existeCodigo)
                {
                    ModelState.AddModelError("Codigo", "Ya existe un repuesto con este código.");
                }
                else
                {
                    repuesto.FechaCreacion = DateTime.Now;
                    
                    // Calcular precio de venta si no se especificó
                    if (repuesto.PrecioVenta <= 0 && repuesto.PrecioCosto > 0)
                    {
                        repuesto.PrecioVenta = repuesto.PrecioCosto * (1 + repuesto.PorcentajeGanancia / 100);
                    }

                    _context.Add(repuesto);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Repuesto {repuesto.Nombre} creado exitosamente.";
                    return RedirectToAction(nameof(Details), new { id = repuesto.RepuestoId });
                }
            }

            await CargarViewBags();
            return View(repuesto);
        }

        // GET: Repuestos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var repuesto = await _context.Repuestos.FindAsync(id);
            if (repuesto == null) return NotFound();

            await CargarViewBags();
            return View(repuesto);
        }

        // POST: Repuestos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RepuestoId,CategoriaRepuestoId,Codigo,Nombre,Descripcion,Marca,Modelo,PrecioCosto,PorcentajeGanancia,PrecioVenta,StockActual,StockMinimo,StockMaximo,Ubicacion,Proveedor,FechaCreacion,Activo")] Repuesto repuesto)
        {
            if (id != repuesto.RepuestoId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar código único
                    var existeCodigo = await _context.Repuestos
                        .AnyAsync(r => r.Codigo == repuesto.Codigo && r.RepuestoId != repuesto.RepuestoId);

                    if (existeCodigo)
                    {
                        ModelState.AddModelError("Codigo", "Ya existe un repuesto con este código.");
                    }
                    else
                    {
                        // Calcular precio de venta si cambió el costo o porcentaje
                        if (repuesto.PrecioCosto > 0)
                        {
                            repuesto.PrecioVenta = repuesto.PrecioCosto * (1 + repuesto.PorcentajeGanancia / 100);
                        }

                        _context.Update(repuesto);
                        await _context.SaveChangesAsync();
                        
                        TempData["Success"] = $"Repuesto {repuesto.Nombre} actualizado exitosamente.";
                        return RedirectToAction(nameof(Details), new { id = repuesto.RepuestoId });
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RepuestoExists(repuesto.RepuestoId))
                        return NotFound();
                    else
                        throw;
                }
            }

            await CargarViewBags();
            return View(repuesto);
        }

        // GET: Repuestos/MovimientoStock/5
        public async Task<IActionResult> MovimientoStock(int? id)
        {
            if (id == null) return NotFound();

            var repuesto = await _context.Repuestos.FindAsync(id);
            if (repuesto == null) return NotFound();

            var movimiento = new MovimientoInventario
            {
                RepuestoId = repuesto.RepuestoId,
                FechaMovimiento = DateTime.Now,
                TipoMovimiento = TipoMovimiento.Entrada
            };

            ViewBag.Repuesto = repuesto;
            return View(movimiento);
        }

        // POST: Repuestos/MovimientoStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MovimientoStock([Bind("RepuestoId,TipoMovimiento,Cantidad,Motivo,DocumentoReferencia")] MovimientoInventario movimiento)
        {
            if (ModelState.IsValid)
            {
                var repuesto = await _context.Repuestos.FindAsync(movimiento.RepuestoId);
                if (repuesto == null) return NotFound();

                movimiento.FechaMovimiento = DateTime.Now;
                movimiento.StockAnterior = repuesto.StockActual;

                // Actualizar stock según tipo de movimiento
                switch (movimiento.TipoMovimiento)
                {
                    case TipoMovimiento.Entrada:
                        repuesto.StockActual += movimiento.Cantidad;
                        break;
                    case TipoMovimiento.Salida:
                        if (repuesto.StockActual >= movimiento.Cantidad)
                        {
                            repuesto.StockActual -= movimiento.Cantidad;
                        }
                        else
                        {
                            ModelState.AddModelError("Cantidad", "No hay suficiente stock disponible.");
                            ViewBag.Repuesto = repuesto;
                            return View(movimiento);
                        }
                        break;
                    case TipoMovimiento.Ajuste:
                        repuesto.StockActual = movimiento.Cantidad;
                        break;
                }

                movimiento.StockNuevo = repuesto.StockActual;

                _context.Add(movimiento);
                _context.Update(repuesto);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Movimiento de stock registrado exitosamente. Nuevo stock: {repuesto.StockActual}";
                return RedirectToAction(nameof(Details), new { id = repuesto.RepuestoId });
            }

            var repuestoView = await _context.Repuestos.FindAsync(movimiento.RepuestoId);
            ViewBag.Repuesto = repuestoView;
            return View(movimiento);
        }

        // GET: Repuestos/StockBajo
        public async Task<IActionResult> StockBajo()
        {
            var repuestosStockBajo = await _context.Repuestos
                .Include(r => r.CategoriaRepuesto)
                .Where(r => r.Activo && r.StockActual <= r.StockMinimo)
                .OrderBy(r => r.StockActual)
                .ToListAsync();

            return View(repuestosStockBajo);
        }

        private bool RepuestoExists(int id)
        {
            return _context.Repuestos.Any(e => e.RepuestoId == id);
        }

        private async Task CargarViewBags()
        {
            ViewBag.Categorias = await _context.CategoriasRepuesto
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }
    }
}