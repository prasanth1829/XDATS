using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Models
{
    public class Country
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = default!;   // e.g., "India"

        [MaxLength(3)]
        public string? IsoCode { get; set; }           // e.g., "IN"

        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
