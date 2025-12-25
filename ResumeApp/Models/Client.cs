using ResumeApp.Models;
using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Models
{
    public class Client
    {
        public int Id { get; set; }

        // Company info
        public string CompanyName { get; set; }
        public string WebsiteUrl { get; set; }
        public string CompanyType { get; set; }
        public string CompanySize { get; set; }

        // Primary Contact
        public string ContactName { get; set; }
        public string Designation { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string PreferredCommunication { get; set; }

        // Hiring
        public string EngagementTypes { get; set; }

        // Terms
        public bool AcceptTerms { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;     // toggle visibility without delete
        public bool IsDeleted { get; set; } = false;   // soft delete
        public int? HeadquarterCountryId { get; set; }
        public Country? HeadquarterCountry { get; set; }

        public int? HeadquarterLocationId { get; set; }
        public Location? HeadquarterLocation { get; set; }

        // Navigation
        public ICollection<ClientOtherLocation> OtherLocationsMap { get; set; } = new List<ClientOtherLocation>();

        public ICollection<Spokesperson> Spokespersons { get; set; }
        public ICollection<ClientDocument> Documents { get; set; }
        public ICollection<ClientDocumentItem> ClientDocumentItems { get; set; } = new List<ClientDocumentItem>();

    }

    public class Spokesperson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string PreferredCommunication { get; set; }

        // FK
        public int ClientId { get; set; }
        public Client Client { get; set; }
    }

    public class ClientDocument
    {
        public int Id { get; set; }
        public string? NDAPath { get; set; }
        public string? MSAPath { get; set; }
        public string? CorporatePresentationPath { get; set; }
        public string? CorporatePresentationText { get; set; }

        // FK
        public int ClientId { get; set; }
        public Client Client { get; set; }
    }
}
