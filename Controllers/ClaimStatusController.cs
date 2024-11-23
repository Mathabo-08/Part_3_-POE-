using Claim_System.Data;
using Claim_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Claim_System.Controllers
{
    public class ClaimStatusController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClaimStatusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ClaimStatus
        public async Task<IActionResult> ClaimStatus()
        {
            var lecturerEmail = HttpContext.Session.GetString("LecturerEmail");

            if (string.IsNullOrEmpty(lecturerEmail))
            {
                return RedirectToAction("login_Lecturer", "Lecturer");
            }

            // Fetch claims responded to by contractors for the logged-in lecturer
            var respondedClaims = await _context.Claims
                .Where(c => c.LecturerEmail == lecturerEmail && c.Status != "Pending")
                .Include(c => c.ClaimStatuses)  // Include ClaimStatuses navigation property
                .ToListAsync();

            // Map claims to ClaimStatusViewModel
            var viewModel = respondedClaims.Select(c => new ClaimStatusViewModel
            {
                LecturerEmail = lecturerEmail,
                Claims = new List<Claim> { c },  // Wrap each claim in a list
                ClaimStatuses = c.ClaimStatuses.ToList(),
                LatestStatus = c.ClaimStatuses
                    .OrderByDescending(cs => cs.DateUpdated)
                    .FirstOrDefault()  // Get the most recent status
            }).ToList();

            return View("~/Views/AppViews/claimStatus.cshtml", viewModel);
        }

    }
}
