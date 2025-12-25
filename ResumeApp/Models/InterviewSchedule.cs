using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Models
{
    public enum InterviewStatus
    {
        Planned = 0,
        Confirmed = 1,
        Canceled = 2,
        Completed = 3
    }
    public enum InterviewOutcome
    {
        None = 0,
        CandidateAttended = 1,
        CandidateNotAttended = 2,
        CandidatePostponed = 3,
        CandidateNotResponding = 4,
        PanelNotAttended = 5,
        PanelPostponed = 6
    }
    public class InterviewSchedule
    {
        [Key] public int Id { get; set; }

        [Required] public int RequirementId { get; set; }
        [Required] public int ResumeId { get; set; }

        [Range(1, 99)]
        public int Round { get; set; } = 1;

        [Required] public DateTime ScheduledStartUtc { get; set; }
        [Required] public DateTime ScheduledEndUtc { get; set; }

        [MaxLength(40)] public string Mode { get; set; } = "Video";
        [MaxLength(500)] public string? LocationOrLink { get; set; }

        [MaxLength(2000)]
        public string? PanelUserIdsCsv { get; set; }

        [MaxLength(1000)] public string? Notes { get; set; }

        public InterviewStatus Status { get; set; } = InterviewStatus.Planned;

        public InterviewOutcome Outcome { get; set; } = InterviewOutcome.None;
        [MaxLength(1000)] public string? OutcomeNote { get; set; }
        public DateTime? ActualStartUtc { get; set; }
        public DateTime? ActualEndUtc { get; set; }

        public string? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
