using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using System.Security.Claims;

namespace ResumeApp.Middleware
{
    public class UserActivityMiddleware
    {
        private readonly RequestDelegate _next;

        public UserActivityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userId))
                {
                    var session = await db.UserSessionLogs
                        .Where(s => s.UserId == userId && s.LogoutTime == null)
                        .OrderByDescending(s => s.LoginTime)
                        .FirstOrDefaultAsync();

                    if (session != null)
                    {
                        session.LastSeenAt = DateTime.UtcNow;
                        await db.SaveChangesAsync();
                    }
                }
            }

            await _next(context);
        }
    }
}
