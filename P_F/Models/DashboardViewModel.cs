namespace P_F.Models
{
    public class DashboardViewModel
    {
        // Métricas principales
        public int TotalClientes { get; set; }
        public int TotalVehiculos { get; set; }
        public int TotalEmpleados { get; set; }
        public int OrdenesHoy { get; set; }
        public int OrdenesPendientes { get; set; }
        public int OrdenesEnProceso { get; set; }

        // Ventas
        public decimal VentasDelMes { get; set; }
        public decimal VentasDelMesAnterior { get; set; }
        public decimal CrecimientoVentas { get; set; }

        // Inventario
        public int RepuestosBajoStock { get; set; }

        // Datos para gráficos
        public List<VentaDiaria> VentasUltimos7Dias { get; set; } = new();
        public List<EstadisticaEmpleado> EstadisticasEmpleados { get; set; } = new();
        public List<RepuestoCritico> TopRepuestosCriticos { get; set; } = new();
    }

    public class VentaDiaria
    {
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
    }

    public class EstadisticaEmpleado
    {
        public string Nombre { get; set; } = string.Empty;
        public int OrdenesActivas { get; set; }
        public int OrdenesCompletadas { get; set; }
    }

    public class RepuestoCritico
    {
        public string Nombre { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
    }
}