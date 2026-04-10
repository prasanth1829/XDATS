using ResumeApp.Models;

namespace ResumeApp.ViewModels.Admin
{
    public class AdminInProgressFilterVm
    {
        public List<string> UploadedByUserIds { get; set; } = new();

        public List<int> ClientIds { get; set; } = new();

        public List<CandidateStatus> Statuses { get; set; } = new();

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool Export { get; set; } = false;
    }
}
