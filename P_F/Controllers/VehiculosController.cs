using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P_F.Data;
using P_F.Models.Entities;
using P_F.Services;

namespace P_F.Controllers
{
    public class VehiculosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IClienteService _clienteService;

        public VehiculosController(ApplicationDbContext context, IClienteService clienteService)
        {
            _context = context;
            _clienteService = clienteService;
        }

        // GET: Vehiculos
        public async Task<IActionResult> Index(string searchString, int? clienteId, string sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentClienteFilter"] = clienteId;
            ViewData["MarcaSortParm"] = string.IsNullOrEmpty(sortOrder) ? "marca_desc" : "";
            ViewData["PlacaSortParm"] = sortOrder == "Placa" ? "placa_desc" : "Placa";
            ViewData["AnioSortParm"] = sortOrder == "Anio" ? "anio_desc" : "Anio";

            var vehiculosQuery = _context.Vehiculos
                .Include(v => v.Cliente)
                .Include(v => v.OrdenesTrabajo)
                .Where(v => v.Activo);

            if (clienteId.HasValue)
            {
                vehiculosQuery = vehiculosQuery.Where(v => v.ClienteId == clienteId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                vehiculosQuery = vehiculosQuery.Where(v =>
                    v.Placa.Contains(searchString) ||
                    v.Marca.Contains(searchString) ||
                    v.Modelo.Contains(searchString) ||
                    v.Cliente.Nombre.Contains(searchString) ||
                    v.Cliente.Apellido.Contains(searchString));
            }

            vehiculosQuery = sortOrder switch
            {
                "marca_desc" => vehiculosQuery.OrderByDescending(v => v.Marca),
                "Placa" => vehiculosQuery.OrderBy(v => v.Placa),
                "placa_desc" => vehiculosQuery.OrderByDescending(v => v.Placa),
                "Anio" => vehiculosQuery.OrderBy(v => v.Anio),
                "anio_desc" => vehiculosQuery.OrderByDescending(v => v.Anio),
                _ => vehiculosQuery.OrderBy(v => v.Marca)
            };

            ViewBag.Clientes = await _context.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            var vehiculos = await vehiculosQuery.ToListAsync();
            return View(vehiculos);
        }

        // GET: Vehiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos
                .Include(v => v.Cliente)
                .Include(v => v.OrdenesTrabajo)
                    .ThenInclude(ot => ot.EmpleadoAsignado)
                .FirstOrDefaultAsync(v => v.VehiculoId == id);

            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        // GET: Vehiculos/Create
        public async Task<IActionResult> Create(int? clienteId)
        {
            ViewBag.ClienteId = clienteId;
            ViewBag.Clientes = await _context.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            var vehiculo = new Vehiculo 
            { 
                FechaRegistro = DateTime.Now,
                Activo = true
            };

            if (clienteId.HasValue)
            {
                vehiculo.ClienteId = clienteId.Value;
            }

            return View(vehiculo);
        }

        // POST: Vehiculos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClienteId,Placa,Marca,Modelo,Anio,Color,TipoCombustible,Transmision,NumeroMotor,NumeroChasis,Kilometraje,Activo")] Vehiculo vehiculo)
        {
            if (ModelState.IsValid)
            {
                vehiculo.FechaRegistro = DateTime.Now;

                // Verificar si ya existe un vehículo con esa placa
                var existePlaca = await _context.Vehiculos
                    .AnyAsync(v => v.Placa == vehiculo.Placa && v.VehiculoId != vehiculo.VehiculoId);

                if (existePlaca)
                {
                    ModelState.AddModelError("Placa", "Ya existe un vehículo registrado con esta placa.");
                }
                else
                {
                    _context.Add(vehiculo);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Vehículo {vehiculo.Placa} registrado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Clientes = await _context.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(vehiculo);
        }

        // GET: Vehiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            ViewBag.Clientes = await _context.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(vehiculo);
        }

        // POST: Vehiculos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VehiculoId,ClienteId,Placa,Marca,Modelo,Anio,Color,TipoCombustible,Transmision,NumeroMotor,NumeroChasis,Kilometraje,FechaRegistro,Activo")] Vehiculo vehiculo)
        {
            if (id != vehiculo.VehiculoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si ya existe un vehículo con esa placa
                    var existePlaca = await _context.Vehiculos
                        .AnyAsync(v => v.Placa == vehiculo.Placa && v.VehiculoId != vehiculo.VehiculoId);

                    if (existePlaca)
                    {
                        ModelState.AddModelError("Placa", "Ya existe un vehículo registrado con esta placa.");
                    }
                    else
                    {
                        _context.Update(vehiculo);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = $"Vehículo {vehiculo.Placa} actualizado exitosamente.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehiculoExists(vehiculo.VehiculoId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Clientes = await _context.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(vehiculo);
        }

        // GET: Vehiculos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculos
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.VehiculoId == id);

            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        // POST: Vehiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo != null)
            {
                // Verificar si tiene órdenes de trabajo asociadas
                var tieneOrdenes = await _context.OrdenesTrabajo
                    .AnyAsync(ot => ot.VehiculoId == id);

                if (tieneOrdenes)
                {
                    // Si tiene órdenes, solo desactivar
                    vehiculo.Activo = false;
                    _context.Update(vehiculo);
                    TempData["Info"] = $"Vehículo {vehiculo.Placa} desactivado (tiene órdenes de trabajo asociadas).";
                }
                else
                {
                    // Si no tiene órdenes, eliminar completamente
                    _context.Vehiculos.Remove(vehiculo);
                    TempData["Success"] = $"Vehículo {vehiculo.Placa} eliminado exitosamente.";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VehiculoExists(int id)
        {
            return _context.Vehiculos.Any(e => e.VehiculoId == id);
        }

        // Método para obtener vehículos por cliente (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetVehiculosByCliente(int clienteId)
        {
            var vehiculos = await _context.Vehiculos
                .Where(v => v.ClienteId == clienteId && v.Activo)
                .Select(v => new 
                { 
                    VehiculoId = v.VehiculoId,
                    Descripcion = $"{v.Placa} - {v.Marca} {v.Modelo} ({v.Anio})"
                })
                .ToListAsync();

            return Json(vehiculos);
        }
    }
}