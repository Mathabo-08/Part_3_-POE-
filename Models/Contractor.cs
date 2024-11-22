using System.ComponentModel.DataAnnotations;

namespace Claim_System.Models 
{
    public class Contractor
    {
        [Key]
        public string ContractorEmail { get; set; } // primary login identifier and primary key

        [Required]
        public string ContractorPassword { get; set; }
    }

}
