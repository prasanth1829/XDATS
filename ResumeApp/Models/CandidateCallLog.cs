namespace ResumeApp.Models
{
    public class CandidateCallLog
    {
        public int Id { get; set; }

        public int ResumeId { get; set; }
        public int RequirementId { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime CallStartTime { get; set; }
        public DateTime? CallEndTime { get; set; }

        public string? Outcome { get; set; }
        public string? Notes { get; set; }

        public string CalledByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
