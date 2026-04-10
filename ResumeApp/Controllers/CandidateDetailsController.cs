using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;
using ResumeApp.Models;

namespace ResumeApp.Controllers
{
    [Authorize]
    [Route("CandidateDetails")]
    public class CandidateDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CandidateDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /CandidateDetails/Edit/12/7
        [HttpGet("Edit/{resumeId:int}/{requirementId:int}")]
        public async Task<IActionResult> Edit(int resumeId, int requirementId)
        {
            if (resumeId <= 0 || requirementId <= 0)
                return BadRequest();

            var model = await _context.CandidateDetails
                .FirstOrDefaultAsync(x =>
                    x.ResumeId == resumeId &&
                    x.RequirementId == requirementId);

            if (model == null)
            {
                model = new CandidateDetail
                {
                    ResumeId = resumeId,
                    RequirementId = requirementId
                };
            }

            return View(model);
        }

        // POST: /CandidateDetails/Edit
        [HttpPost("Edit/{resumeId:int}/{requirementId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int resumeId,
    int requirementId,
    CandidateDetail model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = await _context.CandidateDetails
                .FirstOrDefaultAsync(x =>
                    x.ResumeId == resumeId &&
                    x.RequirementId == requirementId);

            if (existing == null)
            {
                model.ResumeId = resumeId;
                model.RequirementId = requirementId;
                model.LastUpdatedAt = DateTime.UtcNow;

                _context.CandidateDetails.Add(model);
            }
            else
            {
                existing.DateOfBirth = model.DateOfBirth;
                existing.Qualification = model.Qualification;

                existing.TotalExperience = model.TotalExperience;
                existing.RelevantExperience = model.RelevantExperience;

                existing.CurrentCTC = model.CurrentCTC;
                existing.ExpectedCTC = model.ExpectedCTC;
                existing.NoticePeriod = model.NoticePeriod;

                existing.CurrentCompany = model.CurrentCompany;
                existing.CurrentLocation = model.CurrentLocation;
                existing.PreferredLocation = model.PreferredLocation;

                existing.Remarks = model.Remarks;
                existing.LastUpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "ClientRequirements",
                new { id = requirementId }
            );
        }


    }
}
