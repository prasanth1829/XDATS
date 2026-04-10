using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;
using ResumeApp.Services;
using ResumeApp.ViewModels;
using System.Security.Claims;

namespace ResumeApp.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<Users> userManager,
            IDashboardService dashboardService)
        {
            _context = context;
            _userManager = userManager;
            _dashboardService = dashboardService;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin([FromServices] UserDailyWorkService dailyWorkService)
        {
            var vm = await _dashboardService.GetAdminDashboardAsync();
            ViewBag.TodayWork = await dailyWorkService.GetTodayWorkAsync();

            return View(vm); // Views/Dashboard/Admin.cshtml
        }

        [Authorize(Roles = "Reviewer")]
        public async Task<IActionResult> Reviewer([FromServices] UserDailyWorkService workService)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var vm = await _dashboardService.GetRecruiterDashboardAsync(userId);

            ViewBag.MyTodayWork = await workService.GetMyTodayWorkAsync(userId);

            return View(vm); // Views/Dashboard/Reviewer.cshtml
        }


        [Authorize(Roles = "Team Lead")]
        public IActionResult TeamLead()
        {
            return View(); // placeholder for TL dashboard
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Manager(string? filter)
        {
            var model = await _dashboardService.GetManagerDashboardAsync(filter);
            return View(model);
        }

        [Authorize(Roles = "Vendor")]
        public IActionResult Vendor()
        {
            return View();
        }

        [Authorize(Roles = "Panel")]
        public IActionResult Panel()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Reviewer,Admin,Team Lead,Manager")]
        public async Task<IActionResult> GetRecruitmentFunnel(
            DateTime? fromDate,
            DateTime? toDate,
            int? clientId,
            int? requirementId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.ResumeRequirementLinks
                .AsNoTracking()
                .Where(l => l.LinkedByUserId == userId);

            if (fromDate.HasValue)
                query = query.Where(l => l.LinkedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.LinkedAt <= toDate.Value);

            if (requirementId.HasValue)
                query = query.Where(l => l.RequirementId == requirementId.Value);

            if (clientId.HasValue)
                query = query.Where(l => l.Requirement.ClientId == clientId.Value);

            var funnel = new RecruitmentFunnelVm
            {
                ProfileSubmitted = await query.CountAsync(l =>
                    l.Status == CandidateStatus.New ||
                    l.Status == CandidateStatus.Shortlisted),

                InterviewScheduled = await query.CountAsync(l =>
                    l.Status == CandidateStatus.InterviewScheduled),

                Selected = await query.CountAsync(l =>
                    l.Status == CandidateStatus.Selected),

                OfferReleased = await query.CountAsync(l =>
                    l.Status == CandidateStatus.OfferReleased),

                Joined = await query.CountAsync(l =>
                    l.Status == CandidateStatus.Joined)
            };

            return Json(funnel);
        }

        public async Task<IActionResult> TodayWork([FromServices] UserDailyWorkService service)
        {
            var data = await service.GetTodayWorkAsync();
            return View(data);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> WorkReport(DateTime? fromDate, DateTime? toDate, [FromServices] UserDailyWorkService service)
        {
            var from = (fromDate ?? DateTime.UtcNow).Date;
            var to = (toDate ?? DateTime.UtcNow).Date;

            var data = await service.GetWorkByDateRangeAsync(from, to);

            ViewBag.FromDate = from.ToLocalTime().ToString("yyyy-MM-dd");
            ViewBag.ToDate = to.ToLocalTime().ToString("yyyy-MM-dd");

            return View(data);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportWorkReport(DateTime? fromDate, DateTime? toDate, [FromServices] UserDailyWorkService service)
        {
            var from = (fromDate ?? DateTime.UtcNow).Date;
            var to = (toDate ?? DateTime.UtcNow).Date;

            var data = await service.GetWorkByDateRangeAsync(from, to);

            using var package = new OfficeOpenXml.ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Work Report");

            string[] headers = { "User", "First Login", "Last Logout", "Worked Minutes", "Uploads", "Downloads" };

            for (int i = 0; i < headers.Length; i++)
                ws.Cells[1, i + 1].Value = headers[i];

            int row = 2;
            foreach (var u in data)
            {
                ws.Cells[row, 1].Value = u.UserName;
                ws.Cells[row, 2].Value = u.FirstLoginTime?.ToLocalTime().ToString("g");
                ws.Cells[row, 3].Value = u.LastActivityTime?.ToLocalTime().ToString("g");
                ws.Cells[row, 4].Value = u.TotalWorkedMinutes;
                ws.Cells[row, 5].Value = u.UploadCount;
                ws.Cells[row, 6].Value = u.DownloadCount;
                row++;
            }

            return File(
                package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"UserWorkReport_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
        }

        
        [Authorize(Roles = "Reviewer")]
        [HttpGet]
        public async Task<IActionResult> MyWorkLive([FromServices] UserDailyWorkService workService)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var data = await workService.GetMyTodayWorkAsync(userId);
            return Json(data);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AdminWorkLive([FromServices] UserDailyWorkService workService)
        {
            var data = await workService.GetTodayWorkLiveAsync();
            return Json(data);
        }



    }
}