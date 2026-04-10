namespace ResumeApp.Models
{
    public class CandidateDetail
    {
        public int Id { get; set; }

        // Mapping
        public int ResumeId { get; set; }
        public int RequirementId { get; set; }

        // Personal
        public DateTime? DateOfBirth { get; set; }
        public string? Qualification { get; set; }

        // Experience
        public decimal? TotalExperience { get; set; }
        public decimal? RelevantExperience { get; set; }

        // Compensation
        public decimal? CurrentCTC { get; set; }
        public decimal? ExpectedCTC { get; set; }
        public string? NoticePeriod { get; set; }

        // Company & Location
        public string? CurrentCompany { get; set; }
        public string? CurrentLocation { get; set; }
        public string? PreferredLocation { get; set; }

        // Notes
        public string? Remarks { get; set; }

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
