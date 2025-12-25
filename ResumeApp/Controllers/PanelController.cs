using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;
using ResumeApp.Services;

namespace ResumeApp.Controllers
{
    [Authorize(Roles = "Panel,Admin,Manager")] 

    public class PanelController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Users> _userManager;
        private readonly INotificationService _notificationService;

        public PanelController(ApplicationDbContext db, UserManager<Users> userManager, INotificationService notificationService)
        {
            _db = db;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // List of profiles assigned to me (optional: filter param)
        public async Task<IActionResult> Index(int? requirementId = null)
        {
            var me = _userManager.GetUserId(User)!;

            var q = from a in _db.PanelAssignments
                    where a.PanelUserId == me
                    join l in _db.ResumeRequirementLinks on new { a.RequirementId, a.ResumeId } equals new { l.RequirementId, l.ResumeId }
                    select new { a, l };

            if (requirementId.HasValue)
                q = q.Where(x => x.a.RequirementId == requirementId.Value);

            var items = await q
                .OrderByDescending(x => x.l.UpdatedAt)
                .Select(x => x.l)
                .Include(l => l.Resume)
                .Include(l => l.LinkedByUser)
                .ToListAsync();

            // If you want the JD info at the top:
            ViewBag.Requirements = await _db.ClientRequirements
                .Where(r => items.Select(i => i.RequirementId).Contains(r.Id))
                .ToListAsync();

            return View(items); // make a simple list with a “Review”/“Decide” button
        }

        // Simple detail page if you want to show all info
        public async Task<IActionResult> Review(int requirementId, int resumeId)
        {
            var me = _userManager.GetUserId(User)!;
            var allowed = await _db.PanelAssignments.AnyAsync(a =>
                a.PanelUserId == me && a.RequirementId == requirementId && a.ResumeId == resumeId);
            if (!allowed) return Forbid();

            var link = await _db.ResumeRequirementLinks
                .Include(l => l.Resume).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(l => l.RequirementId == requirementId && l.ResumeId == resumeId);
            if (link == null) return NotFound();

            return View(link);
        }

        // The decision endpoint (Select / Reject / Hold)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decide(int requirementId, int resumeId, string decision, string? remark)
        {
            var me = _userManager.GetUserId(User)!;

            var allowed = await _db.PanelAssignments.AnyAsync(a =>
                a.PanelUserId == me && a.RequirementId == requirementId && a.ResumeId == resumeId);
            if (!allowed) return Forbid();

            var link = await _db.ResumeRequirementLinks
                .FirstOrDefaultAsync(l => l.RequirementId == requirementId && l.ResumeId == resumeId);
            if (link == null) return NotFound();

            CandidateStatus target = decision.ToLower() switch
            {
                "select" => CandidateStatus.PanelScreenSelected,
                "reject" => CandidateStatus.PanelScreenRejected,
                "hold" => CandidateStatus.PanelScreenHold,
                _ => CandidateStatus.PanelScreenHold
            };

            // Write feedback row
            _db.PanelFeedbacks.Add(new PanelFeedback
            {
                RequirementId = requirementId,
                ResumeId = resumeId,
                PanelUserId = me,
                Decision = target,
                Remark = remark
            });

            // Update link status + history
            link.Status = target;
            link.LastComment = remark;
            link.UpdatedAt = DateTime.UtcNow;

            _db.CandidateStatusHistories.Add(new CandidateStatusHistory
            {
                RequirementId = requirementId,
                ResumeId = resumeId,
                Status = target,
                Comment = remark,
                ChangedByUserId = me,
                ChangedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            // Notify TL/Manager/Admin/uploader
            // (you already have a helper in ClientRequirementsController; replicate here minimally)
            var tlIds = (await _userManager.GetUsersInRoleAsync("Team Lead")).Select(u => u.Id);
            var mgrIds = (await _userManager.GetUsersInRoleAsync("Manager")).Select(u => u.Id);
            var adminIds = (await _userManager.GetUsersInRoleAsync("Admin")).Select(u => u.Id);

            var uploaderId = await _db.Resumes
                .Where(r => r.Id == resumeId && r.UserId != null)
                .Select(r => r.UserId!)
                .FirstOrDefaultAsync();

            var recipients = tlIds.Concat(mgrIds).Concat(adminIds)
                                  .Concat(string.IsNullOrWhiteSpace(uploaderId) ? Array.Empty<string>() : new[] { uploaderId })
                                  .Where(id => id != me)
                                  .Distinct()
                                  .ToList();

            await _notificationService.NotifyAsync(
                recipients,
                type: "PanelDecision",
                title: $"Panel: {decision.ToUpperInvariant()} for candidate",
                body: remark,
                url: Url.Action("SharedProfiles", "ClientRequirements", new { id = requirementId }, Request.Scheme)
            );

            TempData["Success"] = "Decision recorded.";
            return RedirectToAction(nameof(Index));
        }
    }
}
