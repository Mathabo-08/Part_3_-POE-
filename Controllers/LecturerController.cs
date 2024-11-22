using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Claim_System.Models;
using Claim_System.Data;
using System.Linq;
using System;

namespace Claim_System.Controllers
{
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LecturerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Lecturer Login Page
        public IActionResult login_Lecturer()
        {
            return View("~/Views/AppViews/login_Lecturer.cshtml");
        }

        // POST: Lecturer Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult login_Lecturer(string lecturerEmail, string lecturerPassword)
        {
            if (string.IsNullOrEmpty(lecturerEmail))
            {
                ViewBag.ErrorMessage = "Email is required.";
                return View("~/Views/AppViews/login_Lecturer.cshtml");
            }

            var lecturer = _context.Lecturers
                .FirstOrDefault(l => l.LecturerEmail == lecturerEmail);

            if (lecturer == null)
            {
                lecturer = new Lecturer
                {
                    LecturerEmail = lecturerEmail,
                    LecturerPassword = lecturerPassword
                };

                _context.Lecturers.Add(lecturer);
                _context.SaveChanges();
            }

            HttpContext.Session.SetString("LecturerEmail", lecturerEmail);
            return RedirectToAction("submitClaim", "Lecturer");
        }

        // GET: Submit Claim
        public IActionResult submitClaim()
        {
            var lecturerEmail = HttpContext.Session.GetString("LecturerEmail");

            if (string.IsNullOrEmpty(lecturerEmail))
            {
                ViewBag.ErrorMessage = "Session expired or not logged in. Please log in again.";
                return RedirectToAction("login_Lecturer");
            }

            return View("~/Views/AppViews/submitClaim.cshtml", new Claim
            {
                LecturerEmail = lecturerEmail
            });
        }

        // POST: Submit Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult submitClaim(Claim claim, IFormFile supportDocument)
        {
            var lecturerEmail = HttpContext.Session.GetString("LecturerEmail");

            if (string.IsNullOrEmpty(lecturerEmail))
            {
                return RedirectToAction("login_Lecturer");
            }

            claim.LecturerEmail = lecturerEmail;
            ModelState.Remove("SupportDocument");

            if (ModelState.IsValid)
            {
                try
                {
                    if (supportDocument != null && supportDocument.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(supportDocument.FileName).ToLower();
                        if (fileExtension != ".pdf" && fileExtension != ".docx")
                        {
                            ModelState.AddModelError("SupportDocument", "Only PDF or DOCX files are allowed.");
                            return View("~/Views/AppViews/submitClaim.cshtml", claim);
                        }

                        if (supportDocument.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("SupportDocument", "File size must be less than 5MB.");
                            return View("~/Views/AppViews/submitClaim.cshtml", claim);
                        }

                        string claimsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/claims_documents");
                        Directory.CreateDirectory(claimsFolder);
                        string filePath = Path.Combine(claimsFolder, supportDocument.FileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            supportDocument.CopyTo(stream);
                        }

                        claim.SupportDocument = $"/claims_documents/{supportDocument.FileName}";
                    }

                    _context.Claims.Add(claim);
                    _context.SaveChanges();

                    ViewBag.SuccessMessage = "Claim successfully submitted!";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while submitting claim: {ex.Message}");
                    ViewBag.ErrorMessage = "An error occurred while submitting your claim. Please try again.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Please ensure all fields are correctly filled out.";
            }

            return View("~/Views/AppViews/submitClaim.cshtml", claim);
        }

        // GET: View Submitted Claims
        public IActionResult viewSubmittedClaims()
        {
            var lecturerEmail = HttpContext.Session.GetString("LecturerEmail");

            if (string.IsNullOrEmpty(lecturerEmail))
            {
                return RedirectToAction("login_Lecturer");
            }

            // Fetch the claims and related claim statuses
            var respondedClaims = _context.Claims
                .Include(c => c.ClaimStatuses)
                .Where(c => c.LecturerEmail == lecturerEmail && c.ClaimStatuses.Any(cs => !string.IsNullOrEmpty(cs.Status)))
                .ToList();

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
