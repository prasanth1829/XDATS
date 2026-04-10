using ResumeApp.Models;

namespace ResumeApp.ViewModels
{
    public class AdminInProgressExportRow
    {
        public string ClientName { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public string CandidateName { get; set; } = "";
        public CandidateStatus Status { get; set; }
        public string UploadedBy { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
    }
}
