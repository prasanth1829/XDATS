using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ResumeApp.ViewModels
{
    public class ClientOnboardingViewModel
    {
        // existing fields you already use:
        public int? ClientId { get; set; }
        public string CompanyName { get; set; } = default!;
        public string WebsiteUrl { get; set; } = default!;
        public string CompanyType { get; set; } = default!;
        public string CompanySize { get; set; } = default!;
        public string ContactName { get; set; } = default!;
        public int? DesignationId { get; set; }   // for dropdown binding
        public string? Phone { get; set; }
        public string? Email { get; set; }

        // CHANGE: allow multiple choices from checkboxes
        public List<string> PreferredCommunication { get; set; } = new();

        // you already have this as IEnumerable<string>? keep if you like:
        public IEnumerable<string>? EngagementTypes { get; set; }

        // Documents
        public IFormFile? NDAFile { get; set; }
        public IFormFile? MSAFile { get; set; }
        public IFormFile? CorporatePresentationFile { get; set; }
        public string? CorporatePresentationText { get; set; }

        // NEW: master-based locations
        public int? HeadquarterCountryId { get; set; }
        public int? HeadquarterLocationId { get; set; }
        public List<int> OtherLocationIds { get; set; } = new();

        // Dynamic spokespeople
        public List<SpokespersonItem> Spokespersons { get; set; } = new();
        public List<DocumentUploadItemVM> DocumentItems { get; set; } = new();

        public bool AcceptTerms { get; set; }
        public bool IsActive { get; set; } = true;

    }
    public class DocumentUploadItemVM
    {
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = default!;
        public bool IsMandatory { get; set; }
        public IFormFile? File { get; set; }
    }
    public class SpokespersonItem
    {
        public string Name { get; set; } = default!;
        public int? DesignationId { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public List<string> PreferredCommunication { get; set; } = new();
    }
}
