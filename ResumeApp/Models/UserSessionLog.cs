using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeApp.Models
{
    public class UserSessionLog
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public Users User { get; set; }

        public DateTime LoginTime { get; set; }

        public DateTime? LogoutTime { get; set; }

        public int? SessionMinutes { get; set; }

        public bool IsAutoLogout { get; set; } = false;
        public DateTime? LastSeenAt { get; set; }

    }
}
