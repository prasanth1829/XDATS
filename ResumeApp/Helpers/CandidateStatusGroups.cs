using ResumeApp.Models;

namespace ResumeApp.Helpers
{
    public class CandidateStatusGroups
    {
        public static readonly CandidateStatus[] InProgress =
        {
            CandidateStatus.New,
            CandidateStatus.Shortlisted,
            CandidateStatus.PanelShortlisted,
            CandidateStatus.InterviewScheduled
        };
    }
}
