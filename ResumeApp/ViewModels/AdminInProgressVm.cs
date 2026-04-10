using ResumeApp.Models;

namespace ResumeApp.ViewModels
{
    public class AdminInProgressVm
    {
        public int RequirementId { get; set; }
        public string JobTitle { get; set; } = "";
        public string ClientName { get; set; } = "";
        public int TotalProfiles { get; set; }

        public List<AdminInProgressProfileRow> Profiles { get; set; } = new();
    }

    public class AdminInProgressProfileRow
    {
        public int ResumeId { get; set; }
        public string CandidateName { get; set; } = "";
        public CandidateStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UploadedBy { get; set; } = "";
        public string? UploadedByUserId { get; set; }
    }
}
