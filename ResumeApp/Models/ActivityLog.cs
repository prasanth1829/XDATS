using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeApp.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public string ActionType { get; set; } 

        public string Message { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; }

        public DateTime Timestamp { get; set; }
    }

}
