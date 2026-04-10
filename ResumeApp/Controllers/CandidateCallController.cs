using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResumeApp.Data;
using ResumeApp.Models;

namespace ResumeApp.Controllers
{
    public class CandidateCallController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;

        public CandidateCallController(
            ApplicationDbContext context,
            UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> StartCall(
            int resumeId,
            int requirementId,
            string phone)
        {
            var call = new CandidateCallLog
            {
                ResumeId = resumeId,
                RequirementId = requirementId,
                PhoneNumber = phone,
                CallStartTime = DateTime.UtcNow,
                CalledByUserId = _userManager.GetUserId(User)
            };

            _context.CandidateCallLogs.Add(call);
            await _context.SaveChangesAsync();

            return Json(new { callId = call.Id });
        }

        [HttpPost]
        public async Task<IActionResult> EndCall(
            int callId,
            string outcome,
            string notes)
        {
            var call = await _context.CandidateCallLogs.FindAsync(callId);
            if (call == null)
                return NotFound();

            call.CallEndTime = DateTime.UtcNow;
            call.Outcome = outcome;
            call.Notes = notes;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
