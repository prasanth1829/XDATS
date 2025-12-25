using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Models
{
    public class ResumeRequirementLink
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ResumeId { get; set; }
        public Resume Resume { get; set; }

        [Required]
        public int RequirementId { get; set; }
        public ClientRequirement Requirement { get; set; }

        public string LinkedByUserId { get; set; }  // who linked it
        public Users LinkedByUser { get; set; }

        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

        // fast-read fields for TL review
        public CandidateStatus Status { get; set; } = CandidateStatus.New;
        [MaxLength(2000)]
        public string? LastComment { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // optimistic concurrency
        [Timestamp]
        public byte[]? RowVersion { get; set; }
        // Match scoring cache
        public short? MatchScore { get; set; }              // 0–100
        public string? MatchBreakdownJson { get; set; }     // JSON with matched/missing & notes
        public DateTime? LastScoredAt { get; set; }         // last compute time

    }
}
