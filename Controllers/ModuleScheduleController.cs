using Claim_System.Data;
using Claim_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Claim_System.Controllers
{
    public class ModuleScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModuleScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display the schedule page
        public IActionResult Index()
        {
            string lecturerEmail = User.Identity.Name; // Assuming lecturer is logged in

            // Check if the lecturer's email is valid
            if (string.IsNullOrEmpty(lecturerEmail))
            {
                // Redirect to the 'login_Lecturer' page in the 'Lecturer' controller
                return RedirectToAction("login_Lecturer", "Lecturer");
            }

            var schedules = _context.ModuleSchedules
                .Where(m => m.LecturerEmail == lecturerEmail)
                .ToList();

            return View("~/Views/AppViews/Index.cshtml", schedules);
        }

        // Add or Edit Reminder
        [HttpPost]
        public IActionResult AddOrUpdateReminder(ModuleSchedule model)
        {
            // Get the logged-in lecturer email
            string lecturerEmail = User.Identity.Name;

            // Check if the lecturer's email is valid
            if (string.IsNullOrEmpty(lecturerEmail))
            {
                // Redirect to the 'login_Lecturer' page in the 'Lecturer' controller
                return RedirectToAction("login_Lecturer", "Lecturer");
            }

            // Ensure the LecturerEmail is set
            model.LecturerEmail = lecturerEmail;

            // Check if the schedule for this lecturer and module already exists
            var existingSchedule = _context.ModuleSchedules
                .FirstOrDefault(m => m.LecturerEmail == model.LecturerEmail && m.ModuleCode == model.ModuleCode);

            if (existingSchedule != null)
            {
                existingSchedule.ReminderDay = model.ReminderDay;
                _context.ModuleSchedules.Update(existingSchedule);
            }
            else
            {
                _context.ModuleSchedules.Add(model);
            }

            // Save the changes to the database
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // Delete Reminder
        public IActionResult DeleteReminder(int id)
        {
            var schedule = _context.ModuleSchedules.Find(id);
            if (schedule != null)
            {
                _context.ModuleSchedules.Remove(schedule);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
