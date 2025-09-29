using System.ComponentModel.DataAnnotations;

namespace ResumeApp.ViewModels
{
    public class SpokespersonViewModel
    {
        [Required]
        public string Name { get; set; }

        public string Designation { get; set; }

        [Phone]
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        // Changed to multiple checkboxes
        public List<string> PreferredCommunication { get; set; } = new();
    }
}
