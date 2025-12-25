using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Models
{
    public class Notification
    {
        [Key] public int Id { get; set; }

        [Required] public string UserId { get; set; } = default!;
        [Required, MaxLength(40)] public string Type { get; set; } = "Info"; // Assignment, Upload, StatusChange...
        [Required, MaxLength(200)] public string Title { get; set; } = "";
        [MaxLength(1000)] public string? Body { get; set; }
        [MaxLength(500)] public string? Url { get; set; }   // deep link back

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
