using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;
using ResumeApp.Services;
using ResumeApp.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Controllers
{
    [Authorize(Roles = "Admin,Manager,Team Lead,Panel,Reviewer")]
    public class InterviewsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<Users> _userManager;
        private readonly INotificationService _notifications;

        public InterviewsController(ApplicationDbContext db, UserManager<Users> userManager, INotificationService notifications)
        {
            _db = db;
            _userManager = userManager;
            _notifications = notifications;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? requirementId = null)
        {
            // Build a simple join that stays fully translatable by EF
            var q = from s in _db.InterviewSchedules
                    join r in _db.Resumes on s.ResumeId equals r.Id
                    join req in _db.ClientRequirements on s.RequirementId equals req.Id
                    select new
                    {
                        S = s,
                        R = r,
                        Req = req
                    };

            if (requirementId.HasValue)
                q = q.Where(x => x.S.RequirementId == requirementId.Value);

            // Order by the UTC column in SQL (fully translatable)
            var rows = await q
                .OrderByDescending(x => x.S.ScheduledStartUtc)
                .ToListAsync();

            // Now convert to your VM and do UTC->Local on the app side
            var items = rows.Select(x => new InterviewRowVm
            {
                Id = x.S.Id,
                RequirementId = x.S.RequirementId,
                ResumeId = x.S.ResumeId,
                Round = x.S.Round,

                StartLocal = DateTime.SpecifyKind(x.S.ScheduledStartUtc, DateTimeKind.Utc).ToLocalTime(),
                EndLocal = DateTime.SpecifyKind(x.S.ScheduledEndUtc, DateTimeKind.Utc).ToLocalTime(),

                CandidateName = x.R.Name,
                CandidateEmail = x.R.Email,
                JobTitle = x.Req.JobTitle,
                Mode = x.S.Mode,
                LocationOrLink = x.S.LocationOrLink,
                Status = x.S.Status,
                Outcome = x.S.Outcome

            }).ToList();

            // Provide JD list for the filter dropdown (optional)
            var jdIds = items.Select(i => i.RequirementId).Distinct().ToList();
            var jds = await _db.ClientRequirements
                               .Where(r => jdIds.Contains(r.Id))
                               .OrderBy(r => r.JobTitle)
                               .ToListAsync();
            ViewBag.Requirements = jds;

            return View(items);
        }

        // DETAILS: /Interviews/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var s = await _db.InterviewSchedules.FirstOrDefaultAsync(x => x.Id == id);
            if (s == null) return NotFound();

            // Load resume + JD
            var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == s.ResumeId);
            var req = await _db.ClientRequirements.FirstOrDefaultAsync(r => r.Id == s.RequirementId);

            var vm = new InterviewDetailsVm
            {
                Id = s.Id,
                RequirementId = s.RequirementId,
                JobTitle = req?.JobTitle ?? $"JD #{s.RequirementId}",
                ResumeId = s.ResumeId,
                CandidateName = resume?.Name ?? "(Unknown)",
                CandidateEmail = resume?.Email,
                Round = s.Round,
                StartLocal = DateTime.SpecifyKind(s.ScheduledStartUtc, DateTimeKind.Utc).ToLocalTime(),
                EndLocal = DateTime.SpecifyKind(s.ScheduledEndUtc, DateTimeKind.Utc).ToLocalTime(),
                Mode = s.Mode,
                LocationOrLink = s.LocationOrLink,
                Notes = s.Notes,
                Status = s.Status,
                Outcome = s.Outcome,
                OutcomeNote = s.OutcomeNote,
                ActualStartLocal = s.ActualStartUtc.HasValue
        ? DateTime.SpecifyKind(s.ActualStartUtc.Value, DateTimeKind.Utc).ToLocalTime()
        : null,
                ActualEndLocal = s.ActualEndUtc.HasValue
        ? DateTime.SpecifyKind(s.ActualEndUtc.Value, DateTimeKind.Utc).ToLocalTime()
        : null
            };

            // Resolve panelists from CSV
            if (!string.IsNullOrWhiteSpace(s.PanelUserIdsCsv))
            {
                var ids = s.PanelUserIdsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(x => x.Trim())
                                           .Distinct()
                                           .ToList();
                foreach (var uid in ids)
                {
                    var u = await _userManager.FindByIdAsync(uid);
                    var display = u == null
                        ? uid
                        : string.IsNullOrWhiteSpace(u.FullName) ? u.Email : $"{u.FullName} ({u.Email})";
                    vm.Panelists.Add((uid, display ?? uid));
                }
            }
            // Load feedback for this schedule
            var fbs = await _db.InterviewFeedbacks
                .Where(f => f.InterviewScheduleId == s.Id)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            foreach (var fb in fbs)
            {
                var u = await _userManager.FindByIdAsync(fb.PanelUserId);
                var display = u == null
                    ? fb.PanelUserId
                    : (string.IsNullOrWhiteSpace(u.FullName) ? u.Email : $"{u.FullName} ({u.Email})");
                vm.Feedback.Add((display ?? fb.PanelUserId, fb.Decision, fb.TechScore, fb.CommScore, fb.CultureScore, fb.Comments, fb.CreatedAt));
            }

            return View(vm);
        }
        // GET: /Interviews/Create?requirementId=..&resumeId=..
        [HttpGet]
        public async Task<IActionResult> Create(int requirementId, int resumeId)
        {
            // basic guards
            var linkExists = await _db.ResumeRequirementLinks
                .AnyAsync(l => l.RequirementId == requirementId && l.ResumeId == resumeId);
            if (!linkExists) return NotFound();
            var maxRound = await _db.InterviewSchedules
        .Where(s => s.RequirementId == requirementId && s.ResumeId == resumeId)
        .Select(s => (int?)s.Round)
        .MaxAsync() ?? 0;

            var nextRound = maxRound + 1;
            // preload panel/admin/manager users for multi-select
            var panels = await _userManager.GetUsersInRoleAsync("Panel");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var mgrs = await _userManager.GetUsersInRoleAsync("Manager");
            ViewBag.PossiblePanelists = panels.Concat(admins).Concat(mgrs).DistinctBy(u => u.Id).ToList();

            // default form values
            var vm = new InterviewCreateVm
            {
                RequirementId = requirementId,
                ResumeId = resumeId,
                Round = nextRound,
                Mode = "Video",
                StartLocal = DateTime.Now.AddDays(1).Date.AddHours(11), // tomorrow 11:00 by default
                DurationMinutes = 60
            };
            return View(vm);
        }

        // POST: /Interviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InterviewCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                var panels = await _userManager.GetUsersInRoleAsync("Panel");
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                var mgrs = await _userManager.GetUsersInRoleAsync("Manager");
                ViewBag.PossiblePanelists = panels.Concat(admins).Concat(mgrs).DistinctBy(u => u.Id).ToList();
                return View(vm);
            }

            // validate candidate link exists
            var link = await _db.ResumeRequirementLinks
                .FirstOrDefaultAsync(l => l.RequirementId == vm.RequirementId && l.ResumeId == vm.ResumeId);
            if (link == null) return NotFound();

            // ensure round is at least next available
            var maxRound = await _db.InterviewSchedules
                .Where(s => s.RequirementId == vm.RequirementId && s.ResumeId == vm.ResumeId)
                .Select(s => (int?)s.Round)
                .MaxAsync() ?? 0;
            if (vm.Round <= maxRound) vm.Round = maxRound + 1;
            // convert local to UTC (assume app server time == local for MVP)
            var startUtc = DateTime.SpecifyKind(vm.StartLocal, DateTimeKind.Local).ToUniversalTime();
            var endUtc = startUtc.AddMinutes(vm.DurationMinutes);

            // normalize panel list and make CSV
            var cleanPanelIds = (vm.PanelUserIds ?? new List<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();
            var csv = cleanPanelIds.Count > 0 ? string.Join(",", cleanPanelIds) : null;

            // create schedule
            var me = _userManager.GetUserId(User)!;
            var schedule = new InterviewSchedule
            {
                RequirementId = vm.RequirementId,
                ResumeId = vm.ResumeId,
                Round = vm.Round,
                ScheduledStartUtc = startUtc,
                ScheduledEndUtc = endUtc,
                Mode = vm.Mode ?? "Video",
                LocationOrLink = vm.LocationOrLink,
                PanelUserIdsCsv = cleanPanelIds.Any() ? string.Join(",", cleanPanelIds) : null,
                Notes = vm.Notes,
                Status = InterviewStatus.Planned,
                CreatedByUserId = me
            };

            _db.InterviewSchedules.Add(schedule);

            // update candidate link → InterviewScheduled
            link.Status = CandidateStatus.InterviewScheduled;
            link.LastComment = $"Interview scheduled (Round {vm.Round})";
            link.UpdatedAt = DateTime.UtcNow;

            _db.CandidateStatusHistories.Add(new CandidateStatusHistory
            {
                RequirementId = vm.RequirementId,
                ResumeId = vm.ResumeId,
                Status = CandidateStatus.InterviewScheduled,
                Comment = $"Round {vm.Round} — {vm.Mode} — {vm.LocationOrLink}",
                ChangedByUserId = me,
                ChangedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            // notify: panelists + TL + Manager + Admin + uploader
            var tlIds = (await _userManager.GetUsersInRoleAsync("Team Lead")).Select(u => u.Id);
            var mgrIds = (await _userManager.GetUsersInRoleAsync("Manager")).Select(u => u.Id);
            var admIds = (await _userManager.GetUsersInRoleAsync("Admin")).Select(u => u.Id);
            var uploaderId = await _db.Resumes
                .Where(r => r.Id == vm.ResumeId && r.UserId != null)
                .Select(r => r.UserId!)
                .FirstOrDefaultAsync();

            var notifyIds = tlIds.Concat(mgrIds).Concat(admIds)
                .Concat(cleanPanelIds)
                .Concat(string.IsNullOrWhiteSpace(uploaderId) ? Array.Empty<string>() : new[] { uploaderId })
                .Where(id => id != me)
                .Distinct()
                .ToList();

            await _notifications.NotifyAsync(
                notifyIds,
                type: "Interview",
                title: "Interview Scheduled",
                body: $"Round {vm.Round} — starts {startUtc:u}",
                url: Url.Action("Details", "ClientRequirements",
                        new { id = vm.RequirementId }, Request.Scheme)
            );

            TempData["Success"] = "Interview scheduled.";
            return RedirectToAction("SharedProfiles", "ClientRequirements", new { id = vm.RequirementId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetOutcome(
        int id,
        InterviewOutcome outcome,
        string? note,
        DateTime? actualStartLocal,
        DateTime? actualEndLocal)
        {
            var s = await _db.InterviewSchedules.FirstOrDefaultAsync(x => x.Id == id);
            if (s == null) return NotFound();

            // Save outcome + actuals
            s.Outcome = outcome;
            s.OutcomeNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            s.ActualStartUtc = actualStartLocal.HasValue
                ? DateTime.SpecifyKind(actualStartLocal.Value, DateTimeKind.Local).ToUniversalTime()
                : null;
            s.ActualEndUtc = actualEndLocal.HasValue
                ? DateTime.SpecifyKind(actualEndLocal.Value, DateTimeKind.Local).ToUniversalTime()
                : null;

            // Optional: if any final attendance state, mark schedule completed
            if (outcome != InterviewOutcome.None)
                s.Status = InterviewStatus.Completed;

            await _db.SaveChangesAsync();

            // (Optional) notify stakeholders
            // await _notifications.NotifyAsync(...)

            TempData["Success"] = "Interview outcome updated.";
            return RedirectToAction(nameof(Details), new { id = s.Id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, InterviewStatus status)
        {
            var s = await _db.InterviewSchedules.FirstOrDefaultAsync(x => x.Id == id);
            if (s == null) return NotFound();

            s.Status = status;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Interview marked as {status}.";
            return RedirectToAction(nameof(Details), new { id });
        }
       
        [HttpGet]
        [Authorize(Roles = "Panel,Admin,Manager")]
        public async Task<IActionResult> Feedback(int id) // id = InterviewSchedule.Id
        {
            var me = _userManager.GetUserId(User)!;
            var s = await _db.InterviewSchedules.FirstOrDefaultAsync(x => x.Id == id);
            if (s == null) return NotFound();

            // Must be assigned as panelist OR privileged
            bool isPrivileged = User.IsInRole("Admin") || User.IsInRole("Manager");
            var assigned = (s.PanelUserIdsCsv ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Contains(me);

            if (!(assigned || isPrivileged))
                return Forbid();

            var existing = await _db.InterviewFeedbacks
                .FirstOrDefaultAsync(f => f.InterviewScheduleId == id && f.PanelUserId == me);

            var vm = new
            {
                InterviewScheduleId = s.Id,
                s.RequirementId,
                s.ResumeId,
                s.Round,
                Decision = existing?.Decision,
                Comments = existing?.Comments,
                TechScore = existing?.TechScore,
                CommScore = existing?.CommScore,
                CultureScore = existing?.CultureScore
            };

            return View(vm); // Views/Interviews/Feedback.cshtml
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Panel,Admin,Manager")]
        public async Task<IActionResult> FeedbackSave(
         int interviewScheduleId,
         InterviewFeedbackDecision decision,
         string? comments,
         int? techScore,
         int? commScore,
         int? cultureScore)
        {
            var me = _userManager.GetUserId(User)!;
            var s = await _db.InterviewSchedules.FirstOrDefaultAsync(x => x.Id == interviewScheduleId);
            if (s == null) return NotFound();

            // Guard: assigned or privileged
            bool isPrivileged = User.IsInRole("Admin") || User.IsInRole("Manager");
            var assigned = (s.PanelUserIdsCsv ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Contains(me);

            if (!(assigned || isPrivileged))
                return Forbid();

            // Only allow FinalSelect on last round (unless privileged)
            // Compute max round fully in SQL, then default in memory
            int? latestRound = await _db.InterviewSchedules
                .Where(x => x.RequirementId == s.RequirementId && x.ResumeId == s.ResumeId)
                .Select(x => (int?)x.Round)   // nullable so MaxAsync returns int?
                .MaxAsync();

            int maxRound = latestRound ?? s.Round;   // if none found, fall back to this interview’s round

            if (decision == InterviewFeedbackDecision.FinalSelect && s.Round < maxRound && !isPrivileged)
            {
                TempData["Error"] = "Final Select is allowed only in the last round.";
                return RedirectToAction(nameof(Details), new { id = interviewScheduleId });
            }


            var existing = await _db.InterviewFeedbacks
                .FirstOrDefaultAsync(f => f.InterviewScheduleId == interviewScheduleId && f.PanelUserId == me);

            if (existing == null)
            {
                existing = new InterviewFeedback
                {
                    InterviewScheduleId = s.Id,
                    RequirementId = s.RequirementId,
                    ResumeId = s.ResumeId,
                    Round = s.Round,
                    PanelUserId = me,
                    Decision = decision,
                    Comments = comments,
                    TechScore = techScore,
                    CommScore = commScore,
                    CultureScore = cultureScore
                };
                _db.InterviewFeedbacks.Add(existing);
            }
            else
            {
                existing.Decision = decision;
                existing.Comments = comments;
                existing.TechScore = techScore;
                existing.CommScore = commScore;
                existing.CultureScore = cultureScore;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            // OPTIONAL: auto-update candidate state on FinalSelect
            if (decision == InterviewFeedbackDecision.FinalSelect)
            {
                var link = await _db.ResumeRequirementLinks
                    .FirstOrDefaultAsync(l => l.RequirementId == s.RequirementId && l.ResumeId == s.ResumeId);
                if (link != null)
                {
                    link.Status = CandidateStatus.Selected; // or require TL to publish
                    link.LastComment = "Final Select by panel";
                    link.UpdatedAt = DateTime.UtcNow;

                    _db.CandidateStatusHistories.Add(new CandidateStatusHistory
                    {
                        RequirementId = s.RequirementId,
                        ResumeId = s.ResumeId,
                        Status = CandidateStatus.Selected,
                        Comment = "Final Select by panel",
                        ChangedByUserId = me,
                        ChangedAt = DateTime.UtcNow
                    });

                    await _db.SaveChangesAsync();
                }
            }

            TempData["Success"] = "Feedback saved.";
            return RedirectToAction(nameof(Details), new { id = interviewScheduleId });
        }

    }
}

