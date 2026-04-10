namespace ResumeApp.ViewModels
{
    public class ManagerDashboardViewModel
    {
        public KpiSection Kpis { get; set; } = new();

        public ClientOverviewSummaryDto Summary { get; set; }
        public List<ClientOverviewRowDto> Clients { get; set; }
    }
}