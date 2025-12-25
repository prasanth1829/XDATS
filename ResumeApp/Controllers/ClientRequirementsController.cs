using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;
using ResumeApp.Models.Master;
using ResumeApp.Services;
using ResumeApp.ViewModels;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using AppUser = ResumeApp.Models.Users;


namespace ResumeApp.Controllers
{
    [Authorize(Roles = "Reviewer,Admin,Team Lead,Manager,Panel")]
    public class ClientRequirementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMatchScoringService _matchScore;

        private bool IsPrivilegedRole() =>
            User.IsInRole("Admin") || User.IsInRole("Team Lead") || User.IsInRole("Manager");

        private async Task<bool> IsAssignedToRequirementAsync(int requirementId)
        {
            var userId = _userManager.GetUserId(User);
            return await _context.RequirementAssignments
                .AnyAsync(a => a.RequirementId == requirementId && a.UserId == userId);
        }
        private async Task<List<(int Id, string Name)>> GetAllSkillsAsync()
        {
            var rows = await _context.Skills
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder).ThenBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            return rows.Select(r => (r.Id, r.Name)).ToList();
        }

        public ClientRequirementsController(
        ApplicationDbContext context,
        IWebHostEnvironment env,
        UserManager<AppUser> userManager,
        INotificationService notificationService,
        IMatchScoringService matchScore)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
            _notificationService = notificationService;
            _matchScore = matchScore;
        }

        private async Task<List<string>> BuildStakeholdersAsync(int requirementId, int? resumeId = null)
        {
            var assigned = await _context.RequirementAssignments
                                         .Where(a => a.RequirementId == requirementId)
                                         .Select(a => a.UserId)
                                         .ToListAsync();

            var adminIds = (await _userManager.GetUsersInRoleAsync("Admin")).Select(u => u.Id);
            var tlIds = (await _userManager.GetUsersInRoleAsync("Team Lead")).Select(u => u.Id);
            var mgrIds = (await _userManager.GetUsersInRoleAsync("Manager")).Select(u => u.Id);

            string? uploaderId = null;
            if (resumeId.HasValue)
            {
                uploaderId = await _context.Resumes
                    .Where(r => r.Id == resumeId.Value && r.UserId != null)
                    .Select(r => r.UserId!)
                    .FirstOrDefaultAsync();
            }

            var me = _userManager.GetUserId(User);

            return assigned
                   .Concat(adminIds)
                   .Concat(tlIds)
                   .Concat(mgrIds)
                   .Concat(uploaderId is null ? Array.Empty<string>() : new[] { uploaderId })
                   .Distinct()
                   .Where(id => id != me && !string.IsNullOrWhiteSpace(id))
                   .ToList();
        }


        public async Task<IActionResult> Index(string? status = "all",int? clientId = null,int page = 1,int pageSize = 50)
        {
            status ??= "all";
            var q = _context.ClientRequirements
             .Include(r => r.Client)
             .AsQueryable();

            if (clientId.HasValue)
            {
                q = q.Where(r => r.ClientId == clientId.Value);
            }


            RequirementStatus? filter = status.ToLower() switch
            {
                "active" => RequirementStatus.Active,
                "hold" => RequirementStatus.Hold,
                "closed" => RequirementStatus.Closed,
                _ => null
            };

            if (filter.HasValue)
                q = q.Where(r => r.Status == filter.Value);

            var total = await q.CountAsync();

            var list = await q
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Status = status.ToLower();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;

            return View(list);
        }

        // GET Create?clientId=5
        public async Task<IActionResult> Create(int clientId)
        {
            var client = await _context.Clients
                .Include(c => c.Spokespersons)
                .FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null) return NotFound();

            // Load client locations
            var locs = await GetClientLocationsAsync(clientId);

            var items = locs.Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = l.Name
            }).ToList();

            // Build VM
            var vm = new ClientRequirementCreateViewModel
            {
                ClientId = client.Id,
                ClientName = client.CompanyName,
                Positions = 1,
                ClientLocations = items,

                // Optional: default spokesperson
                SelectedSpokespersonId = client.Spokespersons?.OrderBy(s => s.Name).FirstOrDefault()?.Id
            };

            // Skills for Tagify
            var skillNames = await _context.Skills
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Name)
                .Select(s => s.Name)
                .ToListAsync();
            ViewBag.SkillNamesJson = JsonSerializer.Serialize(skillNames);

            // Spokesperson dropdown list
            var spkList = (client.Spokespersons ?? new List<Spokesperson>())
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(s.Name) ? s.Email : s.Name
                })
                .ToList();

            vm.SpokespersonList = spkList;

            // Spokesperson metadata for auto-fill JS
            var spkSource = client.Spokespersons ?? new List<Spokesperson>();

            var spkMeta = spkSource
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Email,
                    s.Phone,
                    s.Designation
                })
                .ToList();

            ViewBag.SpokespersonsJson = JsonSerializer.Serialize(spkMeta);

            // Load Qualification options for multi-select
            vm.QualificationOptions = await _context.Qualifications
                .Where(q => q.IsActive)
                .OrderBy(q => q.SortOrder).ThenBy(q => q.Name)
                .Select(q => new SelectListItem
                {
                    Value = q.Id.ToString(),
                    Text = q.Name
                })
                .ToListAsync();
            // Load Notice Period options for multi-select
            vm.NoticePeriodOptions = await _context.NoticePeriodOptions
                .Where(n => n.IsActive)
                .OrderBy(n => n.SortOrder).ThenBy(n => n.Name)
                .Select(n => new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = n.Name
                })
                .ToListAsync();

            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientRequirementCreateViewModel vm)
        {
            vm.SelectedNoticePeriodOptionIds ??= new List<int>();

            // Rebuild client locations for dropdown
            var locsForClient = await GetClientLocationsAsync(vm.ClientId);
            vm.ClientLocations = locsForClient
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Name
                });

            // Reload skills for Tagify suggestions
            var skills = await _context.Skills
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Name)
                .Select(s => s.Name)
                .ToListAsync();
            ViewBag.SkillNamesJson = JsonSerializer.Serialize(skills);

            vm.QualificationOptions = await _context.Qualifications
           .Where(q => q.IsActive)
           .OrderBy(q => q.SortOrder).ThenBy(q => q.Name)
           .Select(q => new SelectListItem
           {
                Value = q.Id.ToString(),
                Text = q.Name
           })
            .ToListAsync();

            // Reload Notice Period options
            var npOptions = await _context.NoticePeriodOptions
                .Where(n => n.IsActive)
                .OrderBy(n => n.SortOrder).ThenBy(n => n.Name)
                .ToListAsync();
           
            var selectedNpIds = vm.SelectedNoticePeriodOptionIds ?? new List<int>();

            vm.NoticePeriodOptions = npOptions
                .Select(n => new SelectListItem
                {
                    Value = n.Id.ToString(),
                    Text = n.Name,
                    Selected = selectedNpIds.Contains(n.Id)
                })
                .ToList();

            // Load client + spokespersons for dropdown
            var client = await _context.Clients
                .Include(c => c.Spokespersons)
                .FirstOrDefaultAsync(c => c.Id == vm.ClientId);

            var spkSource = client?.Spokespersons ?? new List<Spokesperson>();

            vm.SpokespersonList = spkSource
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(s.Name) ? s.Email : s.Name
                })
                .ToList();

            var spkMeta = spkSource
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Email,
                    s.Phone,
                    s.Designation
                })
                .ToList();

            ViewBag.SpokespersonsJson = JsonSerializer.Serialize(spkMeta);

            // If model is invalid, restore basic vendor info & return view
            if (!ModelState.IsValid)
            {
                var allErrors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .Select(x => new
                    {
                        Field = x.Key,
                        Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    })
                    .ToList();

                if (client != null)
                {
                    vm.ClientName = client.CompanyName;
                }
                return View(vm);
            }

            //  Validate that selected JobLocationId belongs to this client
            if (vm.JobLocationId.HasValue && !locsForClient.Any(x => x.Id == vm.JobLocationId.Value))
            {
                ModelState.AddModelError(nameof(vm.JobLocationId), "Please select a valid location for this client.");

                if (client != null)
                {
                    vm.ClientName = client.CompanyName;
                }

                return View(vm);
            }

            // Resolve readable location name
            string? locationName = null;
            if (vm.JobLocationId.HasValue)
            {
                var locTuple = locsForClient.FirstOrDefault(x => x.Id == vm.JobLocationId.Value);
                locationName = locTuple.Name;
            }

            // Take skills exactly as Tagify posts them into the bound fields
            var primaryNames = (vm.SkillsPrimary ?? string.Empty).Trim();
            var secondaryNames = (vm.SkillsSecondary ?? string.Empty).Trim();

            // Build final NoticePeriod text from selected options
            var noticeNames = npOptions
                .Where(n => selectedNpIds.Contains(n.Id))
                .Select(n => n.Name)
                .ToList();

            var noticeText = string.Join(", ", noticeNames);

            // Build requirement entity (including spokesperson + vendor notes if you added them)
            var requirement = new ClientRequirement
            {
                ClientId = vm.ClientId,
                JobTitle = vm.JobTitle,
                Positions = vm.Positions,

                JobLocationId = vm.JobLocationId,
                JobLocation = locationName,

                EmploymentType = vm.EmploymentType,
                WorkShift = vm.WorkShift,

                SkillsPrimary = primaryNames,
                SkillsSecondary = secondaryNames,
                SkillsRequired = vm.SkillsRequired,

                Responsibilities = vm.Responsibilities,
                ExperienceMin = vm.ExperienceMin,
                ExperienceMax = vm.ExperienceMax,
                Education = vm.Education,
                Certifications = vm.Certifications,

                SalaryRange = vm.SalaryRange,
                BillingType = null,
                NoticePeriod = noticeText,
                BudgetNote = vm.BudgetNote,


                RequirementPriority = vm.RequirementPriority,
                Deadline = vm.Deadline,
                ExpectedJoiningDate = vm.ExpectedJoiningDate,

                ScreeningQuestions = vm.ScreeningQuestions,
                SpecialInstructions = vm.SpecialInstructions, // or VendorNotes if you mapped

                CreatedAt = DateTime.UtcNow,
                AttachmentsPath = string.Empty,

                // Optional: if you added these properties to ClientRequirement
                // SpokespersonId = vm.SelectedSpokespersonId,
                // VendorNotes = vm.VendorNotes
            };

            _context.ClientRequirements.Add(requirement);
            await _context.SaveChangesAsync(); // get requirement.Id

            // Save selected Qualifications mapping
            if (vm.SelectedQualificationIds?.Any() == true)
            {
                var qualRows = vm.SelectedQualificationIds
                    .Distinct()
                    .Select(qid => new RequirementQualification
                    {
                        RequirementId = requirement.Id,
                        QualificationId = qid
                    });

                await _context.RequirementQualifications.AddRangeAsync(qualRows);
                await _context.SaveChangesAsync();
            }


            //  Save uploaded files (if any) and notify stakeholders
            if (vm.Attachments != null && vm.Attachments.Any())
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "requirements", requirement.Id.ToString());
                Directory.CreateDirectory(uploadsRoot);

                foreach (var file in vm.Attachments)
                {
                    if (file == null || file.Length <= 0) continue;

                    var ext = Path.GetExtension(file.FileName);
                    var safeName = Path.GetRandomFileName().Replace(".", "") + ext;
                    var filePath = Path.Combine(uploadsRoot, safeName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await file.CopyToAsync(stream);

                    var relative = Path.Combine("uploads", "requirements", requirement.Id.ToString(), safeName)
                                   .Replace("\\", "/");
                    requirement.AttachmentsPath += relative + ";";
                }

                _context.ClientRequirements.Update(requirement);
                await _context.SaveChangesAsync();

                var recipients = await BuildStakeholdersAsync(requirement.Id);
                await _notificationService.NotifyAsync(
                    recipients,
                    type: "JD",
                    title: $"New JD: {requirement.JobTitle}",
                    body: $"Requirement #{requirement.Id} was created.",
                    url: Url.Action("Details", "ClientRequirements", new { id = requirement.Id }, Request.Scheme)
                );
            }

            return RedirectToAction(nameof(Index));
        }



        public IActionResult DownloadAttachment(int requirementId, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return BadRequest();

            var folder = Path.Combine(_env.WebRootPath, "uploads", "requirements", requirementId.ToString());
            var path = Path.Combine(folder, fileName);

            if (!System.IO.File.Exists(path)) return NotFound();

            var contentType = "application/octet-stream";
            return PhysicalFile(path, contentType, fileName);
        }
        // DELETE requirement (only if CLOSED, soft delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            // Ignore filters so we can find even already-deleted ones
            var req = await _context.ClientRequirements
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (req == null)
            {
                TempData["Error"] = "Requirement not found.";
                return RedirectToAction(nameof(Index));
            }

            // Only allow delete if status is Closed
            if (req.Status != RequirementStatus.Closed)
            {
                TempData["Error"] = "Only CLOSED requirements can be deleted.";
                return RedirectToAction(nameof(Index));
            }

            // If already deleted, just show info
            if (req.IsDeleted)
            {
                TempData["Info"] = "Requirement is already deleted.";
                return RedirectToAction(nameof(Index));
            }

            // Soft delete
            req.IsDeleted = true;
            req.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Requirement #{req.Id} deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: ClientRequirements/Details/
        public async Task<IActionResult> Details(int id)
        {
            var requirement = await _context.ClientRequirements
                .Include(r => r.Client)
                    .ThenInclude(c => c.Spokespersons)
                .Include(r => r.RequirementAssignments)
                    .ThenInclude(a => a.User)
                .Include(r => r.MeetingNotes)
                    .ThenInclude(m => m.CreatedByUser)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requirement == null) return NotFound();

            var allResumes = await _context.Resumes
                .OrderByDescending(r => r.UploadedAt)
                .ToListAsync();

            var linkedResumeIds = await _context.ResumeRequirementLinks
                .Where(l => l.RequirementId == id)
                .Select(l => l.ResumeId)
                .ToListAsync();

            ViewBag.AvailableResumes = allResumes
                .Where(r => !linkedResumeIds.Contains(r.Id))
                .ToList();

            ViewBag.LinkedResumes = await _context.ResumeRequirementLinks
                .Where(l => l.RequirementId == id)
                .Include(l => l.Resume)
                .Include(l => l.LinkedByUser)
                .ToListAsync();

            return View(requirement);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Team Lead,Manager")]
        public async Task<IActionResult> SetRequirementStatus(int id, RequirementStatus status)
        {
            var req = await _context.ClientRequirements.FirstOrDefaultAsync(r => r.Id == id);
            if (req == null) return NotFound();

            req.Status = status;
            req.StatusUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Return minimal JSON for the row update
            return Json(new
            {
                ok = true,
                id = req.Id,
                status = req.Status.ToString(),
                updatedAt = req.StatusUpdatedAt
            });
        }

        public async Task<IActionResult> MatchProfiles(int id)
        {
            var requirement = await _context.ClientRequirements
                .Include(r => r.Client)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requirement == null) return NotFound();

            var resumes = await _context.Resumes.ToListAsync();

            var primarySkills = (requirement.SkillsPrimary ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower()).ToList();

            var secondarySkills = (requirement.SkillsSecondary ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower()).ToList();

            var matchedResumes = resumes.Where(resume =>
            {
                if (string.IsNullOrWhiteSpace(resume.Skills)) return false;
                var resumeSkills = resume.Skills.ToLower();

                bool hasPrimary = primarySkills.All(skill =>
                    Regex.IsMatch(resumeSkills, @"\b" + Regex.Escape(skill) + @"\b", RegexOptions.IgnoreCase) ||
                    resumeSkills.Contains(skill));

                bool hasSecondary = !secondarySkills.Any() ||
                    secondarySkills.Any(skill => resumeSkills.Contains(skill));

                bool experienceOk = true;
                if (requirement.ExperienceMin.HasValue)
                    experienceOk &= resume.YearsOfExperience >= requirement.ExperienceMin.Value;
                if (requirement.ExperienceMax.HasValue)
                    experienceOk &= resume.YearsOfExperience <= requirement.ExperienceMax.Value;

                return hasPrimary && hasSecondary && experienceOk;
            }).ToList();

            ViewBag.Requirement = requirement;
            return View("MatchedProfiles", matchedResumes);
        }

        // GET AssignRecruiters
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRecruiters(int id)
        {
            var requirement = await _context.ClientRequirements
                .Include(r => r.Client)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requirement == null) return NotFound();

            var recruiters = await _userManager.GetUsersInRoleAsync("Reviewer");
            var vendors = await _userManager.GetUsersInRoleAsync("Vendor");

            ViewBag.Requirement = requirement;
            ViewBag.Users = recruiters.Concat(vendors).ToList();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRecruiters(int requirementId, List<string> userIds)
        {
            var requirement = await _context.ClientRequirements.FindAsync(requirementId);
            if (requirement == null) return NotFound();

            var current = await _context.RequirementAssignments
                .Where(a => a.RequirementId == requirementId)
                .Select(a => a.UserId)
                .ToListAsync();

            var incoming = userIds?.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList()
                          ?? new List<string>();

            var toAdd = incoming.Except(current).ToList();
            var toRemove = current.Except(incoming).ToList();

            if (toRemove.Any())
            {
                var removeRows = await _context.RequirementAssignments
                    .Where(a => a.RequirementId == requirementId && toRemove.Contains(a.UserId))
                    .ToListAsync();
                _context.RequirementAssignments.RemoveRange(removeRows);
            }

            if (toAdd.Any())
            {
                var addRows = toAdd.Select(uid => new RequirementAssignment
                {
                    RequirementId = requirementId,
                    UserId = uid
                });
                await _context.RequirementAssignments.AddRangeAsync(addRows);
            }

            await _context.SaveChangesAsync();
            // NOTIFY only newly assigned users
            if (toAdd.Any())
            {
                await _notificationService.NotifyAsync(
                    toAdd,
                    type: "Assignment",
                    title: $"Assigned to JD: {requirement.JobTitle}",
                    body: $"You have been assigned to requirement #{requirementId}.",
                    url: Url.Action("SharedProfiles", "ClientRequirements", new { id = requirementId }, Request.Scheme)
                );
            }
            TempData["Success"] = "Recruiters assigned successfully!";
            return RedirectToAction("Details", "ClientRequirements", new { id = requirementId });
        }

        [Authorize(Roles = "Admin,Reviewer,Team Lead,Manager")]
        public async Task<IActionResult> SharedProfiles(int id, CandidateStatus? statusFilter)
        {
            var sort = HttpContext.Request.Query["sort"].ToString(); 

            // Load the JD
            var requirement = await _context.ClientRequirements
                .Include(r => r.Client)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requirement == null) return NotFound();

            //Reviewers must be assigned (privileged can always view)
            if (User.IsInRole("Reviewer") && !await IsAssignedToRequirementAsync(id))
                return Forbid();

            // Start with a plain IQueryable (NO Include yet)
            IQueryable<ResumeRequirementLink> query = _context.ResumeRequirementLinks
                .Where(l => l.RequirementId == id);

            // filter FIRST
            if (statusFilter.HasValue)
            {
                query = query.Where(l => l.Status == statusFilter.Value);
            }

            //Now add the Includes (AFTER filters)
            query = query
                .Include(l => l.Resume).ThenInclude(r => r.User)
                .Include(l => l.LinkedByUser);

            //Execute the query
            var linkedResumes = await query
                .OrderByDescending(l => l.UpdatedAt)
                .ToListAsync();
            // Auto compute match scores if missing or stale 
            var now = DateTime.UtcNow;
            bool anyScored = false;

            // We already included Resume above; if not, ensure l.Resume != null
            foreach (var link in linkedResumes)
            {
                if (link.Resume == null) continue;

                bool needsRecompute =
                    !link.MatchScore.HasValue ||
                    !link.LastScoredAt.HasValue ||
                    (now - link.LastScoredAt.Value).TotalHours > 12;

                if (!needsRecompute) continue;

                var result = _matchScore.Compute(requirement, link.Resume);

                link.MatchScore = (short)result.Score;
                link.MatchBreakdownJson = result.BreakdownJson;
                link.LastScoredAt = now;

                anyScored = true;
            }

            if (anyScored)
            {
                await _context.SaveChangesAsync();
            }

            //Histories only for shown rows
            var resumeIds = linkedResumes.Select(l => l.ResumeId).ToList();
            // sort by score if requested
            if (sort.Equals("score", StringComparison.OrdinalIgnoreCase))
            {
                linkedResumes = linkedResumes
                    .OrderByDescending(l => l.MatchScore ?? -1)
                    .ThenByDescending(l => l.UpdatedAt)
                    .ToList();
            }

            var histories = await _context.CandidateStatusHistories
                .Where(h => h.RequirementId == id && resumeIds.Contains(h.ResumeId))
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();

            //Pack view data
            ViewBag.Requirement = requirement;
            ViewBag.CanChangeStatus = IsPrivilegedRole();
            ViewBag.Histories = histories
                .GroupBy(h => h.ResumeId)
                .ToDictionary(g => g.Key, g => g.Take(5).ToList());

            //Keep current filter so the view can highlight the active button
            ViewBag.StatusFilter = statusFilter;
            // For the "Submit to Panel" modal
            // For the "Submit to Panel" modal — allow Panel + Admin + Manager
            var panelUsers = await _userManager.GetUsersInRoleAsync("Panel");
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");

            // merge and distinct by Id
            var eligible = panelUsers
                .Concat(adminUsers)
                .Concat(managerUsers)
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .OrderBy(u => string.IsNullOrWhiteSpace(u.FullName) ? u.Email : u.FullName)
                .ToList();

            ViewBag.Panelists = eligible;

            return View(linkedResumes);
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Reviewer,Team Lead,Manager,Vendor")]
        public async Task<IActionResult> LinkResume(int requirementId, int resumeId)
        {
            var requirement = await _context.ClientRequirements.FindAsync(requirementId);
            var resume = await _context.Resumes.FindAsync(resumeId);

            if (requirement == null || resume == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var exists = await _context.ResumeRequirementLinks
                .AnyAsync(x => x.ResumeId == resumeId && x.RequirementId == requirementId);
            if (exists)
                return Json(new { success = false, message = "Resume already linked to this requirement." });

            var link = new ResumeRequirementLink
            {
                ResumeId = resumeId,
                RequirementId = requirementId,
                LinkedByUserId = userId
            };

            _context.ResumeRequirementLinks.Add(link);
            await _context.SaveChangesAsync();
            //  NOTIFY stakeholders
            var recipients = await BuildStakeholdersAsync(requirementId, resumeId);
            await _notificationService.NotifyAsync(
                recipients,
                type: "Upload",
                title: "New profile linked",
                body: $"A profile was linked to JD #{requirementId}.",
                url: Url.Action("SharedProfiles", "ClientRequirements", new { id = requirementId }, Request.Scheme)
            );

            return Json(new { success = true, message = "Resume linked successfully!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Team Lead,Manager")]
        public async Task<IActionResult> SetStatus(int requirementId, int resumeId, CandidateStatus status, string? comment)
        {
            var requirementExists = await _context.ClientRequirements.AnyAsync(r => r.Id == requirementId);
            if (!requirementExists) return NotFound();

            var link = await _context.ResumeRequirementLinks
                .FirstOrDefaultAsync(x => x.RequirementId == requirementId && x.ResumeId == resumeId);
            if (link == null) return NotFound();

            link.Status = status;
            if (!string.IsNullOrWhiteSpace(comment))
                link.LastComment = comment.Trim();
            link.UpdatedAt = DateTime.UtcNow;

            _context.CandidateStatusHistories.Add(new CandidateStatusHistory
            {
                ResumeId = resumeId,
                RequirementId = requirementId,
                Status = status,
                Comment = comment,
                ChangedByUserId = _userManager.GetUserId(User)!,
                ChangedAt = DateTime.UtcNow
            });

            try
            {
                await _context.SaveChangesAsync();
                //  NOTIFY stakeholders about the status change
                var recipients = await BuildStakeholdersAsync(requirementId, resumeId);
                await _notificationService.NotifyAsync(
                    recipients,
                    type: "StatusChange",
                    title: $"Status updated to {status}",
                    body: string.IsNullOrWhiteSpace(comment) ? null : comment,
                    url: Url.Action(nameof(SharedProfiles), "ClientRequirements", new { id = requirementId }, Request.Scheme)
                );
                TempData["Success"] = $"Status updated to {status}.";
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "Another user updated this record. Please refresh and try again.";
            }

            return RedirectToAction(nameof(SharedProfiles), new { id = requirementId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Team Lead,Manager,Admin")]
        public async Task<IActionResult> TLDecision(int requirementId, int resumeId, string decision, string? remark)
        {
            // 1) Guard rails
            var link = await _context.ResumeRequirementLinks
                .FirstOrDefaultAsync(x => x.RequirementId == requirementId && x.ResumeId == resumeId);
            if (link == null) return NotFound();

            // 2) Map decision -> status
            CandidateStatus newStatus = decision?.ToLower() switch
            {
                "select" => CandidateStatus.PanelShortlisted,
                "reject" => CandidateStatus.Rejected,
                "hold" => CandidateStatus.Hold,
                _ => link.Status // keep as-is if unknown
            };

            // 3) Apply changes
            link.Status = newStatus;
            if (!string.IsNullOrWhiteSpace(remark)) link.LastComment = remark.Trim();
            link.UpdatedAt = DateTime.UtcNow;

            _context.CandidateStatusHistories.Add(new CandidateStatusHistory
            {
                ResumeId = resumeId,
                RequirementId = requirementId,
                Status = newStatus,
                Comment = remark,
                ChangedByUserId = _userManager.GetUserId(User)!,
                ChangedAt = DateTime.UtcNow
            });

            try
            {
                await _context.SaveChangesAsync();

                // 4) Notify stakeholders (Admin/TL/Manager + assignees + uploader)
                var recipients = await BuildStakeholdersAsync(requirementId, resumeId);
                await _notificationService.NotifyAsync(
                    recipients,
                    type: "StatusChange",
                    title: $"TL decision: {newStatus}",
                    body: string.IsNullOrWhiteSpace(remark) ? null : remark,
                    url: Url.Action(nameof(SharedProfiles), "ClientRequirements",
                                    new { id = requirementId }, Request.Scheme)
                );

                TempData["Success"] = $"Updated to {newStatus}.";
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "Someone else updated this record. Refresh and try again.";
            }

            return RedirectToAction(nameof(SharedProfiles), new { id = requirementId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Team Lead,Manager,Reviewer")]
        public async Task<IActionResult> AssignToPanel(int requirementId, int resumeId, List<string> panelUserIds, string? remark)
        {
            // 1) validate
            var link = await _context.ResumeRequirementLinks
                .FirstOrDefaultAsync(x => x.RequirementId == requirementId && x.ResumeId == resumeId);
            if (link == null) return NotFound();

            // 2) Mark as submitted to panel if not already
            if (link.Status != CandidateStatus.PanelShortlisted)
            {
                link.Status = CandidateStatus.PanelShortlisted;
                link.LastComment = remark;
                link.UpdatedAt = DateTime.UtcNow;

                _context.CandidateStatusHistories.Add(new CandidateStatusHistory
                {
                    RequirementId = requirementId,
                    ResumeId = resumeId,
                    Status = CandidateStatus.PanelShortlisted,
                    Comment = remark,
                    ChangedByUserId = _userManager.GetUserId(User)!,
                    ChangedAt = DateTime.UtcNow
                });
            }

            // 3) Create assignments (skip duplicates thanks to unique index)
            var me = _userManager.GetUserId(User)!;
            var cleanIds = panelUserIds?.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList() ?? new();
            foreach (var pid in cleanIds)
            {
                _context.PanelAssignments.Add(new PanelAssignment
                {
                    RequirementId = requirementId,
                    ResumeId = resumeId,
                    PanelUserId = pid,
                    AssignedByUserId = me
                });
            }

            await _context.SaveChangesAsync();

            // 4) Notify panelists
            if (cleanIds.Any())
            {
                await _notificationService.NotifyAsync(
                    cleanIds,
                    type: "Panel",
                    title: "Profile assigned for screening",
                    body: $"A candidate has been submitted for screening (JD #{requirementId}).",
                    url: Url.Action("Index", "Panel", new { requirementId }, Request.Scheme)
                );
            }

            TempData["Success"] = "Submitted to panel and assignments created.";
            return RedirectToAction(nameof(SharedProfiles), new { id = requirementId });
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Reviewer,Team Lead,Manager")]
        public async Task<IActionResult> SharePreview(int id,[FromQuery] List<int>? resumeIds,bool branded = false)
        {
            // 1) Load JD (+ client) and enforce access
            var requirement = await _context.ClientRequirements
                .Include(r => r.Client)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requirement == null) return NotFound();

            // Reviewers must be assigned (privileged can always view)
            if (User.IsInRole("Reviewer") && !await IsAssignedToRequirementAsync(id))
                return Forbid();

            // 2) Base query: only rows for this JD and Panel Shortlisted
            IQueryable<ResumeRequirementLink> baseQuery = _context.ResumeRequirementLinks
                .Where(l => l.RequirementId == id && l.Status == CandidateStatus.PanelShortlisted);

            // Optional narrowing to selected resumeIds
            if (resumeIds != null && resumeIds.Any())
                baseQuery = baseQuery.Where(l => resumeIds.Contains(l.ResumeId));

            // 3) Add Includes AFTER filtering (avoid IIncludable type issues)
            var q = baseQuery
                .Include(l => l.Resume)
                .Include(l => l.LinkedByUser);

            // 4) Project to your sharing VM
            var today = DateTime.Today;
            var rows = await q
                .OrderByDescending(l => l.UpdatedAt)
                .Select(l => new ClientShareRowVm
                {
                    // Basic identity
                    ResumeId = l.ResumeId,
                    ClientName = requirement.Client.CompanyName,
                    Skill = requirement.JobTitle,
                    RowDate = today,

                    // Candidate
                    Candidate = l.Resume.Name,
                    Email = l.Resume.Email,
                    Phone = l.Resume.Phone,

                    // Optional/nullable business fields (fill later if you add them)
                    DateOfBirth = null,                        // map if you start storing DoB
                    Qualification = null,                        // map from resume if available
                    TotalYearsExp = l.Resume.YearsOfExperience,
                    RelevantYearsExp = null,                        // compute later if you add a relevancy extractor
                    CurrentCTC = null,
                    ExpectedCTC = null,
                    NoticePeriod = null,
                    CurrentCompany = null,
                    CurrentLocation = null,
                    PreferredLocation = null,

                    // Provenance
                    Source = string.IsNullOrWhiteSpace(l.LinkedByUser.FullName)
                                        ? "Xeedo"
                                        : l.LinkedByUser.FullName,

                    // Helpful extras
                    ResumeLink = Url.Action("Details", "Resume", new { id = l.ResumeId }, Request.Scheme),
                    KeySkills = l.Resume.Skills == null
                                        ? null
                                        : (l.Resume.Skills.Length > 120
                                            ? l.Resume.Skills.Substring(0, 120) + "..."
                                            : l.Resume.Skills),

                    // If you later compute & persist score on the link, you can surface it here
                    MatchScore = l.MatchScore
                })
                .ToListAsync();

            // 5) Return the appropriate partial (your views you’ve set up)
            if (branded)
                return PartialView("~/Views/Shared/_ClientShareTableBranded.cshtml", rows);

            return PartialView("~/Views/Shared/_ClientShareTable.cshtml", rows);
        }
        [Authorize(Roles = "Admin,Reviewer,Team Lead,Manager,Panel")]
        public async Task<IActionResult> FullDetails(int id)
        {
            var requirement = await _context.ClientRequirements
                .Include(r => r.Client)
                .ThenInclude(c => c.Spokespersons)
                .Include(r => r.RequirementAssignments)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requirement == null)
                return NotFound();

            return View(requirement);
        }

        private async Task<List<(int Id, string Name)>> GetClientLocationsAsync(int clientId)
        {
            // HQ location id
            var hqLocId = await _context.Clients
                .Where(c => c.Id == clientId)
                .Select(c => c.HeadquarterLocationId)
                .FirstOrDefaultAsync();

            // Other location ids
            var otherLocIds = await _context.ClientOtherLocations
                .Where(x => x.ClientId == clientId)
                .Select(x => x.LocationId)
                .ToListAsync();

            var allIds = new HashSet<int>();
            if (hqLocId.HasValue) allIds.Add(hqLocId.Value);
            foreach (var id in otherLocIds) allIds.Add(id);

            if (allIds.Count == 0) return new(); // no locations configured

            // Pull names from Locations table
            var rows = await _context.Locations
                .Where(l => allIds.Contains(l.Id))
                .OrderBy(l => l.SortOrder)
                .ThenBy(l => l.Name)
                .Select(l => new { l.Id, l.Name })
                .ToListAsync();

            return rows.Select(r => (r.Id, r.Name)).ToList();
        }

        [Authorize(Roles = "Reviewer,Admin,Team Lead,Manager")]
        public async Task<IActionResult> SharedProfilesByStatus(CandidateStatus status)
        {
            var userId = _userManager.GetUserId(User);

            var rows = await _context.ResumeRequirementLinks
                .Where(l =>
                    l.LinkedByUserId == userId &&
                    l.Status == status)
                .Include(l => l.Resume)
                .Include(l => l.Requirement)
                    .ThenInclude(r => r.Client)
                .OrderByDescending(l => l.UpdatedAt)
                .ToListAsync();

            ViewBag.StatusFilter = status;
            ViewBag.Mode = "Dashboard";

            return View("SharedProfilesDashboard", rows);
        }

    }
}