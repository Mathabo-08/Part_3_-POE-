using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Claim_System.Models;
using Claim_System.Data;
using System.Linq;
using System;
using System.IO;

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
                    // Handle the file upload for supporting document
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

                    // Save claim data to the database
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

        // Action for fetching today's module reminders
        public IActionResult GetModuleReminder()
        {
            var lecturerEmail = HttpContext.Session.GetString("LecturerEmail");

            if (string.IsNullOrEmpty(lecturerEmail))
            {
                return RedirectToAction("login_Lecturer");
            }

            // Get today's day of the week
            var currentDay = DateTime.Now.DayOfWeek.ToString();

            // Fetch the schedule for the current day
            var reminders = _context.ModuleSchedules
                .Where(ms => ms.LecturerEmail == lecturerEmail && ms.ReminderDay == currentDay)
                .Select(ms => new { ms.ModuleCode, ms.ReminderDay })
                .ToList();

            return Json(reminders);
        }

        // POST: Submit the module schedule reminder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitModuleScheduleReminder(string moduleCode, string reminderDay)
        {
            var lecturerEmail = HttpContext.Session.GetString("LecturerEmail");

            if (string.IsNullOrEmpty(lecturerEmail))
            {
                return RedirectToAction("login_Lecturer");
            }

            if (string.IsNullOrEmpty(moduleCode) || string.IsNullOrEmpty(reminderDay))
            {
                ViewBag.ReminderErrorMessage = "Module Code and Reminder Day are required.";
                return View("~/Views/AppViews/submitClaim.cshtml");
            }

            try
            {
                // Check if a reminder already exists for the given lecturer, module, and reminder day
                var existingReminder = _context.ModuleSchedules
                    .FirstOrDefault(ms => ms.LecturerEmail == lecturerEmail &&
                                          ms.ModuleCode == moduleCode &&
                                          ms.ReminderDay == reminderDay);

                if (existingReminder != null)
                {
                    ViewBag.ReminderErrorMessage = "A reminder for this module and day already exists.";
                    return View("~/Views/AppViews/submitClaim.cshtml");
                }

                // Create and add a new reminder if none exists
                var moduleSchedule = new ModuleSchedule
                {
                    LecturerEmail = lecturerEmail,
                    ModuleCode = moduleCode,
                    ReminderDay = reminderDay
                };

                // Add the new reminder to the database
                _context.ModuleSchedules.Add(moduleSchedule);
                _context.SaveChanges();

                // Success message
                ViewBag.ReminderSuccessMessage = "Module reminder successfully added!";
            }
            catch (Exception ex)
            {
                ViewBag.ReminderErrorMessage = "An error occurred while adding the module reminder. Please try again.";
            }

            return View("~/Views/AppViews/submitClaim.cshtml");
        }

        // GET: Display today's module schedule reminders
        public IActionResult ModuleScheduleReminder()
        {
            var lecturerEmail = HttpContext.Session.GetString("LecturerEmail");

            if (string.IsNullOrEmpty(lecturerEmail))
            {
                return RedirectToAction("login_Lecturer");
            }

            // Get today's day of the week
            var currentDay = DateTime.Now.DayOfWeek.ToString();

            // Fetch module schedules for the current day
            var reminders = _context.ModuleSchedules
                .Where(ms => ms.LecturerEmail == lecturerEmail && ms.ReminderDay == currentDay)
                .Select(ms => new ModuleScheduleReminderViewModel
                {
                    ModuleCode = ms.ModuleCode,
                    ReminderDay = ms.ReminderDay
                })
                .ToList();

            if (!reminders.Any())
            {
                ViewBag.ErrorMessage = "No reminders found for today.";
            }

            return View("~/Views/AppViews/ModuleScheduleReminder.cshtml", reminders);
        }
    }
}
