using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ResumeApp.ViewModels
{
    public class ClientRequirementCreateViewModel
    {
        public int ClientId { get; set; }

        public string? ClientName { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactNumber { get; set; }
      
        [Display(Name = "Spokesperson")]
        public int? SelectedSpokespersonId { get; set; }

        public IEnumerable<SelectListItem> SpokespersonList { get; set; } = Enumerable.Empty<SelectListItem>();

        [Display(Name = "Vendor / Contact Notes")]
        public string? VendorNotes { get; set; }
        // Requirement details
        [Required] 
        public string JobTitle { get; set; } = string.Empty;

        [Range(1, 1000)] 
        public int Positions { get; set; } = 1;
        
        //Selected location FK
        [Display(Name = "Job Location")]
        public int? JobLocationId { get; set; }

        // Locations to bind in the dropdown
        public IEnumerable<SelectListItem> ClientLocations { get; set; } = Enumerable.Empty<SelectListItem>();
        public string? EmploymentType { get; set; }   // Contract / Permanent / C2H
        public string? WorkShift { get; set; }

        // Job description
        public string? SkillsPrimary { get; set; }
        public string? SkillsSecondary { get; set; }
        public string? SkillsRequired { get; set; }
        public string? Responsibilities { get; set; }
        public int? ExperienceMin { get; set; }
        public int? ExperienceMax { get; set; }
        public string? Education { get; set; }
        public string? Certifications { get; set; }
        public List<int> SelectedQualificationIds { get; set; } = new();
        public IEnumerable<SelectListItem> QualificationOptions { get; set; } = Enumerable.Empty<SelectListItem>();


        // Compensation
        public string? SalaryRange { get; set; }
        public string? BillingType { get; set; }

        public List<int> SelectedNoticePeriodOptionIds { get; set; } = new List<int>();
        public IEnumerable<SelectListItem> NoticePeriodOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        public string? BudgetNote { get; set; }


        // Timeline
        public string? RequirementPriority { get; set; } // High/Medium/Low
        public DateTime? Deadline { get; set; }
        public DateTime? ExpectedJoiningDate { get; set; }

        // Additional Info
        public string? ScreeningQuestions { get; set; }
        public string? SpecialInstructions { get; set; }

        public List<int> PrimarySkillIds { get; set; } = new();
        public List<int> SecondarySkillIds { get; set; } = new();
        public IFormFileCollection? Attachments { get; set; }
    }
}
