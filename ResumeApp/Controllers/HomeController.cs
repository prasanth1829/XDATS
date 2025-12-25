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

    [Authorize]
    public IActionResult Index()
    {
        if (User.IsInRole("Reviewer"))
            return RedirectToAction("Reviewer", "Dashboard");

        if (User.IsInRole("Admin"))
            return View("AdminDashboard");

        if (User.IsInRole("Team Lead"))
            return RedirectToAction("TeamLead", "Dashboard");

        if (User.IsInRole("Manager"))
            return RedirectToAction("Manager", "Dashboard");

        if (User.IsInRole("Vendor"))
            return RedirectToAction("Vendor", "Dashboard");

        if (User.IsInRole("Panel"))
            return RedirectToAction("Panel", "Dashboard");

        return Forbid();
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
        // Redirect old URL to new SaaS dashboard
        return RedirectToAction("Reviewer", "Dashboard");
    }
    public IActionResult ReviewerTerms()
    {
        return View();
    }
}
