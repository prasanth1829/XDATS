namespace ResumeApp.ViewModels
{
    public class UserDailyWorkDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public DateTime? FirstLoginTime { get; set; }
        public DateTime? LastActivityTime { get; set; }

        // CALCULATION
        public int TotalWorkedMinutes { get; set; }

        public int UploadCount { get; set; }
        public int DownloadCount { get; set; }

        public bool IsOnline { get; set; }

    }
}
