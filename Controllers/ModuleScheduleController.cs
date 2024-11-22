using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Claim_System.Data;
using Claim_System.Models;

public class ModuleScheduleController : Controller
{
    private readonly ApplicationDbContext _context;

    public ModuleScheduleController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Index action to display schedules
    public IActionResult Index()
    {
        // Get the logged-in lecturer's email if available
        string lecturerEmail = HttpContext.Session.GetString("LecturerEmail");

        // Get all schedules for the lecturer (if logged in), otherwise all schedules
        var schedules = _context.ModuleSchedules
            .Where(m => string.IsNullOrEmpty(lecturerEmail) || m.LecturerEmail == lecturerEmail)
            .ToList();

        // Create a new ModuleSchedule object for adding or updating a reminder
        var model = new ModuleSchedule(); // Empty object for form input

        // Return the view with both the schedule data and the new model
        return View("~/Views/ModuleSchedule/Index.cshtml", new ModuleScheduleViewModel
        {
            Schedules = schedules,
            ScheduleForm = model
        });
    }

    // Add or update a reminder
    [HttpPost]
    public IActionResult AddOrUpdateReminder(ModuleSchedule model)
    {
        // Get the logged-in lecturer's email
        string lecturerEmail = HttpContext.Session.GetString("LecturerEmail");

        if (string.IsNullOrEmpty(lecturerEmail))
        {
            ModelState.AddModelError("LecturerEmail", "Lecturer is not logged in.");
            return View("~/Views/ModuleSchedule/Index.cshtml", new ModuleScheduleViewModel
            {
                Schedules = _context.ModuleSchedules.ToList(),
                ScheduleForm = model
            });
        }

        // Set LecturerEmail if it's not already set
        model.LecturerEmail = lecturerEmail;

        // Ensure the LecturerEmail exists in the Lecturers table
        var lecturerExists = _context.Lecturers.Any(l => l.LecturerEmail == model.LecturerEmail);
        if (!lecturerExists)
        {
            ModelState.AddModelError("LecturerEmail", "The lecturer email does not exist.");
            return View("~/Views/ModuleSchedule/Index.cshtml", new ModuleScheduleViewModel
            {
                Schedules = _context.ModuleSchedules.ToList(),
                ScheduleForm = model
            });
        }

        // Ensure ModuleCode is not null or empty
        if (string.IsNullOrEmpty(model.ModuleCode))
        {
            ModelState.AddModelError("ModuleCode", "Module code is required.");
        }

        // Ensure ReminderDay is not null or empty
        if (string.IsNullOrEmpty(model.ReminderDay))
        {
            ModelState.AddModelError("ReminderDay", "Reminder day is required.");
        }

        if (ModelState.IsValid)
        {
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

            // Set TempData message to show on the next request
            TempData["SuccessMessage"] = "Reminder successfully set!";

            // Redirect to the Index action to reload the page and show the message
            return RedirectToAction("Index");
        }
        else
        {
            // If the model state is invalid, re-display the view with validation messages
            return View("~/Views/ModuleSchedule/Index.cshtml", new ModuleScheduleViewModel
            {
                Schedules = _context.ModuleSchedules.ToList(),
                ScheduleForm = model
            });
        }
    }

    // Delete a reminder
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
