using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Helpers;
using ResumeApp.Models;
using ResumeApp.ViewModels;

namespace ResumeApp.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RecruiterDashboardViewModel> GetRecruiterDashboardAsync(string userId)
        {
            var vm = new RecruiterDashboardViewModel();

            // ================= HEADER =================
            vm.RecruiterName = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync();

            // ================= BASE QUERY =================
            var links = _context.ResumeRequirementLinks
                .Where(l => l.LinkedByUserId == userId);

            // ================= KPI COUNTS =================

            vm.ProfileSubmittedCount =
                await links.CountAsync(l => l.Status == CandidateStatus.New);

            vm.PipelineCount =
                await links.CountAsync(l => l.Status == CandidateStatus.Shortlisted);

            vm.ScreeningPendingCount =
                await links.CountAsync(l => l.Status == CandidateStatus.PanelShortlisted);

            vm.InterviewScheduledCount =
                await links.CountAsync(l => l.Status == CandidateStatus.InterviewScheduled);

            vm.FinalSelectCount =
                await links.CountAsync(l => l.Status == CandidateStatus.Selected);

            vm.OfferReleasedCount =
                await links.CountAsync(l => l.Status == CandidateStatus.OfferReleased);

            vm.JoinedCount =
                await links.CountAsync(l => l.Status == CandidateStatus.Joined);

            vm.FollowUpCount =
                await links.CountAsync(l => l.Status == CandidateStatus.Hold);

            // ================= STATUS OVERVIEW =================
            vm.StatusCounts = await links
                .GroupBy(l => l.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            // ================= RECENT UPLOADS =================
            vm.RecentUploads = await links
                .Include(l => l.Resume)
                .Include(l => l.Requirement)
                .OrderByDescending(l => l.LinkedAt)
                .Take(5)
                .Select(l => new RecentUploadVm
                {
                    RequirementId = l.RequirementId,
                    ResumeId = l.ResumeId,
                    CandidateName = l.Resume.Name,
                    JobTitle = l.Requirement.JobTitle,
                    UploadedAt = l.LinkedAt,
                    Status = l.Status,
                    MatchScore = l.MatchScore
                })
                .ToListAsync();

            // ================= ACTIONS NEEDED =================
            var now = DateTime.UtcNow;

            var assignedReqIds = await _context.RequirementAssignments
                .Where(a => a.UserId == userId)
                .Select(a => a.RequirementId)
                .ToListAsync();

            var uploads = await links.ToListAsync();

            // JD with ZERO uploads
            foreach (var reqId in assignedReqIds)
            {
                if (!uploads.Any(u => u.RequirementId == reqId))
                {
                    vm.ActionItems.Add(new ActionItemVm
                    {
                        Type = "warning",
                        Message = $"JD #{reqId} has no uploads yet"
                    });
                }
            }

            //  JD idle > 3 days
            foreach (var g in uploads.GroupBy(u => u.RequirementId))
            {
                var lastUpload = g.Max(x => x.LinkedAt);
                if ((now - lastUpload).TotalDays >= 3)
                {
                    vm.ActionItems.Add(new ActionItemVm
                    {
                        Type = "info",
                        Message = $"JD #{g.Key} has no uploads in last 3 days"
                    });
                }
            }

            // 🔵 Pending TL review
            if (uploads.Any(u => u.Status == CandidateStatus.Shortlisted))
            {
                vm.ActionItems.Add(new ActionItemVm
                {
                    Type = "secondary",
                    Message = "Profiles pending TL review"
                });
            }

            // 🟣 Pending Panel feedback > 2 days
            if (uploads.Any(u =>
                u.Status == CandidateStatus.PanelShortlisted &&
                (now - u.UpdatedAt).TotalDays >= 2))
            {
                vm.ActionItems.Add(new ActionItemVm
                {
                    Type = "secondary",
                    Message = "Panel feedback pending for submitted profiles"
                });
            }

            vm.ActionItems = vm.ActionItems.Take(5).ToList();

            // ================= MY RECRUITMENTS =================
            vm.MyRecruitments = await _context.RequirementAssignments
                .Where(a => a.UserId == userId)
                .Select(a => new MyRecruitmentRowVm
                {
                    RequirementId = a.RequirementId,
                    ClientName = a.Requirement.Client.CompanyName,
                    JobTitle = a.Requirement.JobTitle,
                    Positions = a.Requirement.Positions,
                    UploadedCount = _context.ResumeRequirementLinks.Count(l =>
                        l.RequirementId == a.RequirementId &&
                        l.LinkedByUserId == userId),
                    Status = a.Requirement.Status.ToString()
                })
                .OrderByDescending(x => x.UploadedCount)
                .ToListAsync();
            vm.ClientFilters = await _context.RequirementAssignments
                .Where(a => a.UserId == userId)
                .Select(a => a.Requirement.Client)
                .Distinct()
                .Select(c => new DropdownVm
                {
                    Id = c.Id,
                    Name = c.CompanyName
                })
                .ToListAsync();

            vm.RequirementFilters = await _context.RequirementAssignments
                .Where(a => a.UserId == userId)
                .Select(a => a.Requirement)
                .Distinct()
                .Select(r => new DropdownVm
                {
                    Id = r.Id,
                    Name = r.JobTitle
                })
                .ToListAsync();

            // ================= QUICK ACTIONS =================
            var today = DateTime.UtcNow.Date;

            var uploadsToday = await _context.Resumes
                .CountAsync(r => r.UserId == userId && r.UploadedAt >= today);

            var reqsWithUploads = await links
                .Select(l => l.RequirementId)
                .Distinct()
                .ToListAsync();

            var urgentRequirements = assignedReqIds
                .Count(id => !reqsWithUploads.Contains(id));

            var activeClients = await _context.RequirementAssignments
                .Where(a => a.UserId == userId)
                .Select(a => a.Requirement.ClientId)
                .Distinct()
                .CountAsync();

            vm.QuickActions = new List<QuickActionVm>
    {
        new()
        {
            Key = "uploads",
            Title = "My Uploads (Today)",
            Count = uploadsToday,
            BadgeType = uploadsToday > 0 ? "success" : "secondary"
        },
        new()
        {
            Key = "requirements",
            Title = "Urgent Requirements",
            Count = urgentRequirements,
            BadgeType = urgentRequirements > 0 ? "danger" : "secondary"
        },
        new()
        {
            Key = "clients",
            Title = "Active Clients",
            Count = activeClients,
            BadgeType = "primary"
        },
        new()
        {
            Key = "assignments",
            Title = "My Assigned JDs",
            Count = assignedReqIds.Count,
            BadgeType = "info"
        }
    };

            return vm;
        }
        private static CandidateStatus MapKpiToStatus(string kpi)
        {
            return kpi switch
            {
                "ProfileSubmitted" => CandidateStatus.New,
                "Pipeline" => CandidateStatus.Shortlisted,
                "ScreeningPending" => CandidateStatus.PanelShortlisted,
                "InterviewScheduled" => CandidateStatus.InterviewScheduled,
                "FinalSelect" => CandidateStatus.Selected,
                "OfferReleased" => CandidateStatus.OfferReleased,
                "Joined" => CandidateStatus.Joined,
                "FollowUp" => CandidateStatus.Hold,
                _ => CandidateStatus.New
            };
        }

        public async Task<AdminDashboardViewModel> GetAdminDashboardAsync()
        {
            var vm = new AdminDashboardViewModel();

            vm.Kpis.TotalClients = await _context.Clients
                .AsNoTracking()
                .CountAsync(c => c.IsActive && !c.IsDeleted);

            vm.Kpis.TotalRequirements = await _context.ClientRequirements
                .AsNoTracking()
                .CountAsync();

            vm.Kpis.ActiveRequirements = await _context.ClientRequirements
                .AsNoTracking().CountAsync(r => r.Status == RequirementStatus.Active &&
                !r.IsDeleted);

            vm.Kpis.TotalResumes = await _context.Resumes
                .AsNoTracking()
                .CountAsync();

            vm.Kpis.ProfilesInProgress = await _context.ResumeRequirementLinks
                 .AsNoTracking()
                 .Where(l => CandidateStatusGroups.InProgress.Contains(l.Status))
                 .Select(l => l.ResumeId)
                 .Distinct()
                 .CountAsync();


            vm.Kpis.InterviewsScheduled = await _context.InterviewSchedules
                .AsNoTracking()
                .CountAsync(i =>
                    i.Status == InterviewStatus.Planned ||
                    i.Status == InterviewStatus.Confirmed);

            // ============================
            // PIPELINE SECTION
            // ============================

            var currentStatusCounts = await _context.ResumeRequirementLinks
                 .AsNoTracking()
                 .GroupBy(l => l.Status)
                 .Select(g => new
                 {
                     Status = g.Key,
                     Count = g.Count()
                 })
                 .ToListAsync();


            vm.Pipeline.New =
                currentStatusCounts.FirstOrDefault(x => x.Status == CandidateStatus.New)?.Count ?? 0;

            vm.Pipeline.Shortlisted =
                currentStatusCounts.FirstOrDefault(x => x.Status == CandidateStatus.Shortlisted)?.Count ?? 0;

            vm.Pipeline.PanelShortlisted =
                currentStatusCounts.FirstOrDefault(x => x.Status == CandidateStatus.PanelShortlisted)?.Count ?? 0;

            vm.Pipeline.InterviewScheduled =
                currentStatusCounts.FirstOrDefault(x => x.Status == CandidateStatus.InterviewScheduled)?.Count ?? 0;

            vm.Pipeline.Selected =
                currentStatusCounts.FirstOrDefault(x => x.Status == CandidateStatus.Selected)?.Count ?? 0;

            vm.Pipeline.OfferReleased =
                currentStatusCounts.FirstOrDefault(x => x.Status == CandidateStatus.OfferReleased)?.Count ?? 0;

            vm.Pipeline.Joined =
                currentStatusCounts.FirstOrDefault(x => x.Status == CandidateStatus.Joined)?.Count ?? 0;

            vm.Pipeline.Hold =
                currentStatusCounts.FirstOrDefault(x => x.Status == CandidateStatus.Hold)?.Count ?? 0;


            vm.PipelineChart = new List<BarChartItem>
            {
                new() { Label = "New", Count = vm.Pipeline.New },
                new() { Label = "Shortlisted", Count = vm.Pipeline.Shortlisted },
                new() { Label = "Panel Shortlisted", Count = vm.Pipeline.PanelShortlisted },
                new() { Label = "Interview", Count = vm.Pipeline.InterviewScheduled },
                new() { Label = "Selected", Count = vm.Pipeline.Selected },
                new() { Label = "Joined", Count = vm.Pipeline.Joined }
            };


            var clientPerformanceData = await (
                from req in _context.ClientRequirements.AsNoTracking()
                where !req.IsDeleted
                group req by new
                {
                    req.ClientId,
                    req.Client.CompanyName
                }
                into g
                select new ClientPerformanceRow
                {
                    ClientName = g.Key.CompanyName,

                    // Total openings
                    Openings = g.Sum(x => x.Positions),

                    // Total submissions
                    Submissions = _context.ResumeRequirementLinks
                        .Count(l => g.Select(r => r.Id).Contains(l.RequirementId)),

                    // Pipeline
                    Pipeline = _context.ResumeRequirementLinks
                        .Count(l =>
                            g.Select(r => r.Id).Contains(l.RequirementId) &&
                            (l.Status == CandidateStatus.New ||
                             l.Status == CandidateStatus.Shortlisted ||
                             l.Status == CandidateStatus.PanelShortlisted ||
                             l.Status == CandidateStatus.Hold)
                        ),

                    // Interview Scheduled
                    InterviewScheduled = _context.ResumeRequirementLinks
                        .Count(l =>
                            g.Select(r => r.Id).Contains(l.RequirementId) &&
                            l.Status == CandidateStatus.InterviewScheduled
                        ),

                    // Offer Released
                    OfferReleased = _context.ResumeRequirementLinks
                        .Count(l =>
                            g.Select(r => r.Id).Contains(l.RequirementId) &&
                            l.Status == CandidateStatus.OfferReleased
                        ),

                    // Joined
                    Joined = _context.ResumeRequirementLinks
                        .Count(l =>
                            g.Select(r => r.Id).Contains(l.RequirementId) &&
                            l.Status == CandidateStatus.Joined
                        ),

                    // Declined
                    Declined = _context.ResumeRequirementLinks
                        .Count(l =>
                            g.Select(r => r.Id).Contains(l.RequirementId) &&
                            l.Status == CandidateStatus.Rejected
                        )
                })
                .OrderByDescending(x => x.Submissions)
                .ToListAsync();

            vm.ClientPerformance = clientPerformanceData;


            vm.RecruiterPerformance = await (
            from r in _context.Resumes.AsNoTracking()
            join u in _context.Users.AsNoTracking()
                on r.UserId equals u.Id into userGroup
            from u in userGroup.DefaultIfEmpty()
            group new { r, u } by new
            {
                r.UserId,
                RecruiterName = u.FullName ?? u.Email ?? "Unknown"
            }
            into g
            select new RecruiterPerformanceRow
            {
                RecruiterName = g.Key.RecruiterName,
                Uploads = g.Count(),
                Shortlisted = _context.CandidateStatusHistories
                    .Where(s =>
                        s.Status == CandidateStatus.Shortlisted &&
                        g.Select(x => x.r.Id).Contains(s.ResumeId))
                    .Select(s => s.ResumeId)
                    .Distinct()
                    .Count(),
                Interviews = _context.InterviewSchedules
                    .Where(i =>
                        (i.Status == InterviewStatus.Planned ||
                         i.Status == InterviewStatus.Confirmed) &&
                        g.Select(x => x.r.Id).Contains(i.ResumeId))
                    .Select(i => i.ResumeId)
                    .Distinct()
                    .Count()
            })
            .OrderByDescending(x => x.Uploads)
            .Take(5)
            .ToListAsync();

            // RECENT ACTIVITY 

            vm.RecentActivities = await _context.CandidateStatusHistories
                .AsNoTracking()
                .OrderByDescending(a => a.ChangedAt)
                .Take(5)
                .Select(a => new ActivityItem
                {
                    Message = $"Resume #{a.ResumeId} moved to {(CandidateStatus)a.Status}",
                    Timestamp = a.ChangedAt
                })
                .ToListAsync();
            var today = DateTime.UtcNow.Date;

            vm.QuickCounts.TotalUsers = await _context.Users
                .AsNoTracking()
                .CountAsync();

            vm.QuickCounts.TotalResumes = vm.Kpis.TotalResumes;

            vm.QuickCounts.UploadsToday = await _context.Resumes
                .AsNoTracking()
                .CountAsync(r => r.UploadedAt >= today);

            vm.QuickCounts.TotalClients = await _context.Clients
                .AsNoTracking()
                .CountAsync(c => !c.IsDeleted);

            vm.QuickCounts.ActiveClients = await _context.Clients
                .AsNoTracking()
                .CountAsync(c => c.IsActive && !c.IsDeleted);

            vm.QuickCounts.ActiveRequirements = vm.Kpis.ActiveRequirements;
            var jdWithZeroUploads = await _context.ClientRequirements
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.Status == RequirementStatus.Active)
            .Select(r => new
            {
                r.Id,
                r.JobTitle,
                UploadCount = _context.ResumeRequirementLinks
                    .Count(l => l.RequirementId == r.Id)
            })
            .Where(x => x.UploadCount == 0)
            .Take(3)
            .ToListAsync();
                    foreach (var jd in jdWithZeroUploads)
                    {
                        vm.Alerts.Add(new AdminAlertVm
                        {
                            Type = "danger",
                            Message = $"JD '{jd.JobTitle}' has no uploads yet",
                            ActionUrl = $"/ClientRequirements/FullDetails/{jd.Id}"
                        });
                    }
                    var idleJds = await _context.ResumeRequirementLinks
            .GroupBy(l => l.RequirementId)
            .Select(g => new
            {
                RequirementId = g.Key,
                LastUpload = g.Max(x => x.LinkedAt)
            })
            .Where(x => EF.Functions.DateDiffDay(x.LastUpload, DateTime.UtcNow) >= 3)
            .Take(3)
            .ToListAsync();
                    foreach (var jd in idleJds)
                    {
                        vm.Alerts.Add(new AdminAlertVm
                        {
                            Type = "warning",
                            Message = $"JD #{jd.RequirementId} has no uploads in last 3 days",
                            ActionUrl = $"/ClientRequirements/FullDetails/{jd.RequirementId}"
                        });
                    }
                    var panelPendingCount = await _context.CandidateStatusHistories
            .AsNoTracking()
            .Where(s =>
                s.Status == CandidateStatus.PanelShortlisted &&
                EF.Functions.DateDiffDay(s.ChangedAt, DateTime.UtcNow) >= 2)
            .Select(s => s.ResumeId)
            .Distinct()
            .CountAsync();
                    if (panelPendingCount > 0)
                    {
                        vm.Alerts.Add(new AdminAlertVm
                        {
                            Type = "info",
                            Message = $"{panelPendingCount} profiles pending panel feedback (> 2 days)",
                            ActionUrl = "/Dashboard/Admin"
                        });
                    }
                    var overdueInterviews = await _context.InterviewSchedules
            .AsNoTracking()
            .CountAsync(i =>
                i.Status == InterviewStatus.Confirmed &&
                i.ScheduledEndUtc < DateTime.UtcNow &&
                i.ActualEndUtc == null);
                    if (overdueInterviews > 0)
                    {
                        vm.Alerts.Add(new AdminAlertVm
                        {
                            Type = "warning",
                            Message = $"{overdueInterviews} interviews not completed",
                            ActionUrl = "/Interview"
                        });
                    }
                    vm.Alerts = vm.Alerts
            .OrderBy(a => a.Type == "danger" ? 0 :
                          a.Type == "warning" ? 1 : 2)
            .Take(6)
            .ToList();

            return vm;
        }
        public async Task<ManagerDashboardViewModel> GetManagerDashboardAsync(string? filter = null)
        {
            var kpis = new KpiSection();

            kpis.TotalClients = await _context.Clients
                .AsNoTracking()
                .CountAsync(c => c.IsActive && !c.IsDeleted);

            kpis.TotalRequirements = await _context.ClientRequirements
                .AsNoTracking()
                .CountAsync(r => !r.IsDeleted);

            kpis.ActiveRequirements = await _context.ClientRequirements
                .AsNoTracking()
                .CountAsync(r => r.Status == RequirementStatus.Active && !r.IsDeleted);

            kpis.TotalResumes = await _context.Resumes
                .AsNoTracking()
                .CountAsync();

            kpis.ProfilesInProgress = await _context.ResumeRequirementLinks
                .AsNoTracking()
                .Where(l => CandidateStatusGroups.InProgress.Contains(l.Status))
                .CountAsync();

            kpis.InterviewsScheduled = await _context.ResumeRequirementLinks
                .AsNoTracking()
                .CountAsync(l => l.Status == CandidateStatus.InterviewScheduled);


            // Base Requirements 
            var requirements = _context.ClientRequirements
                .Where(r => !r.IsDeleted);

            // Group Requirements by Client
            var requirementStats = await requirements
                .GroupBy(r => new { r.ClientId, r.Client.CompanyName })
                .Select(g => new
                {
                    g.Key.ClientId,
                    g.Key.CompanyName,
                    TotalOpenings = g.Sum(x => x.Positions),
                    ActiveRequirements = g.Count(x => x.Status == RequirementStatus.Active)
                })
                .ToListAsync();

            // Resume stats grouped by client
            var resumeStats = await _context.ResumeRequirementLinks
                .AsNoTracking()
                .Where(l => !l.Requirement.IsDeleted)
                .GroupBy(l => l.Requirement.ClientId)
                .Select(g => new
                {
                    ClientId = g.Key,

                    // ALL submissions
                    Submissions = g.Count(),

                    // In Progress 
                    InterviewScheduled = g.Count(x => x.Status == CandidateStatus.InterviewScheduled),

                    Rejected = g.Count(x =>
                        x.Status == CandidateStatus.Rejected ||
                        x.Status == CandidateStatus.PanelScreenRejected),

                    Selected = g.Count(x =>
                        x.Status == CandidateStatus.Selected ||
                        x.Status == CandidateStatus.PanelScreenSelected),

                    OfferReleased = g.Count(x => x.Status == CandidateStatus.OfferReleased),

                    Joined = g.Count(x => x.Status == CandidateStatus.Joined)
                })
                .ToListAsync();

            // 4️⃣ Merge both
            var clients = requirementStats.Select(r =>
            {
                var stat = resumeStats.FirstOrDefault(x => x.ClientId == r.ClientId);

                return new ClientOverviewRowDto
                {
                    ClientId = r.ClientId,
                    ClientName = r.CompanyName,
                    Openings = r.TotalOpenings,
                    Submissions = stat?.Submissions ?? 0,
                    InterviewScheduled = stat?.InterviewScheduled ?? 0,
                    Rejected = stat?.Rejected ?? 0,
                    Selected = stat?.Selected ?? 0,
                    OfferReleased = stat?.OfferReleased ?? 0,
                    Joined = stat?.Joined ?? 0,
                    Declined = 0 // not implemented yet
                };
            }).ToList();

            //  Apply Active/Inactive filter
            if (!string.IsNullOrEmpty(filter))
            {
                if (filter == "active")
                    clients = clients.Where(c =>
                        requirementStats.Any(r => r.ClientId == c.ClientId && r.ActiveRequirements > 0))
                        .ToList();

                if (filter == "inactive")
                    clients = clients.Where(c =>
                        requirementStats.Any(r => r.ClientId == c.ClientId && r.ActiveRequirements == 0))
                        .ToList();
            }

            // 6️⃣ Build summary
            var summary = new ClientOverviewSummaryDto
            {
                TotalClients = requirementStats.Count,
                ActiveClients = requirementStats.Count(r => r.ActiveRequirements > 0),
                InactiveClients = requirementStats.Count(r => r.ActiveRequirements == 0),

                TotalOpenings = requirementStats.Sum(r => r.TotalOpenings),
                ActiveRequirements = requirementStats.Sum(r => r.ActiveRequirements),

                TotalSubmissions = clients.Sum(c => c.Submissions),
                TotalInterviewScheduled = clients.Sum(c => c.InterviewScheduled),
                TotalRejected = clients.Sum(c => c.Rejected),
                TotalSelected = clients.Sum(c => c.Selected),
                TotalOfferReleased = clients.Sum(c => c.OfferReleased),
                TotalJoined = clients.Sum(c => c.Joined),
                TotalDeclined = 0
            };

            return new ManagerDashboardViewModel
            {
                Kpis = kpis,
                Summary = summary,
                Clients = clients
            };
        }
    }
}
