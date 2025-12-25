using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Models
{
    public class InterviewFeedback
    {
        [Key] public int Id { get; set; }

        [Required] public int InterviewScheduleId { get; set; }  // FK -> InterviewSchedule.Id
        [Required] public int RequirementId { get; set; }
        [Required] public int ResumeId { get; set; }
        [Required] public int Round { get; set; }

        [Required] public string PanelUserId { get; set; } = default!;

        [Required] public InterviewFeedbackDecision Decision { get; set; }
        [MaxLength(2000)] public string? Comments { get; set; }

        // Optional simple scoring
        public int? TechScore { get; set; }     // 1..10
        public int? CommScore { get; set; }     // 1..10
        public int? CultureScore { get; set; }  // 1..10

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
