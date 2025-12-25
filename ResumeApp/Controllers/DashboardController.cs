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
        [Authorize(Roles = "Reviewer")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var vm = await _dashboardService.GetRecruiterDashboardAsync(userId);

            return View("Reviewer", vm);
        }


        // Recruiter/Reviewer Dashboard -> My Assigned Requirements
        public async Task<IActionResult> MyAssignments()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            var assignments = await _context.RequirementAssignments
                .Include(a => a.Requirement)
                .ThenInclude(r => r.Client)
                .Where(a => a.UserId == userId)
                .Select(a => a.Requirement)
                .ToListAsync();

            return View(assignments);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }

        [Authorize(Roles = "Team Lead")]
        public IActionResult TeamLead()
        {
            return View();
        }

        [Authorize(Roles = "Manager")]
        public IActionResult Manager()
        {
            return View();
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

        [Authorize(Roles = "Reviewer")]
        public async Task<IActionResult> Reviewer()
        {
            var user = await _userManager.GetUserAsync(User);
            var vm = await _dashboardService.GetRecruiterDashboardAsync(user.Id);
            return View(vm);
        }
        [HttpGet]
        [Authorize(Roles = "Reviewer,Admin,Team Lead,Manager")]
        public async Task<IActionResult> GetRecruitmentFunnel(
        DateTime? fromDate,
        DateTime? toDate,
        int? clientId,
        int? requirementId)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var query = _context.ResumeRequirementLinks
                .Where(l => l.LinkedByUserId == userId);

            // Date filter
            if (fromDate.HasValue)
                query = query.Where(l => l.LinkedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.LinkedAt <= toDate.Value);

            // Requirement filter
            if (requirementId.HasValue)
                query = query.Where(l => l.RequirementId == requirementId.Value);

            // Client filter
            if (clientId.HasValue)
                query = query.Where(l => l.Requirement.ClientId == clientId.Value);

            var funnel = new RecruitmentFunnelVm
            {
                ProfileSubmitted = await query.CountAsync(l =>
                    l.Status == CandidateStatus.New ||
                    l.Status == CandidateStatus.Shortlisted),

                Selected = await query.CountAsync(l =>
                    l.Status == CandidateStatus.Selected),

                InterviewScheduled = await query.CountAsync(l =>
                    l.Status == CandidateStatus.InterviewScheduled),

                OfferReleased = await query.CountAsync(l =>
                    l.Status == CandidateStatus.OfferReleased),

                Joined = await query.CountAsync(l =>
                    l.Status == CandidateStatus.Joined)
            };

            return Json(funnel);
        }

    }
}
