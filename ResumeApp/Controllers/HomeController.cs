using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeDashboardViewModel();

        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        model.TotalResumes = await _context.Resumes.CountAsync();
        model.ResumesToday = await _context.Resumes.CountAsync(r => r.UploadedAt >= today && r.UploadedAt < today.AddDays(1));
        model.ResumesThisMonth = await _context.Resumes.CountAsync(r => r.UploadedAt >= monthStart && r.UploadedAt < monthStart.AddMonths(1));

        var lastResume = await _context.Resumes.Include(r => r.User).OrderByDescending(r => r.UploadedAt).FirstOrDefaultAsync();
        if (lastResume != null)
        {
            model.LastUploadedBy = lastResume.User?.FullName ?? "Unknown";
            model.LatestUpload = lastResume.UploadedAt;
        }

        // Last 7 days
        var start = today.AddDays(-6).Date;
        var end = today.AddDays(1).Date;
        var grouped = await _context.Resumes
            .Where(r => r.UploadedAt >= start && r.UploadedAt < end)
            .GroupBy(r => new { r.UploadedAt.Year, r.UploadedAt.Month, r.UploadedAt.Day })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Day, Count = g.Count() })
            .ToListAsync();

        var labels = new List<string>();
        var counts = new List<int>();
        for (int i = 6; i >= 0; i--)
        {
            var d = today.AddDays(-i).Date;
            labels.Add(d.ToString("dd MMM"));
            var grp = grouped.FirstOrDefault(g => g.Year == d.Year && g.Month == d.Month && g.Day == d.Day);
            counts.Add(grp?.Count ?? 0);
        }

        model.Last7DaysLabels = labels;
        model.Last7DaysCounts = counts;

        return View(model);
    }
    [HttpGet]
    public async Task<IActionResult> GetUploadStats()
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var total = await _context.Resumes.CountAsync();
        var todayCount = await _context.Resumes.CountAsync(r => r.UploadedAt >= today && r.UploadedAt < today.AddDays(1));
        var monthCount = await _context.Resumes.CountAsync(r => r.UploadedAt >= monthStart && r.UploadedAt < monthStart.AddMonths(1));

        var lastResume = await _context.Resumes
            .Include(r => r.User)
            .OrderByDescending(r => r.UploadedAt)
            .FirstOrDefaultAsync();

        var start = today.AddDays(-6).Date;
        var grouped = await _context.Resumes
            .Where(r => r.UploadedAt >= start && r.UploadedAt < today.AddDays(1))
            .GroupBy(r => new { r.UploadedAt.Year, r.UploadedAt.Month, r.UploadedAt.Day })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Day, Count = g.Count() })
            .ToListAsync();

        var labels = new List<string>();
        var counts = new List<int>();
        for (int i = 6; i >= 0; i--)
        {
            var d = today.AddDays(-i).Date;
            labels.Add(d.ToString("dd MMM"));
            var grp = grouped.FirstOrDefault(g => g.Year == d.Year && g.Month == d.Month && g.Day == d.Day);
            counts.Add(grp?.Count ?? 0);
        }

        return Json(new
        {
            TotalResumes = total,
            ResumesToday = todayCount,
            ResumesThisMonth = monthCount,
            LastUploadedBy = lastResume?.User?.FullName ?? "N/A",
            LatestUpload = lastResume?.UploadedAt.ToString("dd MMM yyyy, hh:mm tt"),
            Labels = labels,
            Counts = counts
        });
    }


    [Authorize(Roles = "Admin")]
    public IActionResult AdminDashboard()
    {
        return View();
    }

    [Authorize(Roles = "Reviewer")]
    public IActionResult ReviewerDashboard()
    {
        return View();
    }
    public IActionResult ReviewerTerms()
    {
        return View();
    }
}
