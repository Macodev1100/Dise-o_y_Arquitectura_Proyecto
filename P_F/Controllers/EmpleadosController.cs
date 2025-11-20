using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P_F.Data;
using P_F.Models.Entities;

namespace P_F.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmpleadosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Empleados
        public async Task<IActionResult> Index(string searchString, TipoEmpleado? tipo, bool? activo, string sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["TipoFilter"] = tipo;
            ViewData["ActivoFilter"] = activo;
            ViewData["NombreSortParm"] = string.IsNullOrEmpty(sortOrder) ? "nombre_desc" : "";
            ViewData["TipoSortParm"] = sortOrder == "Tipo" ? "tipo_desc" : "Tipo";
            ViewData["FechaSortParm"] = sortOrder == "Fecha" ? "fecha_desc" : "Fecha";

            var empleadosQuery = _context.Empleados.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                empleadosQuery = empleadosQuery.Where(e =>
                    e.Nombre.Contains(searchString) ||
                    e.Apellido.Contains(searchString) ||
                    e.DocumentoIdentidad.Contains(searchString) ||
                    (e.Email != null && e.Email.Contains(searchString)));
            }

            if (tipo.HasValue)
            {
                empleadosQuery = empleadosQuery.Where(e => e.TipoEmpleado == tipo.Value);
            }

            if (activo.HasValue)
            {
                empleadosQuery = empleadosQuery.Where(e => e.Activo == activo.Value);
            }
            else
            {
                empleadosQuery = empleadosQuery.Where(e => e.Activo); // Por defecto solo activos
            }

            empleadosQuery = sortOrder switch
            {
                "nombre_desc" => empleadosQuery.OrderByDescending(e => e.Nombre),
                "Tipo" => empleadosQuery.OrderBy(e => e.TipoEmpleado),
                "tipo_desc" => empleadosQuery.OrderByDescending(e => e.TipoEmpleado),
                "Fecha" => empleadosQuery.OrderBy(e => e.FechaContratacion),
                "fecha_desc" => empleadosQuery.OrderByDescending(e => e.FechaContratacion),
                _ => empleadosQuery.OrderBy(e => e.Nombre)
            };

            var empleados = await empleadosQuery.ToListAsync();
            return View(empleados);
        }

        // GET: Empleados/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados
                .Include(e => e.OrdenesAsignadas)
                    .ThenInclude(o => o.Cliente)
                .Include(e => e.OrdenesAsignadas)
                    .ThenInclude(o => o.Vehiculo)
                .Include(e => e.RegistrosTiempo)
                .FirstOrDefaultAsync(e => e.EmpleadoId == id);

            if (empleado == null) return NotFound();

            return View(empleado);
        }

        // GET: Empleados/Create
        public IActionResult Create()
        {
            var empleado = new Empleado
            {
                FechaContratacion = DateTime.Now,
                Activo = true,
                SalarioHora = 15.00m,
                PorcentajeComision = 5.00m
            };

            return View(empleado);
        }

        // POST: Empleados/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Apellido,DocumentoIdentidad,Email,Telefono,Direccion,TipoEmpleado,Especialidad,SalarioHora,PorcentajeComision,Observaciones,Activo")] Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                // Verificar documento único
                var existeDocumento = await _context.Empleados
                    .AnyAsync(e => e.DocumentoIdentidad == empleado.DocumentoIdentidad);

                if (existeDocumento)
                {
                    ModelState.AddModelError("DocumentoIdentidad", "Ya existe un empleado con este documento.");
                    return View(empleado);
                }

                empleado.FechaContratacion = DateTime.Now;
                _context.Add(empleado);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Empleado {empleado.Nombre} {empleado.Apellido} registrado exitosamente.";
                return RedirectToAction(nameof(Details), new { id = empleado.EmpleadoId });
            }

            return View(empleado);
        }

        // GET: Empleados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound();

            return View(empleado);
        }

        // POST: Empleados/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmpleadoId,Nombre,Apellido,DocumentoIdentidad,Email,Telefono,Direccion,TipoEmpleado,Especialidad,SalarioHora,PorcentajeComision,FechaContratacion,FechaTerminacion,Observaciones,Activo,UserId")] Empleado empleado)
        {
            if (id != empleado.EmpleadoId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar documento único
                    var existeDocumento = await _context.Empleados
                        .AnyAsync(e => e.DocumentoIdentidad == empleado.DocumentoIdentidad && e.EmpleadoId != empleado.EmpleadoId);

                    if (existeDocumento)
                    {
                        ModelState.AddModelError("DocumentoIdentidad", "Ya existe un empleado con este documento.");
                        return View(empleado);
                    }

                    _context.Update(empleado);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Empleado {empleado.Nombre} {empleado.Apellido} actualizado exitosamente.";
                    return RedirectToAction(nameof(Details), new { id = empleado.EmpleadoId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleado.EmpleadoId))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(empleado);
        }

        // GET: Empleados/RegistroTiempo/5
        public async Task<IActionResult> RegistroTiempo(int? id)
        {
            if (id == null) return NotFound();

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound();

            ViewBag.Empleado = empleado;
            
            var ordenesActivas = await _context.OrdenesTrabajo
                .Where(o => o.EmpleadoAsignadoId == id && o.Estado == EstadoOrden.EnProceso)
                .Include(o => o.Cliente)
                .Include(o => o.Vehiculo)
                .ToListAsync();

            ViewBag.OrdenesActivas = ordenesActivas;

            var registro = new RegistroTiempo
            {
                EmpleadoId = empleado.EmpleadoId,
                FechaInicio = DateTime.Now
            };

            return View(registro);
        }

        // POST: Empleados/RegistroTiempo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroTiempo([Bind("EmpleadoId,OrdenTrabajoId,FechaInicio,FechaFin,MinutosTrabajados,Descripcion")] RegistroTiempo registro)
        {
            if (ModelState.IsValid)
            {
                // Calcular minutos trabajados si se proporciona fecha fin
                if (registro.FechaFin.HasValue)
                {
                    var duracion = registro.FechaFin.Value - registro.FechaInicio;
                    registro.MinutosTrabajados = (int)duracion.TotalMinutes;
                }

                _context.Add(registro);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Registro de tiempo guardado exitosamente.";
                return RedirectToAction(nameof(Details), new { id = registro.EmpleadoId });
            }

            var empleado = await _context.Empleados.FindAsync(registro.EmpleadoId);
            ViewBag.Empleado = empleado;
            
            var ordenesActivas = await _context.OrdenesTrabajo
                .Where(o => o.EmpleadoAsignadoId == registro.EmpleadoId && o.Estado == EstadoOrden.EnProceso)
                .Include(o => o.Cliente)
                .Include(o => o.Vehiculo)
                .ToListAsync();

            ViewBag.OrdenesActivas = ordenesActivas;

            return View(registro);
        }

        // GET: Empleados/Productividad
        public async Task<IActionResult> Productividad(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (!fechaInicio.HasValue)
                fechaInicio = DateTime.Now.AddMonths(-1);
            
            if (!fechaFin.HasValue)
                fechaFin = DateTime.Now;

            ViewData["FechaInicio"] = fechaInicio;
            ViewData["FechaFin"] = fechaFin;

            var estadisticas = await _context.Empleados
                .Where(e => e.Activo && e.TipoEmpleado == TipoEmpleado.Mecanico)
                .Select(e => new
                {
                    Empleado = e,
                    OrdenesCompletadas = e.OrdenesAsignadas
                        .Count(o => o.Estado == EstadoOrden.Completada || o.Estado == EstadoOrden.Entregada),
                    HorasTrabajadasTotal = e.RegistrosTiempo
                        .Where(r => r.FechaInicio >= fechaInicio && r.FechaInicio <= fechaFin)
                        .Sum(r => r.MinutosTrabajados / 60.0),
                    TotalFacturado = e.OrdenesAsignadas
                        .Where(o => (o.Estado == EstadoOrden.Completada || o.Estado == EstadoOrden.Entregada) 
                                   && o.FechaIngreso >= fechaInicio && o.FechaIngreso <= fechaFin)
                        .Sum(o => o.Total)
                })
                .ToListAsync();

            return View(estadisticas);
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.EmpleadoId == id);
        }
    }
}