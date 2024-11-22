using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Claim_System.Models;
using Claim_System.Data;
using System;

namespace Claim_System.Controllers
{
    public class ContractorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContractorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Contractor/Login
        public IActionResult login_Contractor()
        {
            return View("~/Views/AppViews/login_Contractor.cshtml");
        }

        // POST: Contractor/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> login_Contractor(string contractorEmail, string contractorPassword)
        {
            if (string.IsNullOrEmpty(contractorEmail) || string.IsNullOrEmpty(contractorPassword))
            {
                ViewBag.ErrorMessage = "Email and password are required.";
                return View("~/Views/AppViews/login_Contractor.cshtml");
            }

            var contractor = await _context.Contractors.FirstOrDefaultAsync(c => c.ContractorEmail == contractorEmail);

            if (contractor == null)
            {
                contractor = new Contractor
                {
                    ContractorEmail = contractorEmail,
                    ContractorPassword = contractorPassword
                };

                _context.Contractors.Add(contractor);
                await _context.SaveChangesAsync();
            }

            if (contractor.ContractorPassword == contractorPassword)
            {
                HttpContext.Session.SetString("ContractorEmail", contractorEmail);
                return RedirectToAction("pendingClaims");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid login credentials.";
                return View("~/Views/AppViews/login_Contractor.cshtml");
            }
        }

        // GET: Pending Claims
        public IActionResult pendingClaims()
        {
            var contractorEmail = HttpContext.Session.GetString("ContractorEmail");
            if (string.IsNullOrEmpty(contractorEmail))
            {
                return RedirectToAction("login_Contractor");
            }

            var pendingClaims = _context.Claims
                                        .Where(c => c.Status == "Pending")
                                        .ToList();

            return View("~/Views/AppViews/pendingClaims.cshtml", pendingClaims);
        }

        // POST: Approve or Reject Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRejectClaim(int claimId, string status, string contractorFeedback, string contractorType, string contractorWorkCampus)
        {
            var contractorEmail = HttpContext.Session.GetString("ContractorEmail");
            if (string.IsNullOrEmpty(contractorEmail))
            {
                return Json(new { success = false, error = "You are not logged in." });
            }

            if (string.IsNullOrEmpty(status) || string.IsNullOrEmpty(contractorFeedback) ||
                string.IsNullOrEmpty(contractorType) || string.IsNullOrEmpty(contractorWorkCampus))
            {
                return Json(new { success = false, error = "All fields are required." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var claim = await _context.Claims.FirstOrDefaultAsync(c => c.Id == claimId);

                if (claim == null)
                {
                    return Json(new { success = false, error = "Claim not found." });
                }

                // Update claim status
                claim.Status = status;
                _context.Claims.Update(claim);

                var claimStatus = new ClaimStatus
                {
                    ClaimId = claimId,
                    Status = status,
                    ContractorFeedback = contractorFeedback,
                    ContractorType = contractorType,
                    ContractorWorkCampus = contractorWorkCampus,
                    DateUpdated = DateTime.Now
                };

                _context.ClaimStatuses.Add(claimStatus);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
