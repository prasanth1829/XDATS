using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;
using ResumeApp.ViewModels;
using System.Text.RegularExpressions;

namespace ResumeApp.Controllers
{
    [Authorize(Roles = "Reviewer,Admin")]

    public class ClientRequirementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ClientRequirementsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            var list = await _context.ClientRequirements
                .Include(r => r.Client)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(list);
        }
        // GET Create?clientId=5
        public async Task<IActionResult> Create(int clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null) return NotFound();

            var vm = new ClientRequirementCreateViewModel
            {
                ClientId = client.Id,
                ClientName = client.CompanyName,
                ContactName = client.ContactName,
                ContactEmail = client.Email,
                ContactNumber = client.Phone,
                Positions = 1
            };
            return View(vm);
        }
        // POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientRequirementCreateViewModel vm)
        {
            Console.WriteLine("🚀 POST Create fired!");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState invalid:");
                foreach (var e in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine($" - {e.ErrorMessage}");

                // Re-populate vendor readonly fields if we lost them
                var client = await _context.Clients.FindAsync(vm.ClientId);
                if (client != null)
                {
                    vm.ClientName = client.CompanyName;
                    vm.ContactName = client.ContactName;
                    vm.ContactEmail = client.Email;
                    vm.ContactNumber = client.Phone;
                }
                return View(vm);
            }
            Console.WriteLine(" ModelState valid, saving requirement...");

            // Map ViewModel -> Entity
            var requirement = new ClientRequirement
            {
                ClientId = vm.ClientId,
                JobTitle = vm.JobTitle,
                Positions = vm.Positions,
                JobLocation = vm.JobLocation,
                EmploymentType = vm.EmploymentType,
                WorkShift = vm.WorkShift,
                SkillsPrimary = vm.SkillsPrimary,
                SkillsSecondary = vm.SkillsSecondary,
                SkillsRequired = vm.SkillsRequired,
                Responsibilities = vm.Responsibilities,
                ExperienceMin = vm.ExperienceMin,
                ExperienceMax = vm.ExperienceMax,
                Education = vm.Education,
                Certifications = vm.Certifications,
                SalaryRange = vm.SalaryRange,
                BillingType = vm.BillingType,
                NoticePeriod = vm.NoticePeriod,
                RequirementPriority = vm.RequirementPriority,
                Deadline = vm.Deadline,
                ExpectedJoiningDate = vm.ExpectedJoiningDate,
                ScreeningQuestions = vm.ScreeningQuestions,
                SpecialInstructions = vm.SpecialInstructions,
                CreatedAt = DateTime.UtcNow,
                AttachmentsPath = ""
            };

            // Save the requirement first to get its Id (simpler to organize files by Id)
            _context.ClientRequirements.Add(requirement);
            await _context.SaveChangesAsync(); // requirement.Id now populated

            // Save uploaded files if any
            if (vm.Attachments != null && vm.Attachments.Any())
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "requirements", requirement.Id.ToString());
                Directory.CreateDirectory(uploadsRoot);

                foreach (var file in vm.Attachments)
                {
                    if (file.Length <= 0) continue;

                    // sanitize and unique filename
                    var ext = Path.GetExtension(file.FileName);
                    var safeName = Path.GetRandomFileName().Replace(".", "") + ext;
                    var filePath = Path.Combine(uploadsRoot, safeName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // store relative path (for link)
                    var relative = Path.Combine("uploads", "requirements", requirement.Id.ToString(), safeName).Replace("\\", "/");
                    requirement.AttachmentsPath += relative + ";";
                }

                // update record with attachments paths
                _context.ClientRequirements.Update(requirement);
                await _context.SaveChangesAsync();
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


        // GET: ClientRequirements/Details/
        public async Task<IActionResult> Details(int id)
        {
            var requirement = await _context.ClientRequirements
                .Include(r => r.Client) // load client info
                .ThenInclude(c => c.Spokespersons) // include spokespeople
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requirement == null) return NotFound();

            return View(requirement);
        }

        public async Task<IActionResult> MatchProfiles(int id)
        {
            //  Load requirement
            var requirement = await _context.ClientRequirements
                .Include(r => r.Client)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requirement == null) return NotFound();

            //  Get all resumes
            var resumes = await _context.Resumes.ToListAsync();

            // Prepare required skills
            var primarySkills = (requirement.SkillsPrimary ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => s.Trim().ToLower()).ToList();
            var secondarySkills = (requirement.SkillsSecondary ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(s => s.Trim().ToLower()).ToList();

            //  Match resumes
            var matchedResumes = resumes.Where(resume =>
            {
                if (string.IsNullOrWhiteSpace(resume.Skills)) return false;

                var resumeSkills = resume.Skills.ToLower();

                //  Must contain ALL primary skills (fuzzy/partial match allowed)
                bool hasPrimary = primarySkills.All(skill =>
                    Regex.IsMatch(resumeSkills, @"\b" + Regex.Escape(skill) + @"\b", RegexOptions.IgnoreCase) ||
                    resumeSkills.Contains(skill));

                //  Secondary skills are optional (at least one)
                bool hasSecondary = !secondarySkills.Any() ||
                    secondarySkills.Any(skill => resumeSkills.Contains(skill));

                //  Experience filtering
                bool experienceOk = true;
                if (requirement.ExperienceMin.HasValue)
                    experienceOk &= resume.YearsOfExperience >= requirement.ExperienceMin.Value;
                if (requirement.ExperienceMax.HasValue)
                    experienceOk &= resume.YearsOfExperience <= requirement.ExperienceMax.Value;

                return hasPrimary && hasSecondary && experienceOk;
            }).ToList();

            // 5. Pass to view
            ViewBag.Requirement = requirement;
            return View("MatchedProfiles", matchedResumes);
        }
    }
}