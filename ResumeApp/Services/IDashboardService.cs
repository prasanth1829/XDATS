using ResumeApp.ViewModels;

namespace ResumeApp.Services
{
    public interface IDashboardService
    {
        Task<RecruiterDashboardViewModel> GetRecruiterDashboardAsync(string userId);

    }
}
