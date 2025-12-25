using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;

namespace ResumeApp.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        public ClientsController(ApplicationDbContext ctx) => _ctx = ctx;

        public async Task<IActionResult> Index()
        {
            var clients = await _ctx.Clients
                .Include(c => c.Spokespersons)
                .Include(c => c.Documents)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(clients);
        }

        // Details
        public async Task<IActionResult> Details(int id)
        {
            var client = await _ctx.Clients
                .Include(c => c.HeadquarterCountry)
                .Include(c => c.HeadquarterLocation)
                .Include(c => c.OtherLocationsMap).ThenInclude(o => o.Location)
                .Include(c => c.Documents) 
                .Include(c => c.Spokespersons)
                .Include(c => c.ClientDocumentItems)         
                 .ThenInclude(i => i.DocumentType)           
                 .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null) return NotFound();

            var recentJds = await _ctx.ClientRequirements
                .Where(r => r.ClientId == id)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentJds = recentJds;
            return View(client);
        }



        // GET: Clients/Edit/
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _ctx.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: Clients/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,CompanyName,WebsiteUrl,CompanyType,CompanySize,HeadquarterLocation,OtherOfficeLocations,ContactName,Designation,Phone,Email,PreferredCommunication,EngagementTypes,IsActive")] Client updated)
        {
            if (!ModelState.IsValid)
                return View(updated);

            var client = await _ctx.Clients.FirstOrDefaultAsync(c => c.Id == updated.Id);
            if (client == null) return NotFound();

            // Map safe fields
            client.CompanyName = updated.CompanyName?.Trim();
            client.WebsiteUrl = updated.WebsiteUrl?.Trim();
            client.CompanyType = updated.CompanyType?.Trim();
            client.CompanySize = updated.CompanySize?.Trim();

            client.ContactName = updated.ContactName?.Trim();
            client.Designation = updated.Designation?.Trim();
            client.Phone = updated.Phone?.Trim();
            client.Email = updated.Email?.Trim();
            client.PreferredCommunication = updated.PreferredCommunication?.Trim();
            client.EngagementTypes = updated.EngagementTypes?.Trim();
            client.IsActive = updated.IsActive;

            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Client updated successfully.";
            return RedirectToAction(nameof(Index)); // ← go back to Clients list
        }

        // POST Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _ctx.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();

            // block if the client has any active JDs
            // If you don't have a status column yet, just check any JD exists:
            var hasAnyRequirement = await _ctx.ClientRequirements.AnyAsync(r => r.ClientId == id);

            // If you DO have a status, use:
            // var hasActiveRequirement = await _ctx.ClientRequirements.AnyAsync(r => r.ClientId == id && r.Status == "Active");

            if (hasAnyRequirement)
            {
                TempData["Error"] = "Cannot delete: client has active job requirements. Close or reassign them first.";
                return RedirectToAction(nameof(Details), new { id });
            }

            client.IsDeleted = true;
            client.IsActive = false;
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Client deleted.";
            return RedirectToAction("ClientList", "UserManagement");
        }
    }
}
