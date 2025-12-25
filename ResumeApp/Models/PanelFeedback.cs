using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Models
{
    public class PanelFeedback
    {
        [Key] public int Id { get; set; }

        [Required] public int RequirementId { get; set; }
        [Required] public int ResumeId { get; set; }
        [Required] public string PanelUserId { get; set; } = default!;

        [Required] public CandidateStatus Decision { get; set; } // PanelScreenSelected/Rejected/Hold
        [MaxLength(1000)] public string? Remark { get; set; }

        public DateTime DecidedAt { get; set; } = DateTime.UtcNow;
    }
}
