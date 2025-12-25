namespace ResumeApp.ViewModels
{
    public class ClientShareRowVm
    {
        public int ResumeId { get; set; }

        // Branded header context
        public string? ClientName { get; set; }
        public string? Skill { get; set; }    
        public DateTime? RowDate { get; set; }

        // Candidate basics
        public string Candidate { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }

        // Extra fields (often not in DB yet—leave null if unknown)
        public DateTime? DateOfBirth { get; set; }
        public string? Qualification { get; set; }
        public decimal? TotalYearsExp { get; set; }      
        public decimal? RelevantYearsExp { get; set; }   
        public string? CurrentCTC { get; set; }
        public string? ExpectedCTC { get; set; }
        public string? NoticePeriod { get; set; }
        public string? CurrentCompany { get; set; }
        public string? CurrentLocation { get; set; }
        public string? PreferredLocation { get; set; }
        public string? Source { get; set; }              

        // Convenience
        public string? ResumeLink { get; set; }
        public string? KeySkills { get; set; }
        public int? MatchScore { get; set; }
    }
}
