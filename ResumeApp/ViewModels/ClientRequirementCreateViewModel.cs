using System.ComponentModel.DataAnnotations;

namespace ResumeApp.ViewModels
{
    public class ClientRequirementCreateViewModel
    {
        public int ClientId { get; set; }

        // Vendor (readonly in the UI)
        public string ClientName { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactNumber { get; set; }

        // Requirement details
        [Required] public string JobTitle { get; set; }
        [Range(1, 1000)] public int Positions { get; set; } = 1;
        public string JobLocation { get; set; }      // Onsite/Remote/Hybrid
        public string EmploymentType { get; set; }   // Contract / Permanent / C2H
        public string WorkShift { get; set; }

        // Job description
        public string SkillsPrimary { get; set; }
        public string SkillsSecondary { get; set; }
        public string? SkillsRequired { get; set; }
        public string Responsibilities { get; set; }
        public int? ExperienceMin { get; set; }
        public int? ExperienceMax { get; set; }
        public string Education { get; set; }
        public string Certifications { get; set; }

        // Compensation
        public string SalaryRange { get; set; }
        public string BillingType { get; set; } // Hourly, Monthly, Fixed
        public string NoticePeriod { get; set; }

        // Timeline
        public string RequirementPriority { get; set; } // High/Medium/Low
        public DateTime? Deadline { get; set; }
        public DateTime? ExpectedJoiningDate { get; set; }

        // Additional Info
        public string ScreeningQuestions { get; set; }
        public string SpecialInstructions { get; set; }

        public IFormFileCollection Attachments { get; set; }
    }
}
