using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Models.Master
{
    public class Qualification
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
    public class RequirementQualification
    {
        public int Id { get; set; }

        public int RequirementId { get; set; }
        public ClientRequirement Requirement { get; set; }

        public int QualificationId { get; set; }
        public Qualification Qualification { get; set; }
    }
}
