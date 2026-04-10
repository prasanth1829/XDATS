namespace ResumeApp.ViewModels
{
    public class ClientOverviewSummaryDto
    {
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public int InactiveClients { get; set; }

        public int TotalOpenings { get; set; }
        public int ActiveRequirements { get; set; }

        public int TotalSubmissions { get; set; }
        public int TotalInterviewScheduled { get; set; }
        public int TotalRejected { get; set; }
        public int TotalSelected { get; set; }
        public int TotalOfferReleased { get; set; }
        public int TotalJoined { get; set; }
        public int TotalDeclined { get; set; }
    }
}
