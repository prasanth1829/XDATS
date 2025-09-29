using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeApp.Models
{
    public class ClientRequirement
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }

        [Required]
        public string JobTitle { get; set; }

        [Column("Positions")]
        public int Positions { get; set; }

        public string? JobLocation { get; set; }
        public string? EmploymentType { get; set; }
        public string? WorkShift { get; set; }

        public string? SkillsPrimary { get; set; }
        public string? SkillsSecondary { get; set; }
        public string? SkillsRequired { get; set; }

        [Column("Responsibilities")]
        public string? Responsibilities { get; set; }

        public int? ExperienceMin { get; set; }
        public int? ExperienceMax { get; set; }
        public string? Education { get; set; }
        public string? Certifications { get; set; }

        public string? SalaryRange { get; set; }
        public string? BillingType { get; set; }
        public string? NoticePeriod { get; set; }

        public string? RequirementPriority { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? ExpectedJoiningDate { get; set; }

        public string? ScreeningQuestions { get; set; }
        public string? SpecialInstructions { get; set; }
        public string? AttachmentsPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
