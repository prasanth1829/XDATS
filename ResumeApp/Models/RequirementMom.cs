using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ResumeApp.Models
{
    public class RequirementMom
    {
        [Key]

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }

        // FK to requirement
        public int RequirementId { get; set; }
        [ValidateNever]

        public ClientRequirement Requirement { get; set; } = null!;

        [Required]
        [MaxLength(256)]
        public string Title { get; set; } = string.Empty;

        public DateTime? MeetingDate { get; set; }
        public string? Minutes { get; set; }
     
        public string? AttachmentsPath { get; set; }

        // Audit
        public string CreatedByUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(CreatedByUserId))]
        [ValidateNever]
        public Users CreatedByUser { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? LastEditedByUserId { get; set; }

        [ForeignKey(nameof(LastEditedByUserId))]
        [ValidateNever]
        public Users? LastEditedByUser { get; set; }

        public DateTime? LastEditedAt { get; set; }

        public ICollection<RequirementMomHistory> History { get; set; } = new List<RequirementMomHistory>();
    }

    public class RequirementMomHistory
    {
        public int Id { get; set; }

        public int RequirementMomId { get; set; }
        [ValidateNever]
        public RequirementMom RequirementMom { get; set; } = null!;

        public DateTime EditedAt { get; set; } = DateTime.UtcNow;

        public string EditedByUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(EditedByUserId))]
        [ValidateNever]
        public Users EditedByUser { get; set; } = null!;

        /// <summary>
        /// Snapshot of notes before edit
        /// </summary>
        public string? NotesHtml { get; set; }

        /// <summary>
        /// Snapshot of attachments path before edit
        /// </summary>
        public string? AttachmentsPath { get; set; }
    }
}
