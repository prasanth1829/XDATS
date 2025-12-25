using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Models
{
    public class DocumentType
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Name { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public bool IsMandatory { get; set; } = false;

        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
