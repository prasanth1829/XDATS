using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;

namespace ResumeApp.Services
{
    public class NotificationService: INotificationService
    {
        private readonly ApplicationDbContext _db;
        public NotificationService(ApplicationDbContext db) { _db = db; }

        public async Task NotifyAsync(IEnumerable<string> userIds, string type, string title, string? body, string? url)
        {
            var ids = userIds?.Distinct().Where(s => !string.IsNullOrWhiteSpace(s)).ToList()
                      ?? new List<string>();
            if (ids.Count == 0) return;

            var now = DateTime.UtcNow;
            var items = ids.Select(u => new Notification
            {
                UserId = u,
                Type = type,
                Title = title,
                Body = body,
                Url = url,
                CreatedAt = now,
                IsRead = false
            });

            await _db.Notifications.AddRangeAsync(items);
            await _db.SaveChangesAsync();
        }

        public Task<int> UnreadCountAsync(string userId) =>
            _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).CountAsync();

        public Task<List<Notification>> RecentAsync(string userId, int take = 5) =>
            _db.Notifications.Where(n => n.UserId == userId)
                             .OrderByDescending(n => n.CreatedAt)
                             .Take(take)
                             .ToListAsync();

        public async Task MarkAsReadAsync(string userId, int id)
        {
            var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (n == null) return;
            n.IsRead = true;
            await _db.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await _db.Notifications
                     .Where(n => n.UserId == userId && !n.IsRead)
                     .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRead, true));
        }
    }
}
