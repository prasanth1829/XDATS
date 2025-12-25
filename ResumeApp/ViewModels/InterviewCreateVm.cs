using System.ComponentModel.DataAnnotations;

namespace ResumeApp.ViewModels
{
    public class InterviewCreateVm
    {
        [Required] public int RequirementId { get; set; }
        [Required] public int ResumeId { get; set; }

        [Range(1, 99)] public int Round { get; set; } = 1;

        [Required] public DateTime StartLocal { get; set; }
        [Range(15, 480)] public int DurationMinutes { get; set; } = 60;

        [MaxLength(40)] public string? Mode { get; set; } = "Video";
        [MaxLength(500)] public string? LocationOrLink { get; set; }
        public List<string>? PanelUserIds { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
    }
}
