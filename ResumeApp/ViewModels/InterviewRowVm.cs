using ResumeApp.Models;

namespace ResumeApp.ViewModels
{
    public class InterviewRowVm
    {
        public int Id { get; set; }
        public int RequirementId { get; set; }
        public int ResumeId { get; set; }
        public int Round { get; set; }

        public DateTime StartLocal { get; set; }
        public DateTime EndLocal { get; set; }

        public string? CandidateName { get; set; }
        public string? CandidateEmail { get; set; }
        public string JobTitle { get; set; } = "";

        public string? Mode { get; set; }
        public string? LocationOrLink { get; set; }
        public InterviewStatus Status { get; set; }

        public InterviewOutcome Outcome { get; set; }
    }
}
