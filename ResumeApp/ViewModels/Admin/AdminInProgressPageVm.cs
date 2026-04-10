using ResumeApp.Models;

namespace ResumeApp.ViewModels.Admin
{
    public class AdminInProgressPageVm
    {
        
        public AdminInProgressFilterVm Filter { get; set; } = new();
        
        public List<AdminInProgressVm> Results { get; set; } = new();

        public List<SimpleUserVm> Recruiters { get; set; } = new();

        // Clients list
        public List<SimpleClientVm> Clients { get; set; } = new();

        // Status options
        public List<StatusOptionVm> StatusOptions { get; set; } = new();
    }
    public class SimpleUserVm
    {
        public string UserId { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class SimpleClientVm
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = "";
    }

    public class StatusOptionVm
    {
        public CandidateStatus Status { get; set; }
        public string Label { get; set; } = "";
    }
}
