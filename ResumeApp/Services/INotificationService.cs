using ResumeApp.Models;

namespace ResumeApp.Services
{
    public interface INotificationService
    {
        Task NotifyAsync(IEnumerable<string> userIds, string type, string title, string? body, string? url);
        Task<int> UnreadCountAsync(string userId);
        Task<List<Notification>> RecentAsync(string userId, int take = 5);
        Task MarkAsReadAsync(string userId, int id);
        Task MarkAllAsReadAsync(string userId);
    }
}
