using System.ComponentModel.DataAnnotations;

namespace Claim_System.Models
{
    public class Lecturer
    {
        [Key] // Marks LecturerEmail as the primary key
        public string LecturerEmail { get; set; } // Primary key column

        [Required] // Ensures this field is not nullable
        public string LecturerPassword { get; set; } // Password column

        // Navigation property to access associated module schedules
        public ICollection<ModuleSchedule> ModuleSchedules { get; set; }
    }
}
