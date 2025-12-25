namespace ResumeApp.Models.Master
{
    public class NoticePeriodOption
    {
        public int Id { get; set; }

        // e.g. "0 – 15 days", "1 month", "2 months"
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // for ordering in dropdowns
        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
