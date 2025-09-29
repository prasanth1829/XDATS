using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeApp.Models
{
    public class Resume
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? ExtractedText { get; set; }
        public DateTime UploadedAt { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Skills { get; set; }
        public string Experience { get; set; }

        public int YearsOfExperience { get; set; }

        public string? Remark { get; set; }


        [NotMapped]
        public IFormFile? UploadFile { get; set; }

        public string? FileHash { get; set; }

        public string? UserId { get; set; }
        public Users? User { get; set; }
    }
}
