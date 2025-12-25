using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Models
{
    public class CandidateStatusHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ResumeId { get; set; }

        [Required]
        public int RequirementId { get; set; }

        [Required]
        public CandidateStatus Status { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }

        [Required]
        public string ChangedByUserId { get; set; } = default!;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
