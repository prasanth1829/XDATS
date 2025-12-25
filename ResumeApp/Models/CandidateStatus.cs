namespace ResumeApp.Models
{
    public enum CandidateStatus
    {
        New = 0,
        Shortlisted = 1,
        Rejected = 2,
        Hold = 3,
        PanelShortlisted = 4,

        PanelScreenSelected = 5,
        PanelScreenRejected = 6,
        PanelScreenHold = 7,

        InterviewScheduled = 8,
        Selected = 9,
        OfferReleased = 10,
        Joined = 11
    }
}
