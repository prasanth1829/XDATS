using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;

namespace ResumeApp.Models
{
    public class Location
    {
        public int Id { get; set; }

   
        [Required, MaxLength(100)]
        public string Name { get; set; } = default!;


        [Required]
        public int CountryId { get; set; }

        public Country? Country { get; set; }
        [MaxLength(100)]
        public string? StateOrProvince { get; set; }

        [MaxLength(50)]
        public string? Timezone { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
