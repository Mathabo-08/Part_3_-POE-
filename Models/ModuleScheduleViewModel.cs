namespace Claim_System.Models
{
    using System.Collections.Generic;

    public class ModuleScheduleViewModel
    {
        public List<ModuleSchedule> Schedules { get; set; }
        public ModuleSchedule ScheduleForm { get; set; } // For adding/updating reminders

        public string ReminderDay { get; set; } // Ensure this property exists

        public string ModuleCode { get; set; }
    }


}