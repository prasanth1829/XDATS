using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.ViewModels;

namespace ResumeApp.Services
{
    public class UserDailyWorkService
    {
        private readonly ApplicationDbContext _context;

        public UserDailyWorkService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDailyWorkDto>> GetTodayWorkAsync()
        {
            var (todayStart, todayEnd) = TimeHelper.GetTodayUtcRange();
            var now = TimeHelper.UtcNow;

            var sessions = await _context.UserSessionLogs
                .Where(s =>
                    s.LoginTime < todayEnd &&
                    (s.LogoutTime ?? now) > todayStart)
                .ToListAsync();

            var onlineUserIds = await _context.UserSessionLogs
                .Where(s =>
                    s.LogoutTime == null &&
                    s.LastSeenAt != null &&
                    s.LastSeenAt >= now.AddMinutes(-5))
                .Select(s => s.UserId)
                .Distinct()
                .ToListAsync();

            var grouped = sessions.GroupBy(s => s.UserId);

            var uploads = await _context.ActivityLogs
                .Where(a => a.ActionType == "UPLOAD_RESUME" && a.Timestamp >= todayStart)
                .GroupBy(a => a.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var downloads = await _context.ActivityLogs
                .Where(a => a.ActionType == "DOWNLOAD_RESUME" && a.Timestamp >= todayStart)
                .GroupBy(a => a.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var users = await _context.Users
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();

            var result = new List<UserDailyWorkDto>();

            foreach (var u in users)
            {
                if (!grouped.Any(g => g.Key == u.Id))
                    continue;

                var userSessions = grouped.First(g => g.Key == u.Id);

                int totalMinutes = 0;
                DateTime? firstLogin = null;
                DateTime? lastActivity = null;

                foreach (var s in userSessions)
                {
                    // Work calculation (clipped)
                    var workStart = s.LoginTime < todayStart ? todayStart : s.LoginTime;
                    var workEnd = s.LogoutTime ?? s.LastSeenAt ?? now;

                    if (workEnd <= workStart) continue;

                    totalMinutes += (int)(workEnd - workStart).TotalMinutes;

                    // ✅ REAL FIRST LOGIN (NOT CLIPPED)
                    if (s.LoginTime >= todayStart && s.LoginTime < todayEnd)
                    {
                        if (firstLogin == null || s.LoginTime < firstLogin)
                            firstLogin = s.LoginTime;
                    }

                    // Last activity
                    if (lastActivity == null || workEnd > lastActivity)
                        lastActivity = workEnd;
                }


                uploads.TryGetValue(u.Id, out var up);
                downloads.TryGetValue(u.Id, out var down);

                result.Add(new UserDailyWorkDto
                {
                    UserId = u.Id,
                    UserName = u.FullName,
                    FirstLoginTime = firstLogin,
                    LastActivityTime = lastActivity,
                    TotalWorkedMinutes = totalMinutes,
                    UploadCount = up,
                    DownloadCount = down,
                    IsOnline = onlineUserIds.Contains(u.Id)
                });
            }

            return result.OrderByDescending(x => x.TotalWorkedMinutes).ToList();
        }
        public async Task<UserDailyWorkDto?> GetMyTodayWorkAsync(string userId)
        {
            var (todayStart, todayEnd) = TimeHelper.GetTodayUtcRange();
            var now = TimeHelper.UtcNow;

            var sessions = await _context.UserSessionLogs
                .Where(s =>
                    s.UserId == userId &&
                    s.LoginTime < todayEnd &&
                    (s.LogoutTime ?? now) > todayStart)
                .ToListAsync();

            if (!sessions.Any())
                return null;

            int totalMinutes = 0;
            DateTime? firstLogin = null;
            DateTime? lastActivity = null;

            foreach (var s in sessions)
            {
                var workStart = s.LoginTime < todayStart ? todayStart : s.LoginTime;
                var workEnd = s.LogoutTime ?? s.LastSeenAt ?? now;

                if (workEnd <= workStart) continue;

                totalMinutes += (int)(workEnd - workStart).TotalMinutes;

                // ✅ REAL FIRST LOGIN TODAY
                if (s.LoginTime >= todayStart && s.LoginTime < todayEnd)
                {
                    if (firstLogin == null || s.LoginTime < firstLogin)
                        firstLogin = s.LoginTime;
                }

                if (lastActivity == null || workEnd > lastActivity)
                    lastActivity = workEnd;
            }


            var uploads = await _context.ActivityLogs.CountAsync(a =>
                a.UserId == userId &&
                a.ActionType == "UPLOAD_RESUME" &&
                a.Timestamp >= todayStart);

            var downloads = await _context.ActivityLogs.CountAsync(a =>
                a.UserId == userId &&
                a.ActionType == "DOWNLOAD_RESUME" &&
                a.Timestamp >= todayStart);

            var isOnline = await _context.UserSessionLogs.AnyAsync(s =>
                s.UserId == userId &&
                s.LogoutTime == null &&
                s.LastSeenAt >= now.AddMinutes(-5));

            return new UserDailyWorkDto
            {
                UserId = userId,
                FirstLoginTime = firstLogin,
                LastActivityTime = lastActivity,
                TotalWorkedMinutes = totalMinutes,
                UploadCount = uploads,
                DownloadCount = downloads,
                IsOnline = isOnline
            };
        }


        public async Task<List<UserDailyWorkDto>> GetWorkByDateRangeAsync(DateTime fromDateUtc,DateTime toDateUtc)
        {
            var rangeStart = fromDateUtc.Date;
            var rangeEnd = toDateUtc.Date.AddDays(1);

            var sessions = await _context.UserSessionLogs
                .Where(s =>
                    s.LoginTime < rangeEnd &&
                    (s.LogoutTime ?? DateTime.UtcNow) > rangeStart)
                .ToListAsync();

            var sessionByUser = sessions
                .GroupBy(s => s.UserId)
                .Select(g =>
                {
                    int totalMinutes = 0;
                    DateTime? firstLogin = null;
                    DateTime? lastLogout = null;

                    foreach (var s in g)
                    {
                        if (s.LogoutTime != null && s.LogoutTime < s.LoginTime)
                            continue;

                        var start = s.LoginTime < rangeStart ? rangeStart : s.LoginTime;
                        var end = (s.LogoutTime ?? DateTime.UtcNow) > rangeEnd
                            ? rangeEnd
                            : (s.LogoutTime ?? DateTime.UtcNow);

                        if (end <= start) continue;

                        totalMinutes += (int)(end - start).TotalMinutes;

                        if (firstLogin == null || start < firstLogin)
                            firstLogin = start;

                        if (lastLogout == null || end > lastLogout)
                            lastLogout = end;
                    }

                    return new
                    {
                        UserId = g.Key,
                        FirstLogin = firstLogin,
                        LastLogout = lastLogout,
                        TotalMinutes = totalMinutes
                    };
                })
                .ToDictionary(x => x.UserId);

            var uploads = await _context.ActivityLogs
                .Where(a =>
                    a.ActionType == "UPLOAD_RESUME" &&
                    a.Timestamp >= rangeStart &&
                    a.Timestamp < rangeEnd)
                .GroupBy(a => a.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var downloads = await _context.ActivityLogs
                .Where(a =>
                    a.ActionType == "DOWNLOAD_RESUME" &&
                    a.Timestamp >= rangeStart &&
                    a.Timestamp < rangeEnd)
                .GroupBy(a => a.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var users = await _context.Users
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();

            var result = new List<UserDailyWorkDto>();

            foreach (var u in users)
            {
                if (!sessionByUser.TryGetValue(u.Id, out var s))
                    continue;

                uploads.TryGetValue(u.Id, out var up);
                downloads.TryGetValue(u.Id, out var down);

                result.Add(new UserDailyWorkDto
                {
                    UserId = u.Id,
                    UserName = u.FullName,
                    FirstLoginTime = s.FirstLogin,
                    LastActivityTime = s.LastLogout,
                    TotalWorkedMinutes = s.TotalMinutes,
                    UploadCount = up,
                    DownloadCount = down
                });
            }

            return result.OrderByDescending(x => x.TotalWorkedMinutes).ToList();
        }


        public async Task<List<UserDailyWorkDto>> GetTodayWorkLiveAsync()
        {
            return await GetTodayWorkAsync();
        }



    }
}
