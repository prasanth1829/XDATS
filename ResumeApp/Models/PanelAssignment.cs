using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Models
{
    public class PanelAssignment
    {
        
            [Key] public int Id { get; set; }

            [Required] public int RequirementId { get; set; }
            [Required] public int ResumeId { get; set; }
            [Required] public string PanelUserId { get; set; } = default!;

            public string? AssignedByUserId { get; set; }
            public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        
    }
}
