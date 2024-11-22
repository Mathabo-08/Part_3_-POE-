using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Claim_System.Models
{
    public class Claim
    {
        public int Id { get; set; }

        // Initialize the collection to prevent null reference issues
        public virtual ICollection<ClaimStatus> ClaimStatuses { get; set; } = new List<ClaimStatus>();

        [Required(ErrorMessage = "Claim date is required.")]
        [DataType(DataType.Date)]
        public DateTime ClaimDate { get; set; }

        [Required(ErrorMessage = "Employee number is required.")]
        [MaxLength(50, ErrorMessage = "Employee number cannot exceed 50 characters.")]
        public string EmployeeNumber { get; set; }

        [Required(ErrorMessage = "Lecturer email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string LecturerEmail { get; set; }

        [MaxLength(100, ErrorMessage = "Course name cannot exceed 100 characters.")]
        public string Course { get; set; }

        [MaxLength(50, ErrorMessage = "Module code cannot exceed 50 characters.")]
        public string ModuleCode { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 1000000.00, ErrorMessage = "Amount must be between 0.01 and 1,000,000.00.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Monthly hours worked are required.")]
        [Range(1, 744, ErrorMessage = "Monthly hours worked must be between 1 and 744.")]
        public int MonthlyHoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required.")]
        [Range(0.01, 10000.00, ErrorMessage = "Hourly rate must be between 0.01 and 10,000.00.")]
        public decimal HourlyRate { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public string Status { get; set; } = "Pending"; // Default value

        // Explicitly mark SupportDocument as optional and remove [Required] attribute
        [MaxLength(255, ErrorMessage = "File path cannot exceed 255 characters.")]
        public string SupportDocument { get; set; } // No [Required] attribute, making it optional
    }
}
