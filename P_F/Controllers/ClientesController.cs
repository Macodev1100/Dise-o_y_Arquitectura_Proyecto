using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P_F.Models.Entities;
using P_F.Services;

namespace P_F.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly IVehiculoService _vehiculoService;

        public ClientesController(IClienteService clienteService, IVehiculoService vehiculoService)
        {
            _clienteService = clienteService;
            _vehiculoService = vehiculoService;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            var clientes = await _clienteService.GetAllAsync();
            return View(clientes);
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var cliente = await _clienteService.GetByIdAsync(id);
            if (cliente == null)
                return NotFound();

            ViewBag.Vehiculos = await _vehiculoService.GetByClienteIdAsync(id);
            return View(cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                await _clienteService.CreateAsync(cliente);
                TempData["Success"] = "Cliente creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var cliente = await _clienteService.GetByIdAsync(id);
            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cliente cliente)
        {
            if (id != cliente.ClienteId)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _clienteService.UpdateAsync(cliente);
                TempData["Success"] = "Cliente actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _clienteService.DeleteAsync(id);
            if (result)
                TempData["Success"] = "Cliente eliminado exitosamente.";
            else
                TempData["Error"] = "No se pudo eliminar el cliente.";

            return RedirectToAction(nameof(Index));
        }

        // AJAX: Buscar clientes
        [HttpGet]
        public async Task<IActionResult> Buscar(string termino)
        {
            var clientes = await _clienteService.SearchAsync(termino ?? "");
            var resultado = clientes.Select(c => new
            {
                id = c.ClienteId,
                nombre = $"{c.Nombre} {c.Apellido}",
                documento = c.DocumentoIdentidad,
                telefono = c.Telefono,
                email = c.Email
            });
            return Json(resultado);
        }
    }
}