namespace Claim_System.Models
{
    public class ModuleSchedule
    {
        public int Id { get; set; }
        public string LecturerEmail { get; set; }  // Ensure this property is named 'LecturerEmail'
        public string ModuleCode { get; set; }
        public string ReminderDay { get; set; }
    }



}
