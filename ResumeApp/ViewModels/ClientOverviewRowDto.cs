namespace ResumeApp.ViewModels
{
    public class ClientOverviewRowDto
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }

        public int Openings { get; set; }
        public int Submissions { get; set; }
        public int InterviewScheduled { get; set; }
        public int Rejected { get; set; }
        public int Selected { get; set; }
        public int OfferReleased { get; set; }
        public int Joined { get; set; }
        public int Declined { get; set; }  // for future use
    }
}
