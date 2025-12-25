using ResumeApp.Models.Master;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeApp.Models
{
    public enum RequirementStatus
    {
        Active = 0,
        Hold = 1,
        Closed = 2
    }
    public class ClientRequirement
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }

        [Required]
        public string JobTitle { get; set; }

        [Column("Positions")]
        public int Positions { get; set; }
        public int? JobLocationId { get; set; }

        [ForeignKey(nameof(JobLocationId))]
        public Location? JobLocationRef { get; set; }

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
        public string? BudgetNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public RequirementStatus Status { get; set; } = RequirementStatus.Active;
        public DateTime? StatusUpdatedAt { get; set; }
        public int? SpokespersonId { get; set; }

        [ForeignKey(nameof(SpokespersonId))]
        public Spokesperson? Spokesperson { get; set; }

        // notes about vendor / spokesperson
        public string? VendorNotes { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public ICollection<RequirementAssignment> RequirementAssignments { get; set; } = new List<RequirementAssignment>();

        public ICollection<RequirementSkill> RequirementSkills { get; set; } = new List<RequirementSkill>();
        public ICollection<RequirementQualification> RequirementQualifications { get; set; } = new List<RequirementQualification>();

        public ICollection<RequirementMom> MeetingNotes { get; set; } = new List<RequirementMom>();

    }
}
