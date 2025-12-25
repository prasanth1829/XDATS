using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;
using ResumeApp.ViewModels;

namespace ResumeApp.Services
{
    public class DashboardService: IDashboardService
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


    }
}
