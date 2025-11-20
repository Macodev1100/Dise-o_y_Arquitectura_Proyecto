using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using P_F.Data;
using P_F.Models.Entities;

namespace P_F.Services
{
    public class PdfService : IPdfService
    {
        private readonly ApplicationDbContext _context;

        public PdfService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerarFacturaPdfAsync(Factura factura)
        {
            factura = await _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.OrdenTrabajo)
                .Include(f => f.Empleado)
                .Include(f => f.Pagos)
                .FirstOrDefaultAsync(f => f.FacturaId == factura.FacturaId) ?? factura;

            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 50, 50);
            PdfWriter.GetInstance(document, stream);
            
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            // Título
            document.Add(new Paragraph("TALLER MECÁNICO P&F", titleFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph($"FACTURA N° {factura.NumeroFactura}", headerFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph("\n"));

            // Información del cliente
            document.Add(new Paragraph("DATOS DEL CLIENTE", headerFont));
            document.Add(new Paragraph($"Nombre: {factura.Cliente.Nombre} {factura.Cliente.Apellido}", normalFont));
            document.Add(new Paragraph($"Documento: {factura.Cliente.DocumentoIdentidad}", normalFont));
            document.Add(new Paragraph($"Teléfono: {factura.Cliente.Telefono}", normalFont));
            document.Add(new Paragraph("\n"));

            // Información de la factura
            document.Add(new Paragraph("INFORMACIÓN DE FACTURA", headerFont));
            document.Add(new Paragraph($"Fecha Emisión: {factura.FechaEmision:dd/MM/yyyy}", normalFont));
            document.Add(new Paragraph($"Estado: {factura.Estado}", normalFont));
            document.Add(new Paragraph("\n"));

            // Totales
            document.Add(new Paragraph($"Subtotal: ${factura.SubTotal:N2}", normalFont));
            document.Add(new Paragraph($"Impuestos: ${factura.Impuestos:N2}", normalFont));
            document.Add(new Paragraph($"Descuento: ${factura.Descuento:N2}", normalFont));
            document.Add(new Paragraph($"TOTAL: ${factura.Total:N2}", headerFont));

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> GenerarOrdenTrabajoPdfAsync(OrdenTrabajo orden)
        {
            orden = await _context.OrdenesTrabajo
                .Include(o => o.Cliente)
                .Include(o => o.Vehiculo)
                .Include(o => o.EmpleadoAsignado)
                .Include(o => o.EmpleadoRecepcion)
                .FirstOrDefaultAsync(o => o.OrdenTrabajoId == orden.OrdenTrabajoId) ?? orden;

            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 50, 50);
            PdfWriter.GetInstance(document, stream);
            
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            // Título
            document.Add(new Paragraph("TALLER MECÁNICO P&F", titleFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph($"ORDEN DE TRABAJO N° {orden.NumeroOrden}", headerFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph("\n"));

            // Cliente
            document.Add(new Paragraph("CLIENTE", headerFont));
            document.Add(new Paragraph($"Nombre: {orden.Cliente.Nombre} {orden.Cliente.Apellido}", normalFont));
            document.Add(new Paragraph($"Teléfono: {orden.Cliente.Telefono}", normalFont));
            document.Add(new Paragraph("\n"));

            // Vehículo
            document.Add(new Paragraph("VEHÍCULO", headerFont));
            document.Add(new Paragraph($"Marca: {orden.Vehiculo.Marca}", normalFont));
            document.Add(new Paragraph($"Modelo: {orden.Vehiculo.Modelo}", normalFont));
            document.Add(new Paragraph($"Año: {orden.Vehiculo.Anio}", normalFont));
            document.Add(new Paragraph($"Placa: {orden.Vehiculo.Placa}", normalFont));
            document.Add(new Paragraph("\n"));

            // Estado
            document.Add(new Paragraph("INFORMACIÓN DE LA ORDEN", headerFont));
            document.Add(new Paragraph($"Fecha Ingreso: {orden.FechaIngreso:dd/MM/yyyy}", normalFont));
            document.Add(new Paragraph($"Estado: {orden.Estado}", normalFont));
            document.Add(new Paragraph($"Prioridad: {orden.Prioridad}", normalFont));
            if (!string.IsNullOrEmpty(orden.DescripcionProblema))
                document.Add(new Paragraph($"Problema: {orden.DescripcionProblema}", normalFont));
            document.Add(new Paragraph($"Total Estimado: ${orden.Total:N2}", headerFont));

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> GenerarReporteVentasPdfAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var ventas = await _context.Facturas
                .Where(f => f.FechaEmision >= fechaInicio && f.FechaEmision <= fechaFin && f.Estado == EstadoFactura.Pagada)
                .Include(f => f.Cliente)
                .OrderBy(f => f.FechaEmision)
                .ToListAsync();

            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 50, 50);
            PdfWriter.GetInstance(document, stream);
            
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            // Título
            document.Add(new Paragraph("REPORTE DE VENTAS", titleFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph($"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}", normalFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph("\n"));

            // Resumen
            var totalVentas = ventas.Sum(v => v.Total);
            var cantidadFacturas = ventas.Count;

            document.Add(new Paragraph("RESUMEN", headerFont));
            document.Add(new Paragraph($"Total Ventas: ${totalVentas:N2}", normalFont));
            document.Add(new Paragraph($"Cantidad de Facturas: {cantidadFacturas}", normalFont));
            document.Add(new Paragraph($"Promedio por Factura: ${(cantidadFacturas > 0 ? totalVentas / cantidadFacturas : 0):N2}", normalFont));
            document.Add(new Paragraph("\n"));

            // Detalle
            document.Add(new Paragraph("DETALLE DE FACTURAS", headerFont));
            foreach (var venta in ventas.Take(20))
            {
                document.Add(new Paragraph($"{venta.NumeroFactura} - {venta.Cliente.Nombre} {venta.Cliente.Apellido} - {venta.FechaEmision:dd/MM/yyyy} - ${venta.Total:N2}", normalFont));
            }

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> GenerarReporteInventarioPdfAsync()
        {
            var repuestos = await _context.Repuestos
                .OrderBy(r => r.StockActual)
                .ToListAsync();

            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 50, 50);
            PdfWriter.GetInstance(document, stream);
            
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            // Título
            document.Add(new Paragraph("REPORTE DE INVENTARIO", titleFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph("\n"));

            // Resumen
            var totalRepuestos = repuestos.Count;
            var repuestosCriticos = repuestos.Count(r => r.StockActual <= r.StockMinimo);
            var valorTotal = repuestos.Sum(r => r.StockActual * r.PrecioVenta);

            document.Add(new Paragraph("RESUMEN", headerFont));
            document.Add(new Paragraph($"Total de Repuestos: {totalRepuestos}", normalFont));
            document.Add(new Paragraph($"Repuestos con Stock Crítico: {repuestosCriticos}", normalFont));
            document.Add(new Paragraph($"Valor Total del Inventario: ${valorTotal:N2}", normalFont));
            document.Add(new Paragraph("\n"));

            // Repuestos críticos
            var criticos = repuestos.Where(r => r.StockActual <= r.StockMinimo).ToList();
            if (criticos.Any())
            {
                document.Add(new Paragraph("REPUESTOS CON STOCK CRÍTICO", headerFont));
                foreach (var repuesto in criticos)
                {
                    var estado = repuesto.StockActual == 0 ? "SIN STOCK" : "CRÍTICO";
                    document.Add(new Paragraph($"{repuesto.Nombre} - Stock: {repuesto.StockActual} - Mínimo: {repuesto.StockMinimo} - {estado}", normalFont));
                }
            }

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> GenerarReporteEmpleadosPdfAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var empleados = await _context.Empleados.ToListAsync();

            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 50, 50);
            PdfWriter.GetInstance(document, stream);
            
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            // Título
            document.Add(new Paragraph("REPORTE DE PRODUCTIVIDAD DE EMPLEADOS", titleFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph($"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}", normalFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph("\n"));

            foreach (var empleado in empleados)
            {
                var ordenesCompletadas = await _context.OrdenesTrabajo
                    .CountAsync(o => o.EmpleadoAsignadoId == empleado.EmpleadoId && 
                              o.Estado == EstadoOrden.Completada &&
                              o.FechaIngreso >= fechaInicio && 
                              o.FechaIngreso <= fechaFin);

                var ordenesActivas = await _context.OrdenesTrabajo
                    .CountAsync(o => o.EmpleadoAsignadoId == empleado.EmpleadoId && 
                              o.Estado == EstadoOrden.EnProceso);

                document.Add(new Paragraph($"{empleado.Nombre} {empleado.Apellido} ({empleado.TipoEmpleado})", headerFont));
                document.Add(new Paragraph($"Órdenes Completadas: {ordenesCompletadas}", normalFont));
                document.Add(new Paragraph($"Órdenes Activas: {ordenesActivas}", normalFont));
                document.Add(new Paragraph($"Estado: {(empleado.Activo ? "Activo" : "Inactivo")}", normalFont));
                document.Add(new Paragraph("\n"));
            }

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> GenerarReporteClientesFrecuentesPdfAsync()
        {
            var clientesFrecuentes = await _context.Clientes
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
                .ToListAsync();

            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 50, 50);
            PdfWriter.GetInstance(document, stream);
            
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            // Título
            document.Add(new Paragraph("REPORTE DE CLIENTES FRECUENTES", titleFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy}", normalFont) { Alignment = Element.ALIGN_CENTER });
            document.Add(new Paragraph("\n"));

            document.Add(new Paragraph("TOP 20 CLIENTES MÁS FRECUENTES", headerFont));
            document.Add(new Paragraph("\n"));

            foreach (var item in clientesFrecuentes)
            {
                document.Add(new Paragraph($"{item.Cliente.Nombre} {item.Cliente.Apellido}", headerFont));
                document.Add(new Paragraph($"Teléfono: {item.Cliente.Telefono}", normalFont));
                document.Add(new Paragraph($"Total de Órdenes: {item.TotalOrdenes}", normalFont));
                document.Add(new Paragraph($"Última Visita: {item.UltimaVisita:dd/MM/yyyy}", normalFont));
                document.Add(new Paragraph($"Total Gastado: ${item.TotalGastado:N2}", normalFont));
                document.Add(new Paragraph("\n"));
            }

            document.Close();
            return stream.ToArray();
        }
    }
}