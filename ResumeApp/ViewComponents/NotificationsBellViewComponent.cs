using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;

namespace ResumeApp.ViewComponents
{
    public class NotificationsBellViewComponent: ViewComponent
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Users> _userManager;

        public NotificationsBellViewComponent(ApplicationDbContext db, UserManager<Users> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(int take = 5)
        {
            var uid = _userManager.GetUserId(HttpContext.User);
            if (string.IsNullOrEmpty(uid))
            {
                ViewBag.Unread = 0;
                return View(new List<Notification>());
            }

            var unread = await _db.Notifications.Where(n => n.UserId == uid && !n.IsRead).CountAsync();
            var items = await _db.Notifications.Where(n => n.UserId == uid)
                                                .OrderByDescending(n => n.CreatedAt)
                                                .Take(take)
                                                .ToListAsync();
            ViewBag.Unread = unread;
            return View(items);
        }
    }
}
