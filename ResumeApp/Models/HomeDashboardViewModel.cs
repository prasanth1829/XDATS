using System;

namespace ResumeApp.Models
{
    public class HomeDashboardViewModel
    {
        public int TotalResumes { get; set; }
        public int ResumesToday { get; set; }
        public int ResumesThisMonth { get; set; }
        public string LastUploadedBy { get; set; } = "N/A";
        public DateTime? LatestUpload { get; set; }

        public List<string> Last7DaysLabels { get; set; } = new();
        public List<int> Last7DaysCounts { get; set; } = new();
    }
}
