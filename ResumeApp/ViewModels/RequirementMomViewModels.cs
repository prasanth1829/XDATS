using System.ComponentModel.DataAnnotations;

namespace ResumeApp.ViewModels
{
    public class RequirementMomCreateViewModel
    {
        public int RequirementId { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Meeting / MOM Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Meeting Date")]
        public DateTime? MeetingDate { get; set; }

        [Required]
        [Display(Name = "Minutes / Discussion Points")]
        public string NotesHtml { get; set; } = string.Empty;

        [Display(Name = "Attachments")]
        public List<IFormFile>? Attachments { get; set; }
    }

    public class RequirementMomEditViewModel
    {
        public int Id { get; set; }          // MOM Id
        public int RequirementId { get; set; }

        [Required]
        [MaxLength(256)]
        public string Title { get; set; } = string.Empty;

        public DateTime? MeetingDate { get; set; }

        [Required]
        public string NotesHtml { get; set; } = string.Empty;

        public List<IFormFile>? NewAttachments { get; set; }

        // To display existing attachments
        public List<string> ExistingFiles { get; set; } = new();
    }
}
