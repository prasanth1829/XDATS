using ResumeApp.ViewModels;

namespace ResumeApp.Services
{
    public interface IDashboardService
    {
        Task<RecruiterDashboardViewModel> GetRecruiterDashboardAsync(string userId);
        Task<AdminDashboardViewModel> GetAdminDashboardAsync();
        Task<ManagerDashboardViewModel> GetManagerDashboardAsync(string? filter = null);

    }
}
