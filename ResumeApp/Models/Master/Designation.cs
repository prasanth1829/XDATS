namespace ResumeApp.Models
{
    public class Designation
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;   // e.g., "HR Manager"

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
