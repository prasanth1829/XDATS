using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;
using ResumeApp.ViewModels;

namespace ResumeApp.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Users> _userManager;

        public NotificationsController(ApplicationDbContext db, UserManager<Users> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Optional full page list
        public async Task<IActionResult> Index()
        {
            var uid = _userManager.GetUserId(User);
            var items = await _db.Notifications.Where(n => n.UserId == uid)
                                               .OrderByDescending(n => n.CreatedAt)
                                               .ToListAsync();
            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            var uid = _userManager.GetUserId(User);
            var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
            if (n != null)
            {
                n.IsRead = true;
                await _db.SaveChangesAsync();
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            var uid = _userManager.GetUserId(User);
            await _db.Notifications.Where(n => n.UserId == uid && !n.IsRead)
                                   .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRead, true));
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // Optional JSON endpoint for polling
        [HttpGet]
        public async Task<IActionResult> Latest(int take = 5)
        {
            var uid = _userManager.GetUserId(User);
            var unread = await _db.Notifications.Where(n => n.UserId == uid && !n.IsRead).CountAsync();
            var items = await _db.Notifications.Where(n => n.UserId == uid)
                                                .OrderByDescending(n => n.CreatedAt)
                                                .Take(take)
                                                .Select(n => new { n.Id, n.Title, n.Body, n.Url, n.IsRead, n.CreatedAt })
                                                .ToListAsync();
            return Json(new { unread, items });
        }
    }
}
