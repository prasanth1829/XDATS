using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ResumeApp.ViewModels
{
    public class ClientOnboardingViewModel
    {
        // Company Information
        [Required]
        public string CompanyName { get; set; }

        [Required, Url]
        public string WebsiteUrl { get; set; }

        [Required]
        public string CompanyType { get; set; } // Product / Service

        [Required]
        public string CompanySize { get; set; } // 1-50, 51-200, etc.

        public string HeadquarterLocation { get; set; }
        public string OtherOfficeLocations { get; set; }

        // Primary Contact
        [Required]
        public string ContactName { get; set; }

        public string Designation { get; set; }

        [Phone]
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        // Primary contact preferred communication (single selection in model)
        [Required]
        public string PreferredCommunication { get; set; }

        // Spokespersons (dynamic list)
        public List<SpokespersonViewModel> Spokespersons { get; set; } = new();

        // Documents
        public IFormFile? NDAFile { get; set; }
        public IFormFile? MSAFile { get; set; }
        public IFormFile? CorporatePresentationFile { get; set; }

        [MaxLength(4000, ErrorMessage = "Maximum 500 words allowed.")]
        public string? CorporatePresentationText { get; set; }

        // Hiring
        public List<string> EngagementTypes { get; set; } = new();

        // Terms
        public bool AcceptTerms { get; set; }
    }
}
