using DocumentFormat.OpenXml.Office.CustomUI;
using ResumeApp.Models;

namespace ResumeApp.ViewModels
{
    public class RecruiterDashboardViewModel
    {
        public string RecruiterName { get; set; }

        // KPI cards
        public int AssignedJdCount { get; set; }
        public int UploadsLast30Days { get; set; }
        public int InReviewCount { get; set; }
        public int SelectedCount { get; set; }

        public int ProfileSubmittedCount { get; set; }
        public int PipelineCount { get; set; }
        public int ScreeningPendingCount { get; set; }
        public int InterviewScheduledCount { get; set; }

        public int FinalSelectCount { get; set; }
        public int OfferReleasedCount { get; set; }
        public int JoinedCount { get; set; }
        public int FollowUpCount { get; set; }
        public RecruitmentFunnelVm Funnel { get; set; }

        public List<DropdownVm> ClientFilters { get; set; }
        public List<DropdownVm> RequirementFilters { get; set; }

        // Tables & charts
        public Dictionary<CandidateStatus, int> StatusCounts { get; set; } = new();

        public List<MyRecruitmentRowVm> MyRecruitments { get; set; } = new();
        public List<RecentUploadVm> RecentUploads { get; set; } = new();
        public List<ActionItemVm> ActionItems { get; set; } = new();
        public List<QuickActionVm> QuickActions { get; set; } = new();

    }

    public class MyRecruitmentRowVm
    {
        public int RequirementId { get; set; }
        public string ClientName { get; set; }
        public string JobTitle { get; set; }
        public int Positions { get; set; }
        public int UploadedCount { get; set; }
        public string Status { get; set; }
    }

    public class RecentUploadVm
    {
        public int RequirementId { get; set; }
        public int ResumeId { get; set; }

        public string CandidateName { get; set; }
        public string JobTitle { get; set; }

        public DateTime UploadedAt { get; set; }
        public CandidateStatus Status { get; set; }

        public short? MatchScore { get; set; }
    }

    public class ActionItemVm
    {
        public string Type { get; set; }   
        public string Message { get; set; }
    }
    public class QuickActionVm
    {
        public string Key { get; set; }     
        public string Title { get; set; }     
        public int Count { get; set; }        
        public string BadgeType { get; set; } 
    }
    public class RecruitmentFunnelVm
    {
        public int ProfileSubmitted { get; set; }
        public int Selected { get; set; }
        public int InterviewScheduled { get; set; }
        public int OfferReleased { get; set; }
        public int Joined { get; set; }
    }
    public class DropdownVm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
    }


}
