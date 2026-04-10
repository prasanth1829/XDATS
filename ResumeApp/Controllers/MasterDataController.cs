using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;
using ResumeApp.Models.Master;
namespace ResumeApp.Controllers
{
    public class MasterDataController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        public MasterDataController(ApplicationDbContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<IActionResult> CountryList()
        {
            var items = await _ctx.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return Json(items);
        }

        [HttpGet]
        public async Task<IActionResult> LocationsByCountry(int countryId)
        {
            var items = await _ctx.Locations
                .Where(l => l.CountryId == countryId && l.IsActive)
                .OrderBy(l => l.SortOrder).ThenBy(l => l.Name)
                .Select(l => new { l.Id, l.Name })
                .ToListAsync();

            return Json(items);
        }
        [HttpGet]
        public async Task<IActionResult> CountriesJson()
        {
            var rows = await _ctx.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return Json(rows);
        }

        [HttpGet]
        public IActionResult CreateCountry()
        {
            return View(new Country { IsActive = true, SortOrder = 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCountry(Country model)
        {
            if (!ModelState.IsValid) return View(model);

            // Basic duplicate guard (case-insensitive)
            var exists = await _ctx.Countries
                .AnyAsync(c => c.Name.ToLower() == model.Name.Trim().ToLower());
            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), "Country already exists.");
                return View(model);
            }

            model.Name = model.Name.Trim();
            model.IsoCode = string.IsNullOrWhiteSpace(model.IsoCode) ? null : model.IsoCode.Trim().ToUpper();
            model.CreatedAt = DateTime.UtcNow;

            _ctx.Countries.Add(model);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Country created.";
            return RedirectToAction(nameof(Countries));
        }
        [HttpGet]
        public async Task<IActionResult> Countries()
        {
            var items = await _ctx.Countries
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
            return View(items);
        }

        // ---------- Locations ----------
        [HttpGet]
        public async Task<IActionResult> CreateLocation()
        {
            await LoadCountriesAsync();
            return View(new Location { IsActive = true, SortOrder = 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocation(Location model)
        {
            await LoadCountriesAsync();

            if (!ModelState.IsValid) return View(model);

            // Ensure country exists
            var countryExists = await _ctx.Countries.AnyAsync(c => c.Id == model.CountryId && c.IsActive);
            if (!countryExists)
            {
                ModelState.AddModelError(nameof(model.CountryId), "Invalid or inactive country.");
                return View(model);
            }

            // Duplicate guard: same Country + Name
            var dup = await _ctx.Locations.AnyAsync(l =>
                l.CountryId == model.CountryId &&
                l.Name.ToLower() == model.Name.Trim().ToLower());
            if (dup)
            {
                ModelState.AddModelError(nameof(model.Name), "Location already exists in this country.");
                return View(model);
            }

            model.Name = model.Name.Trim();
            model.StateOrProvince = string.IsNullOrWhiteSpace(model.StateOrProvince) ? null : model.StateOrProvince.Trim();
            model.Timezone = string.IsNullOrWhiteSpace(model.Timezone) ? null : model.Timezone.Trim();
            model.CreatedAt = DateTime.UtcNow;

            _ctx.Locations.Add(model);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Location created.";
            return RedirectToAction(nameof(Locations));
        }

        [HttpGet]
        public async Task<IActionResult> Locations()
        {
            var items = await _ctx.Locations
                .Include(l => l.Country)
                .OrderBy(l => l.Country!.Name)
                .ThenBy(l => l.SortOrder)
                .ThenBy(l => l.Name)
                .ToListAsync();
            return View(items);
        }

        // Helpers
        private async Task LoadCountriesAsync()
        {
            ViewBag.Countries = await _ctx.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.CountryCount = await _ctx.Countries.CountAsync();
            ViewBag.LocationCount = await _ctx.Locations.CountAsync();
            ViewBag.DesignationCount = await _ctx.Designations.CountAsync();
            ViewBag.DocumentTypeCount = await _ctx.DocumentTypes.CountAsync();
            ViewBag.SkillCount = await _ctx.Skills.CountAsync();
            ViewBag.QualificationCount = await _ctx.Qualifications.CountAsync();
            ViewBag.NoticePeriodCount = await _ctx.NoticePeriodOptions.CountAsync();

            return View();
        }


        // ---------- Countries: Edit ----------
        [HttpGet]
        public async Task<IActionResult> EditCountry(int id)
        {
            var item = await _ctx.Countries.FirstOrDefaultAsync(c => c.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCountry(int id, Country model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            // duplicate guard
            var dup = await _ctx.Countries
                .AnyAsync(c => c.Id != id && c.Name.ToLower() == model.Name.Trim().ToLower());
            if (dup)
            {
                ModelState.AddModelError(nameof(model.Name), "Another country with the same name exists.");
                return View(model);
            }

            var item = await _ctx.Countries.FirstOrDefaultAsync(c => c.Id == id);
            if (item == null) return NotFound();

            item.Name = model.Name.Trim();
            item.IsoCode = string.IsNullOrWhiteSpace(model.IsoCode) ? null : model.IsoCode.Trim().ToUpper();
            item.IsActive = model.IsActive;
            item.SortOrder = model.SortOrder;
            item.UpdatedAt = DateTime.UtcNow;

            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Country updated.";
            return RedirectToAction(nameof(Countries));
        }

        // ---------- Countries: Delete ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var item = await _ctx.Countries.FirstOrDefaultAsync(c => c.Id == id);
            if (item == null) return NotFound();

            var inUse = await _ctx.Locations.AnyAsync(l => l.CountryId == id);
            if (inUse)
            {
                TempData["Error"] = "Cannot delete: locations exist under this country. Remove/move them first.";
                return RedirectToAction(nameof(Countries));
            }

            _ctx.Countries.Remove(item);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Country deleted.";
            return RedirectToAction(nameof(Countries));
        }
        // ---------- Edit Location ----------
        [HttpGet]
        public async Task<IActionResult> EditLocation(int id)
        {
            var loc = await _ctx.Locations.FirstOrDefaultAsync(l => l.Id == id);
            if (loc == null) return NotFound();

            ViewBag.Countries = await _ctx.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return View(loc);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLocation(int id, Location model)
        {
            if (id != model.Id) return NotFound();

            ViewBag.Countries = await _ctx.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            if (!ModelState.IsValid) return View(model);

            var loc = await _ctx.Locations.FirstOrDefaultAsync(l => l.Id == id);
            if (loc == null) return NotFound();

            // Ensure country exists
            var countryExists = await _ctx.Countries.AnyAsync(c => c.Id == model.CountryId && c.IsActive);
            if (!countryExists)
            {
                ModelState.AddModelError(nameof(model.CountryId), "Invalid or inactive country.");
                return View(model);
            }

            var dup = await _ctx.Locations.AnyAsync(l =>
                l.Id != model.Id &&
                l.CountryId == model.CountryId &&
                l.Name.ToLower() == (model.Name ?? "").Trim().ToLower());
            if (dup)
            {
                ModelState.AddModelError(nameof(model.Name), "Location already exists in this country.");
                return View(model);
            }

            loc.Name = (model.Name ?? "").Trim();
            loc.StateOrProvince = string.IsNullOrWhiteSpace(model.StateOrProvince) ? null : model.StateOrProvince.Trim();
            loc.CountryId = model.CountryId;
            loc.SortOrder = model.SortOrder;
            loc.IsActive = model.IsActive;
            loc.UpdatedAt = DateTime.UtcNow;

            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Location updated.";
            return RedirectToAction(nameof(Locations));
        }

        // ---------- Delete Location ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var loc = await _ctx.Locations.FirstOrDefaultAsync(l => l.Id == id);
            if (loc == null)
            {
                TempData["Error"] = "Location not found.";
                return RedirectToAction(nameof(Locations));
            }

            // Guard: prevent delete if referenced by clients
            bool usedAsHQ = await _ctx.Clients.AnyAsync(c => c.HeadquarterLocationId == id);
            bool usedAsOther = await _ctx.ClientOtherLocations.AnyAsync(x => x.LocationId == id);

            if (usedAsHQ || usedAsOther)
            {
                TempData["Error"] = "Cannot delete. Location is referenced by one or more clients.";
                return RedirectToAction(nameof(Locations));
            }

            _ctx.Locations.Remove(loc);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Location deleted.";
            return RedirectToAction(nameof(Locations));
        }
        // LIST Designations
        [HttpGet]
        public async Task<IActionResult> Designations()
        {
            var items = await _ctx.Designations
                .OrderBy(d => d.SortOrder)
                .ThenBy(d => d.Name)
                .ToListAsync();
            return View(items);
        }

        // JSON for dropdowns
        [HttpGet]
        public async Task<IActionResult> DesignationList()
        {
            var items = await _ctx.Designations
                .Where(d => d.IsActive)
                .OrderBy(d => d.SortOrder).ThenBy(d => d.Name)
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();

            return Json(items);
        }

        // CREATE
        [HttpGet]
        public IActionResult CreateDesignation()
            => View(new Designation { IsActive = true, SortOrder = 0 });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDesignation(Designation model)
        {
            if (!ModelState.IsValid) return View(model);

            var name = model.Name?.Trim() ?? "";
            var exists = await _ctx.Designations
                .AnyAsync(d => d.Name.ToLower() == name.ToLower());
            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), "Designation already exists.");
                return View(model);
            }

            model.Name = name;
            model.CreatedAt = DateTime.UtcNow;
            _ctx.Designations.Add(model);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Designation created.";
            return RedirectToAction(nameof(Designations));
        }

        // EDIT
        [HttpGet]
        public async Task<IActionResult> EditDesignation(int id)
        {
            var item = await _ctx.Designations.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDesignation(int id, Designation model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var item = await _ctx.Designations.FindAsync(id);
            if (item == null) return NotFound();

            var name = model.Name?.Trim() ?? "";
            var dup = await _ctx.Designations
                .AnyAsync(d => d.Id != id && d.Name.ToLower() == name.ToLower());
            if (dup)
            {
                ModelState.AddModelError(nameof(model.Name), "Another designation with this name exists.");
                return View(model);
            }

            item.Name = name;
            item.IsActive = model.IsActive;
            item.SortOrder = model.SortOrder;
            item.UpdatedAt = DateTime.UtcNow;

            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Designation updated.";
            return RedirectToAction(nameof(Designations));
        }

        // DELETE (soft-check to avoid breaking references)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDesignation(int id)
        {
            var item = await _ctx.Designations.FindAsync(id);
            if (item == null) return NotFound();

            var inUse = await _ctx.Clients.AnyAsync(c => c.Designation == item.Name)
                     || await _ctx.Spokespersons.AnyAsync(s => s.Designation == item.Name);

            if (inUse)
            {
                TempData["Error"] = "Cannot delete: designation is referenced by clients/spokespersons.";
                return RedirectToAction(nameof(Designations));
            }

            _ctx.Designations.Remove(item);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Designation deleted.";
            return RedirectToAction(nameof(Designations));
        }
        // ---------- Document Types ----------
        [HttpGet]
        public async Task<IActionResult> DocumentTypes()
        {
            var items = await _ctx.DocumentTypes
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .ToListAsync();
            return View(items);
        }

        [HttpGet]
        public IActionResult CreateDocumentType()
        {
            return View(new DocumentType { IsActive = true, SortOrder = 0, IsMandatory = false });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDocumentType(DocumentType model)
        {
            if (!ModelState.IsValid) return View(model);

            var exists = await _ctx.DocumentTypes
                .AnyAsync(d => d.Name.ToLower() == model.Name.Trim().ToLower());
            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), "Document type already exists.");
                return View(model);
            }

            model.Name = model.Name.Trim();
            model.CreatedAt = DateTime.UtcNow;
            _ctx.DocumentTypes.Add(model);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Document type created.";
            return RedirectToAction(nameof(DocumentTypes));
        }

        [HttpGet]
        public async Task<IActionResult> EditDocumentType(int id)
        {
            var item = await _ctx.DocumentTypes.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDocumentType(DocumentType model)
        {
            if (!ModelState.IsValid) return View(model);

            var dup = await _ctx.DocumentTypes.AnyAsync(d =>
                d.Id != model.Id &&
                d.Name.ToLower() == model.Name.Trim().ToLower());
            if (dup)
            {
                ModelState.AddModelError(nameof(model.Name), "Another document type with same name exists.");
                return View(model);
            }

            var item = await _ctx.DocumentTypes.FindAsync(model.Id);
            if (item == null) return NotFound();

            item.Name = model.Name.Trim();
            item.IsActive = model.IsActive;
            item.IsMandatory = model.IsMandatory;
            item.SortOrder = model.SortOrder;

            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Document type updated.";
            return RedirectToAction(nameof(DocumentTypes));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocumentType(int id)
        {
            var count = await _ctx.ClientDocumentItems.CountAsync(x => x.DocumentTypeId == id);
            if (count > 0)
            {
                TempData["Error"] = "Cannot delete. This document type is used by clients.";
                return RedirectToAction(nameof(DocumentTypes));
            }

            var item = await _ctx.DocumentTypes.FindAsync(id);
            if (item == null)
            {
                TempData["Error"] = "Not found.";
                return RedirectToAction(nameof(DocumentTypes));
            }

            _ctx.DocumentTypes.Remove(item);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Document type deleted.";
            return RedirectToAction(nameof(DocumentTypes));
        }

        // JSON for dropdowns
        [HttpGet]
        public async Task<IActionResult> DocumentTypeList()
        {
            var rows = await _ctx.DocumentTypes
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new { x.Id, x.Name })
                .ToListAsync();
            return Json(rows);
        }

        // ---------- Skills (Master) ----------
        [HttpGet]
        public async Task<IActionResult> Skills()
        {
            var items = await _ctx.Skills
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Name)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public IActionResult CreateSkill()
        {
            return View(new Skill
            {
                IsActive = true,
                SortOrder = 0
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSkill(Skill model)
        {
            if (!ModelState.IsValid) return View(model);

            var name = (model.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(nameof(model.Name), "Name is required.");
                return View(model);
            }

            // duplicate guard (case-insensitive)
            var exists = await _ctx.Skills
                .AnyAsync(s => s.Name.ToLower() == name.ToLower());
            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), "Skill already exists.");
                return View(model);
            }

            model.Name = name;
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = null;

            _ctx.Skills.Add(model);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Skill created.";
            return RedirectToAction(nameof(Skills));
        }

        [HttpGet]
        public async Task<IActionResult> EditSkill(int id)
        {
            var item = await _ctx.Skills.FirstOrDefaultAsync(s => s.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSkill(int id, Skill model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var name = (model.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(nameof(model.Name), "Name is required.");
                return View(model);
            }

            // duplicate guard
            var dup = await _ctx.Skills.AnyAsync(s =>
                s.Id != id && s.Name.ToLower() == name.ToLower());
            if (dup)
            {
                ModelState.AddModelError(nameof(model.Name), "Another skill with same name exists.");
                return View(model);
            }

            var item = await _ctx.Skills.FirstOrDefaultAsync(s => s.Id == id);
            if (item == null) return NotFound();

            item.Name = name;
            item.IsActive = model.IsActive;
            item.SortOrder = model.SortOrder;
            item.UpdatedAt = DateTime.UtcNow;

            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Skill updated.";
            return RedirectToAction(nameof(Skills));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            var item = await _ctx.Skills.FirstOrDefaultAsync(s => s.Id == id);
            if (item == null)
            {
                TempData["Error"] = "Skill not found.";
                return RedirectToAction(nameof(Skills));
            }

            // Guard: if you later reference skills from RequirementSkills heavily,
            // you may prefer to only mark inactive instead of hard delete.
            _ctx.Skills.Remove(item);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Skill deleted.";
            return RedirectToAction(nameof(Skills));
        }

        // JSON endpoint for dropdowns / Tagify (if ever needed client-side)
        [HttpGet]
        public async Task<IActionResult> SkillNames()
        {
            var rows = await _ctx.Skills
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Name)
                .Select(s => s.Name)
                .ToListAsync();

            return Json(rows);
        }

        // ---------- Qualifications ----------
        [HttpGet]
        public async Task<IActionResult> Qualifications()
        {
            var items = await _ctx.Qualifications
                .OrderBy(q => q.SortOrder)
                .ThenBy(q => q.Name)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public IActionResult CreateQualification()
        {
            return View(new Qualification { IsActive = true, SortOrder = 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQualification(Qualification model)
        {
            if (!ModelState.IsValid) return View(model);

            var name = model.Name?.Trim() ?? "";
            var exists = await _ctx.Qualifications
                .AnyAsync(q => q.Name.ToLower() == name.ToLower());

            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), "Qualification already exists.");
                return View(model);
            }

            model.Name = name;
            model.CreatedAt = DateTime.UtcNow;

            _ctx.Qualifications.Add(model);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Qualification created.";
            return RedirectToAction(nameof(Qualifications));
        }

        [HttpGet]
        public async Task<IActionResult> EditQualification(int id)
        {
            var item = await _ctx.Qualifications.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQualification(int id, Qualification model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var item = await _ctx.Qualifications.FindAsync(id);
            if (item == null) return NotFound();

            var name = model.Name?.Trim() ?? "";
            var dup = await _ctx.Qualifications
                .AnyAsync(q => q.Id != id && q.Name.ToLower() == name.ToLower());

            if (dup)
            {
                ModelState.AddModelError(nameof(model.Name), "Another qualification with this name exists.");
                return View(model);
            }

            item.Name = name;
            item.IsActive = model.IsActive;
            item.SortOrder = model.SortOrder;
            item.UpdatedAt = DateTime.UtcNow;

            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Qualification updated.";
            return RedirectToAction(nameof(Qualifications));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQualification(int id)
        {
            var item = await _ctx.Qualifications.FindAsync(id);
            if (item == null) return NotFound();

            // Guard: prevent delete if in use by requirements
            var inUse = await _ctx.RequirementQualifications
                .AnyAsync(rq => rq.QualificationId == id);

            if (inUse)
            {
                TempData["Error"] = "Cannot delete: qualification is used by one or more requirements.";
                return RedirectToAction(nameof(Qualifications));
            }

            _ctx.Qualifications.Remove(item);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Qualification deleted.";
            return RedirectToAction(nameof(Qualifications));
        }

        // JSON for dropdowns / API usage
        [HttpGet]
        public async Task<IActionResult> QualificationList()
        {
            var rows = await _ctx.Qualifications
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new { x.Id, x.Name })
                .ToListAsync();

            return Json(rows);
        }

        // ---------- Notice Period Options ----------
        [HttpGet]
        public async Task<IActionResult> NoticePeriods()
        {
            var items = await _ctx.NoticePeriodOptions
                .OrderBy(n => n.SortOrder)
                .ThenBy(n => n.Name)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public IActionResult CreateNoticePeriod()
        {
            return View(new NoticePeriodOption
            {
                IsActive = true,
                SortOrder = 0
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNoticePeriod(NoticePeriodOption model)
        {
            if (!ModelState.IsValid) return View(model);

            var name = (model.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(nameof(model.Name), "Name is required.");
                return View(model);
            }

            var exists = await _ctx.NoticePeriodOptions
                .AnyAsync(n => n.Name.ToLower() == name.ToLower());

            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), "Notice period option already exists.");
                return View(model);
            }

            model.Name = name;
            model.CreatedAt = DateTime.UtcNow;

            _ctx.NoticePeriodOptions.Add(model);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Notice period option created.";
            return RedirectToAction(nameof(NoticePeriods));
        }

        [HttpGet]
        public async Task<IActionResult> EditNoticePeriod(int id)
        {
            var item = await _ctx.NoticePeriodOptions.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNoticePeriod(int id, NoticePeriodOption model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var item = await _ctx.NoticePeriodOptions.FindAsync(id);
            if (item == null) return NotFound();

            var name = (model.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(nameof(model.Name), "Name is required.");
                return View(model);
            }

            var dup = await _ctx.NoticePeriodOptions
                .AnyAsync(n => n.Id != id && n.Name.ToLower() == name.ToLower());

            if (dup)
            {
                ModelState.AddModelError(nameof(model.Name), "Another notice period option with this name exists.");
                return View(model);
            }

            item.Name = name;
            item.IsActive = model.IsActive;
            item.SortOrder = model.SortOrder;
            item.UpdatedAt = DateTime.UtcNow;

            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Notice period option updated.";
            return RedirectToAction(nameof(NoticePeriods));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNoticePeriod(int id)
        {
            var item = await _ctx.NoticePeriodOptions.FindAsync(id);
            if (item == null) return NotFound();

            // optional guard: prevent delete if many requirements are using this text
            // Currently requirements store plain text, so this is safe to delete.
            _ctx.NoticePeriodOptions.Remove(item);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Notice period option deleted.";
            return RedirectToAction(nameof(NoticePeriods));
        }

        // JSON for dropdowns / filters if needed later
        [HttpGet]
        public async Task<IActionResult> NoticePeriodList()
        {
            var rows = await _ctx.NoticePeriodOptions
                .Where(n => n.IsActive)
                .OrderBy(n => n.SortOrder)
                .ThenBy(n => n.Name)
                .Select(n => new { n.Id, n.Name })
                .ToListAsync();

            return Json(rows);
        }



    }
}
