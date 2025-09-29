namespace ResumeApp.Models
{
    public class ResumeFormViewModel
    {
        public int ResumeId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }

        public string JobTitle { get; set; }
        public string PreferredJobType { get; set; }

        public int YearsOfExperience { get; set; }
        public int MonthsOfExperience { get; set; }

        public string Skills { get; set; }
        public string Experience { get; set; }

        public string ResumeFileName { get; set; }

    }
}
