namespace ResumeApp.ViewModels
{
    public class AdminDashboardViewModel
    {
        public KpiSection Kpis { get; set; } = new();
        public PipelineSection Pipeline { get; set; } = new();
        public List<BarChartItem> PipelineChart { get; set; } = new();
        public List<RecruiterPerformanceRow> RecruiterPerformance { get; set; } = new();
        public List<ActivityItem> RecentActivities { get; set; } = new();
        public AdminQuickCountsVm QuickCounts { get; set; } = new();
        public List<AdminAlertVm> Alerts { get; set; } = new();
        public List<ClientPerformanceRow> ClientPerformance { get; set; } = new();



    }
    // ================= KPI =================
    public class KpiSection
    {
        public int TotalClients { get; set; }
        public int TotalRequirements { get; set; }
        public int ActiveRequirements { get; set; }
        public int TotalResumes { get; set; }
        public int ProfilesInProgress { get; set; }
        public int InterviewsScheduled { get; set; }
    }

    // ================= PIPELINE =================
    public class PipelineSection
    {
        public int New { get; set; }
        public int Shortlisted { get; set; }
        public int PanelShortlisted { get; set; }
        public int InterviewScheduled { get; set; }
        public int Selected { get; set; }
        public int OfferReleased { get; set; }
        public int Joined { get; set; }
        public int Hold { get; set; }
    }

    // ================= CHART =================
    public class BarChartItem
    {
        public string Label { get; set; } = "";
        public int Count { get; set; }
    }

    // ================= RECRUITER PERFORMANCE =================
    public class RecruiterPerformanceRow
    {
        public string RecruiterName { get; set; } = "";
        public int Uploads { get; set; }
        public int Shortlisted { get; set; }
        public int Interviews { get; set; }
    }

    // ================= ACTIVITY FEED =================
    public class ActivityItem
    {
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
    public class AdminQuickCountsVm
    {
        public int TotalUsers { get; set; }
        public int TotalResumes { get; set; }
        public int UploadsToday { get; set; }
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public int ActiveRequirements { get; set; }
    }
    public class AdminAlertVm
    {
        public string Type { get; set; } = "info"; // danger | warning | info
        public string Message { get; set; }
        public string? ActionUrl { get; set; }
    }
    public class ClientPerformanceRow
    {
        public string ClientName { get; set; } = "";
        public int Openings { get; set; }
        public int Submissions { get; set; }
        public int Pipeline { get; set; }
        public int InterviewScheduled { get; set; }
        public int OfferReleased { get; set; }
        public int Joined { get; set; }
        public int Declined { get; set; }
    }

}
