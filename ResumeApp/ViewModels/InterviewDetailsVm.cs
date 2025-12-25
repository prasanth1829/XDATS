using ResumeApp.Models;

namespace ResumeApp.ViewModels
{
    public class InterviewDetailsVm
    {
        public int Id { get; set; }
        public int RequirementId { get; set; }
        public string JobTitle { get; set; } = "";
        public int ResumeId { get; set; }
        public string CandidateName { get; set; } = "";
        public string? CandidateEmail { get; set; }

        public int Round { get; set; }
        public DateTime StartLocal { get; set; }
        public DateTime EndLocal { get; set; }
        public string? Mode { get; set; }
        public string? LocationOrLink { get; set; }
        public string? Notes { get; set; }
        public InterviewStatus Status { get; set; }

        public InterviewOutcome Outcome { get; set; }
        public string? OutcomeNote { get; set; }
        public DateTime? ActualStartLocal { get; set; }
        public DateTime? ActualEndLocal { get; set; }

        public List<(string UserId, string Display)> Panelists { get; set; } = new();
        public List<(string PanelDisplay, InterviewFeedbackDecision Decision, int? Tech, int? Comm, int? Culture, string? Comments, DateTime When)> Feedback { get; set; } = new();
    }
}
