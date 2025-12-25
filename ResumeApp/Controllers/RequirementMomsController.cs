using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;
using ResumeApp.ViewModels;

namespace ResumeApp.Controllers
{
    public class RequirementMomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IWebHostEnvironment _env;

        public RequirementMomsController(
            ApplicationDbContext context,
            UserManager<Users> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // ------------------------------------------------------
        // CREATE (GET)
        // /RequirementMoms/Create?requirementId=5
        // ------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Create(int requirementId)
        {
            var requirement = await _context.ClientRequirements
                .FirstOrDefaultAsync(r => r.Id == requirementId);

            if (requirement == null)
                return NotFound();

            var model = new RequirementMom
            {
                RequirementId = requirementId,
                MeetingDate = DateTime.Today
            };

            return View(model);
        }

        // ------------------------------------------------------
        // CREATE (POST)
        // ------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RequirementMom model, List<IFormFile>? Attachments)
        {
            var requirement = await _context.ClientRequirements
                .FirstOrDefaultAsync(r => r.Id == model.RequirementId);

            if (requirement == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User)!;

            // Create NEW MOM entity (DO NOT USE the posted model directly)
            var mom = new RequirementMom
            {
                RequirementId = model.RequirementId,
                Title = model.Title?.Trim(),
                MeetingDate = model.MeetingDate,
                Minutes = model.Minutes,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
                // DO NOT set Id — DB will auto-generate it
            };

            // 1) Save MOM FIRST to get its ID
            _context.RequirementMoms.Add(mom);
            await _context.SaveChangesAsync();   // mom.Id now available

            // 2) Handle attachments
            if (Attachments != null && Attachments.Any())
            {
                var root = Path.Combine(
                    _env.WebRootPath,
                    "uploads",
                    "requirement-moms",
                    mom.RequirementId.ToString(),
                    mom.Id.ToString());

                Directory.CreateDirectory(root);

                foreach (var file in Attachments)
                {
                    if (file == null || file.Length == 0) continue;

                    var ext = Path.GetExtension(file.FileName);
                    var safeName = Path.GetRandomFileName().Replace(".", "") + ext;
                    var fullPath = Path.Combine(root, safeName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    mom.AttachmentsPath += safeName + ";";
                }

                // UPDATE the existing MOM — do NOT Add() again
                _context.RequirementMoms.Update(mom);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "MOM created successfully.";
            return RedirectToAction("Details", "ClientRequirements", new { id = mom.RequirementId });
        }


        // ------------------------------------------------------
        // EDIT (GET)
        // /RequirementMoms/Edit/5
        // ------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var mom = await _context.RequirementMoms
                .Include(m => m.Requirement)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mom == null)
                return NotFound();

            return View(mom);
        }

        // ------------------------------------------------------
        // EDIT (POST)
        // ------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            RequirementMom formModel,
            List<IFormFile>? Attachments)
        {
            if (id != formModel.Id)
                return NotFound();

            var mom = await _context.RequirementMoms
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mom == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View(formModel);
            }

            var userId = _userManager.GetUserId(User)!;

            // --- Create history snapshot BEFORE applying changes ---
            var history = new RequirementMomHistory
            {
                RequirementMomId = mom.Id,
                EditedAt = DateTime.UtcNow,
                EditedByUserId = userId,
                NotesHtml = mom.Minutes,              // snapshot of previous text
                AttachmentsPath = mom.AttachmentsPath // snapshot of previous attachments
            };
            _context.RequirementMomHistories.Add(history);

            // --- Update current MOM ---
            mom.Title = formModel.Title;
            mom.MeetingDate = formModel.MeetingDate;
            mom.Minutes = formModel.Minutes;
            mom.LastEditedByUserId = userId;
            mom.LastEditedAt = DateTime.UtcNow;

            // Handle *new* attachments (append)
            if (Attachments != null && Attachments.Any())
            {
                var root = Path.Combine(
                    _env.WebRootPath,
                    "uploads",
                    "requirement-moms",
                    mom.RequirementId.ToString(),
                    mom.Id.ToString());

                Directory.CreateDirectory(root);

                foreach (var file in Attachments)
                {
                    if (file == null || file.Length <= 0) continue;

                    var ext = Path.GetExtension(file.FileName);
                    var safeName = Path.GetRandomFileName().Replace(".", "") + ext;
                    var filePath = Path.Combine(root, safeName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    mom.AttachmentsPath += safeName + ";";
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "MOM updated successfully.";
            return RedirectToAction("Details", "ClientRequirements", new { id = mom.RequirementId });
        }

        // ------------------------------------------------------
        // HISTORY VIEW
        // /RequirementMoms/History/5
        // ------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> History(int id)
        {
            var mom = await _context.RequirementMoms
                .Include(m => m.Requirement)
                .Include(m => m.History)
                    .ThenInclude(h => h.EditedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mom == null)
                return NotFound();

            // You can make a strongly-typed view for this;
            // for now we'll pass the MOM with its History.
            return View(mom);
        }

        // ------------------------------------------------------
        // DOWNLOAD ATTACHMENT
        // /RequirementMoms/DownloadAttachment/5?fileName=abc.pdf
        // id = MOM Id
        // ------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> DownloadAttachment(int id, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest();

            var mom = await _context.RequirementMoms
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mom == null)
                return NotFound();

            var root = Path.Combine(
                _env.WebRootPath,
                "uploads",
                "requirement-moms",
                mom.RequirementId.ToString(),
                mom.Id.ToString());

            var path = Path.Combine(root, fileName);

            if (!System.IO.File.Exists(path))
                return NotFound();

            var contentType = "application/octet-stream";
            return PhysicalFile(path, contentType, fileName);
        }
    }
}
