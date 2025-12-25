using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ResumeApp.Data;
using ResumeApp.Models;
using ResumeApp.ViewModels;
using System.Drawing;

namespace ResumeApp.Controllers
{
    [Authorize(Roles = "Reviewer,Admin")]

    public class UserManagementController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserManagementController(
        UserManager<Users> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> ActivityLogs(int page = 1, int pageSize = 100)
        {
            var query = _context.ActivityLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp);

            int totalCount = await query.CountAsync();

            var logs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            foreach (var log in logs)
            {
                log.Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc);
                log.Timestamp = TimeZoneInfo.ConvertTimeFromUtc(log.Timestamp, istZone);
            }

            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;

            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> ExportActivityLogsPdf()
        {
            var logs = await _context.ActivityLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Activity Logs");

            // Headers
            worksheet.Cells[1, 1].Value = "Date & Time";
            worksheet.Cells[1, 2].Value = "User";
            worksheet.Cells[1, 3].Value = "Action";
            worksheet.Cells[1, 4].Value = "Details";

            using (var range = worksheet.Cells[1, 1, 1, 4])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            // Data
            int row = 2;
            foreach (var log in logs)
            {
                worksheet.Cells[row, 1].Value = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cells[row, 2].Value = log.User?.FullName ?? "System";
                worksheet.Cells[row, 3].Value = log.ActionType ?? "";
                worksheet.Cells[row, 4].Value = log.Message ?? "";
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            var fileBytes = package.GetAsByteArray();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ActivityLogs.xlsx");
        }
        //List all users
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserWithRolesViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = string.Join(", ", roles),
                    IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow,
                    IsApproved = user.IsApproved
                });
            }

            return View(model);
        }

        // Edit user roles
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            var model = new EditUserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = allRoles.Select(r => new RoleSelection
                {
                    RoleName = r,
                    IsSelected = roles.Contains(r)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName);

            var result = await _userManager.RemoveFromRolesAsync(user, userRoles);
            if (!result.Succeeded)
                return View(model);

            result = await _userManager.AddToRolesAsync(user, selectedRoles);
            if (!result.Succeeded)
                return View(model);

            return RedirectToAction("Index");
        }
        // Lock user account
        [HttpPost]
        public async Task<IActionResult> Lock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(100));
            return RedirectToAction("Index");
        }
        // Unlock user account
        [HttpPost]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> ManageUsers()
        {
            var pendingUsers = await _userManager.Users
                .Where(u => !u.IsApproved)
                .ToListAsync();

            return View(pendingUsers);
        }
        // Approve user for login
        [HttpPost]
        public async Task<IActionResult> Approve(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsApproved = true;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }
        // Deny (delete) user
        [HttpPost]
        public async Task<IActionResult> Deny(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsDenied = true;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("ManageUsers");
        }
        // Show Upload Stats (Today / This Month)
        public async Task<IActionResult> UploadStats(string range = "today")
        {
            var query = _context.Resumes.Include(r => r.User).AsQueryable();
            DateTime now = DateTime.Now;

            if (range.Equals("today", StringComparison.OrdinalIgnoreCase))
            {
                var start = now.Date;
                var end = start.AddDays(1);
                query = query.Where(r => r.UploadedAt >= start && r.UploadedAt < end);
            }
            else if (range.Equals("month", StringComparison.OrdinalIgnoreCase))
            {
                var start = new DateTime(now.Year, now.Month, 1);
                var end = start.AddMonths(1);
                query = query.Where(r => r.UploadedAt >= start && r.UploadedAt < end);
            }
            var stats = await query
                .GroupBy(r => new { r.UserId, r.User!.FullName, r.User!.Email })
                .Select(g => new UserUploadStatsViewModel
                {
                    UserId = g.Key.UserId ?? "",
                    FullName = g.Key.FullName ?? "Unknown",
                    Email = g.Key.Email ?? "N/A",
                    UploadCount = g.Count()
                })
                .OrderByDescending(x => x.UploadCount)
                .ToListAsync();
            ViewBag.Range = range;
            return View(stats);
        }
        [HttpGet]
        public async Task<IActionResult> ExportUploadStats(string range = "today")
        {
            var query = _context.Resumes.Include(r => r.User).AsQueryable();
            DateTime now = DateTime.Now;

            if (range == "today")
            {
                var start = now.Date;
                var end = start.AddDays(1);
                query = query.Where(r => r.UploadedAt >= start && r.UploadedAt < end);
            }
            else if (range == "month")
            {
                var start = new DateTime(now.Year, now.Month, 1);
                var end = start.AddMonths(1);
                query = query.Where(r => r.UploadedAt >= start && r.UploadedAt < end);
            }
            var stats = await query
                .GroupBy(r => new { r.UserId, r.User!.FullName, r.User!.Email })
                .Select(g => new UserUploadStatsViewModel
                {
                    UserId = g.Key.UserId ?? "",
                    FullName = g.Key.FullName ?? "Unknown",
                    Email = g.Key.Email ?? "N/A",
                    UploadCount = g.Count()
                })
                .OrderByDescending(x => x.UploadCount)
                .ToListAsync();

            return ExportStatsToExcel(stats, range);
        }

        // Excel Export
        private FileResult ExportStatsToExcel(List<UserUploadStatsViewModel> stats, string range)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Upload Stats");

            ws.Cells[1, 1].Value = "Full Name";
            ws.Cells[1, 2].Value = "Email";
            ws.Cells[1, 3].Value = "Resumes Uploaded";

            using (var header = ws.Cells[1, 1, 1, 3])
            {
                header.Style.Font.Bold = true;
                header.Style.Fill.PatternType = ExcelFillStyle.Solid;
                header.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            int row = 2;
            foreach (var s in stats)
            {
                ws.Cells[row, 1].Value = s.FullName;
                ws.Cells[row, 2].Value = s.Email;
                ws.Cells[row, 3].Value = s.UploadCount;
                row++;
            }
            ws.Cells.AutoFitColumns();
            return File(package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"UploadStats_{range}.xlsx");
        }

        private async Task LoadCountryLocationListsAsync()
        {
            ViewBag.HeadquarterCountries = await _context.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            ViewBag.HeadquarterLocations = new List<object>();
        }
        [HttpGet]
        public async Task<IActionResult> ClientOnboarding()
        {
            await LoadCountryLocationDesignationListsAsync();
            var vm = new ClientOnboardingViewModel
            {
                DocumentItems = await BuildDocumentItemsAsync()
            };
            return View(vm);
        }

        private async Task LoadCountryLocationDesignationListsAsync()
        {
            // Countries (active only)
            ViewBag.HeadquarterCountries = await _context.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            // Locations are country dependent (keep empty for now)
            ViewBag.HeadquarterLocations = new List<object>();

            // Designations master (active only)
            ViewBag.Designations = await _context.Designations
                .Where(d => d.IsActive)
                .OrderBy(d => d.SortOrder).ThenBy(d => d.Name)
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();
        }
        private async Task<List<DocumentUploadItemVM>> BuildDocumentItemsAsync()
        {
            return await _context.DocumentTypes
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new DocumentUploadItemVM
                {
                    DocumentTypeId = x.Id,
                    DocumentTypeName = x.Name,
                    IsMandatory = x.IsMandatory
                })
                .ToListAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClientOnboarding(ClientOnboardingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCountryLocationDesignationListsAsync();
                model.DocumentItems = await BuildDocumentItemsAsync();
                return View(model);
            }

            // Join checkbox collections
            var engagement = (model.EngagementTypes != null && model.EngagementTypes.Any())
                ? string.Join(", ", model.EngagementTypes)
                : string.Empty;

            var preferredComms = (model.PreferredCommunication != null && model.PreferredCommunication.Any())
                ? string.Join(", ", model.PreferredCommunication)
                : string.Empty;

            // Resolve primary contact designation
            string? primaryDesignation = null;
            if (model.DesignationId.HasValue)
            {
                primaryDesignation = await _context.Designations
                    .Where(d => d.Id == model.DesignationId.Value)
                    .Select(d => d.Name)
                    .FirstOrDefaultAsync();
            }

            // Resolve all spokesperson designation IDs in one query
            var spIds = model.Spokespersons?
                .Where(s => s.DesignationId.HasValue)
                .Select(s => s.DesignationId!.Value)
                .Distinct()
                .ToList() ?? new List<int>();

            var desigMap = spIds.Count == 0
                ? new Dictionary<int, string>()
                : await _context.Designations
                    .Where(d => spIds.Contains(d.Id))
                    .ToDictionaryAsync(d => d.Id, d => d.Name);

            // Build Client
            var client = new Client
            {
                CompanyName = model.CompanyName?.Trim(),
                WebsiteUrl = model.WebsiteUrl?.Trim(),
                CompanyType = model.CompanyType,
                CompanySize = model.CompanySize,

                ContactName = model.ContactName,
                Designation = primaryDesignation, // store resolved name
                Phone = model.Phone,
                Email = model.Email,
                PreferredCommunication = preferredComms,

                EngagementTypes = engagement,
                AcceptTerms = model.AcceptTerms,
                IsActive = true,

                HeadquarterCountryId = model.HeadquarterCountryId,
                HeadquarterLocationId = model.HeadquarterLocationId,

                Spokespersons = new List<Spokesperson>(),
                Documents = new List<ClientDocument>()
            };

            // Primary contact also as first spokesperson (optional)
            client.Spokespersons.Add(new Spokesperson
            {
                Name = model.ContactName,
                Designation = primaryDesignation,
                Phone = model.Phone,
                Email = model.Email,
                PreferredCommunication = preferredComms
            });

            // Extra Spokespersons
            if (model.Spokespersons != null && model.Spokespersons.Any())
            {
                foreach (var sp in model.Spokespersons)
                {
                    string? spDesig = null;
                    if (sp.DesignationId.HasValue && desigMap.TryGetValue(sp.DesignationId.Value, out var name))
                        spDesig = name;

                    client.Spokespersons.Add(new Spokesperson
                    {
                        Name = sp.Name,
                        Designation = spDesig,
                        Phone = sp.Phone,
                        Email = sp.Email,
                        PreferredCommunication =
                            (sp.PreferredCommunication != null && sp.PreferredCommunication.Any())
                                ? string.Join(", ", sp.PreferredCommunication)
                                : string.Empty
                    });
                }
            }

            // Save client to get Id
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            // Other locations
            if (model.OtherLocationIds != null && model.OtherLocationIds.Count > 0)
            {
                var rows = model.OtherLocationIds
                    .Distinct()
                    .Select(locId => new ClientOtherLocation
                    {
                        ClientId = client.Id,
                        LocationId = locId
                    });

                await _context.ClientOtherLocations.AddRangeAsync(rows);
                await _context.SaveChangesAsync();
            }

            // =========================
            // Master-driven multiple document uploads + mandatory validation
            // =========================
            var activeTypes = await _context.DocumentTypes
                .Where(x => x.IsActive)
                .Select(x => new { x.Id, x.IsMandatory })
                .ToListAsync();

            var mandatoryIds = activeTypes
                .Where(t => t.IsMandatory)
                .Select(t => t.Id)
                .ToHashSet();

            var missingMandatory = new List<int>();
            foreach (var reqId in mandatoryIds)
            {
                var row = model.DocumentItems?.FirstOrDefault(d => d.DocumentTypeId == reqId);
                if (row == null || row.File == null || row.File.Length == 0)
                    missingMandatory.Add(reqId);
            }
            if (missingMandatory.Count > 0)
            {
                ModelState.AddModelError("", "Please upload all mandatory documents.");
                await LoadCountryLocationDesignationListsAsync();
                model.DocumentItems = await BuildDocumentItemsAsync();
                return View(model);
            }

            // Declare 'root' ONCE and reuse it everywhere
            var root = Path.Combine(_env.WebRootPath, "uploads", "clients", client.Id.ToString());
            Directory.CreateDirectory(root);

            // Save master-driven document items
            if (model.DocumentItems != null && model.DocumentItems.Count > 0)
            {
                foreach (var item in model.DocumentItems)
                {
                    if (item.File == null || item.File.Length == 0) continue;

                    var safeName = Path.GetFileName(item.File.FileName);
                    var destPath = Path.Combine(root, safeName);
                    using (var fs = new FileStream(destPath, FileMode.Create))
                        await item.File.CopyToAsync(fs);

                    _context.ClientDocumentItems.Add(new ClientDocumentItem
                    {
                        ClientId = client.Id,
                        DocumentTypeId = item.DocumentTypeId,
                        FilePath = $"/uploads/clients/{client.Id}/{safeName}".Replace("\\", "/"),
                        UploadedOn = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

            // =========================
            // Legacy single-file fields (NDA/MSA/Presentation) - optional
            // Reuse the SAME 'root' variable; DO NOT redeclare it.
            // =========================
            var docs = new ClientDocument();
            var hasDocs = false;

            async Task<string?> SaveFileAsync(IFormFile? file)
            {
                if (file == null || file.Length == 0) return null;
                var safeName = Path.GetFileName(file.FileName);
                var destPath = Path.Combine(root, safeName);
                using (var fs = new FileStream(destPath, FileMode.Create))
                    await file.CopyToAsync(fs);
                return $"/uploads/clients/{client.Id}/{safeName}".Replace("\\", "/");
            }

            var nda = await SaveFileAsync(model.NDAFile);
            if (nda != null) { docs.NDAPath = nda; hasDocs = true; }

            var msa = await SaveFileAsync(model.MSAFile);
            if (msa != null) { docs.MSAPath = msa; hasDocs = true; }

            var corp = await SaveFileAsync(model.CorporatePresentationFile);
            if (corp != null) { docs.CorporatePresentationPath = corp; hasDocs = true; }

            if (!string.IsNullOrWhiteSpace(model.CorporatePresentationText))
            {
                docs.CorporatePresentationText = model.CorporatePresentationText;
                hasDocs = true;
            }

            if (hasDocs)
            {
                docs.ClientId = client.Id;
                _context.ClientDocuments.Add(docs);
                await _context.SaveChangesAsync();
            }

            TempData["OnboardSuccess"] = "1";
            TempData["Success"] = "Client onboarded successfully!";
            return RedirectToAction(nameof(ClientList));
        }


        // put this helper inside the same controller (private)
        private static string SanitizeFolder(string name)
        {
            foreach (var ch in Path.GetInvalidFileNameChars())
                name = name.Replace(ch, '-');
            return name.Trim();
        }


        public async Task<IActionResult> ClientList()
        {
            // Fetch clients with existing includes 
            var clients = await _context.Clients
                .Include(c => c.Spokespersons)
                .Include(c => c.Documents)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            // Fetch requirement counts grouped by ClientId
            var reqCounts = await _context.ClientRequirements
                .GroupBy(r => r.ClientId)
                .Select(g => new { ClientId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ClientId, x => x.Count);

            // Pass the count dictionary to the view
            ViewBag.RequirementCounts = reqCounts;

            return View(clients);
        }



    }
}
