using Microsoft.AspNetCore.Mvc;
using P_F.Models.Entities;
using P_F.Services;

namespace P_F.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdenesController : ControllerBase
    {
        private readonly IOrdenTrabajoService _ordenTrabajoService;
        private readonly IClienteService _clienteService;

        public OrdenesController(IOrdenTrabajoService ordenTrabajoService, IClienteService clienteService)
        {
            _ordenTrabajoService = ordenTrabajoService;
            _clienteService = clienteService;
        }

        // GET: api/ordenes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetOrdenes()
        {
            var ordenes = await _ordenTrabajoService.GetAllAsync();
            var resultado = ordenes.Select(o => new
            {
                id = o.OrdenTrabajoId,
                numeroOrden = o.NumeroOrden,
                cliente = $"{o.Cliente.Nombre} {o.Cliente.Apellido}",
                vehiculo = $"{o.Vehiculo.Marca} {o.Vehiculo.Modelo} ({o.Vehiculo.Placa})",
                estado = o.Estado.ToString(),
                fechaIngreso = o.FechaIngreso,
                fechaPromesa = o.FechaPromesaEntrega,
                total = o.Total
            });

            return Ok(resultado);
        }

        // GET: api/ordenes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOrden(int id)
        {
            var orden = await _ordenTrabajoService.GetByIdAsync(id);
            if (orden == null)
                return NotFound();

            var resultado = new
            {
                id = orden.OrdenTrabajoId,
                numeroOrden = orden.NumeroOrden,
                cliente = new
                {
                    id = orden.Cliente.ClienteId,
                    nombre = $"{orden.Cliente.Nombre} {orden.Cliente.Apellido}",
                    telefono = orden.Cliente.Telefono,
                    email = orden.Cliente.Email
                },
                vehiculo = new
                {
                    id = orden.Vehiculo.VehiculoId,
                    placa = orden.Vehiculo.Placa,
                    marca = orden.Vehiculo.Marca,
                    modelo = orden.Vehiculo.Modelo,
                    año = orden.Vehiculo.Anio
                },
                estado = orden.Estado.ToString(),
                prioridad = orden.Prioridad.ToString(),
                fechaIngreso = orden.FechaIngreso,
                fechaPromesa = orden.FechaPromesaEntrega,
                fechaEntrega = orden.FechaEntrega,
                descripcionProblema = orden.DescripcionProblema,
                diagnostico = orden.DiagnosticoTecnico,
                total = orden.Total,
                servicios = orden.Servicios?.Select(s => new
                {
                    nombre = s.Servicio.Nombre,
                    precio = s.Precio,
                    cantidad = s.Cantidad,
                    completado = s.Completado
                }),
                repuestos = orden.Repuestos?.Select(r => new
                {
                    nombre = r.Repuesto.Nombre,
                    cantidad = r.Cantidad,
                    precio = r.PrecioUnitario,
                    entregado = r.Entregado
                })
            };

            return Ok(resultado);
        }

        // PUT: api/ordenes/5/estado
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambioEstadoRequest request)
        {
            var result = await _ordenTrabajoService.CambiarEstadoAsync(id, request.NuevoEstado, request.EmpleadoId, request.Observaciones);
            
            if (result)
                return Ok(new { message = "Estado actualizado exitosamente" });
            else
                return BadRequest(new { message = "Error al actualizar el estado" });
        }

        // GET: api/ordenes/empleado/5
        [HttpGet("empleado/{empleadoId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetOrdenesPorEmpleado(int empleadoId)
        {
            var ordenes = await _ordenTrabajoService.GetByEmpleadoAsync(empleadoId);
            var resultado = ordenes.Select(o => new
            {
                id = o.OrdenTrabajoId,
                numeroOrden = o.NumeroOrden,
                cliente = $"{o.Cliente.Nombre} {o.Cliente.Apellido}",
                vehiculo = $"{o.Vehiculo.Placa}",
                estado = o.Estado.ToString(),
                prioridad = o.Prioridad.ToString(),
                fechaIngreso = o.FechaIngreso
            });

            return Ok(resultado);
        }
    }

    // GET: api/clientes
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetClientes()
        {
            var clientes = await _clienteService.GetAllAsync();
            var resultado = clientes.Select(c => new
            {
                id = c.ClienteId,
                nombre = $"{c.Nombre} {c.Apellido}",
                documento = c.DocumentoIdentidad,
                telefono = c.Telefono,
                email = c.Email,
                vehiculos = c.Vehiculos.Count
            });

            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCliente(int id)
        {
            var cliente = await _clienteService.GetByIdAsync(id);
            if (cliente == null)
                return NotFound();

            var resultado = new
            {
                id = cliente.ClienteId,
                nombre = cliente.Nombre,
                apellido = cliente.Apellido,
                documento = cliente.DocumentoIdentidad,
                telefono = cliente.Telefono,
                email = cliente.Email,
                direccion = cliente.Direccion,
                vehiculos = cliente.Vehiculos.Select(v => new
                {
                    id = v.VehiculoId,
                    placa = v.Placa,
                    marca = v.Marca,
                    modelo = v.Modelo,
                    año = v.Anio
                })
            };

            return Ok(resultado);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<IEnumerable<object>>> BuscarClientes([FromQuery] string termino)
        {
            var clientes = await _clienteService.SearchAsync(termino ?? "");
            var resultado = clientes.Select(c => new
            {
                id = c.ClienteId,
                nombre = $"{c.Nombre} {c.Apellido}",
                documento = c.DocumentoIdentidad,
                telefono = c.Telefono
            });

            return Ok(resultado);
        }
    }

    // DTOs para la API
    public class CambioEstadoRequest
    {
        public EstadoOrden NuevoEstado { get; set; }
        public int EmpleadoId { get; set; }
        public string? Observaciones { get; set; }
    }
}