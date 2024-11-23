using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Claim_System.Models
{
    public class ClaimStatus
    {
        [Key] // Marks this property as the primary key
        public int Id { get; set; } // Primary key, corresponds to the "Id" column in the table

        [ForeignKey("Claim")]
        public int ClaimId { get; set; } // Foreign key to the Claims table

        public string Status { get; set; } // Status of the claim
        public string ContractorFeedback { get; set; } // Feedback from the contractor
        public string ContractorType { get; set; } // Type of contractor
        public string ContractorWorkCampus { get; set; } // Work campus of the contractor

        public DateTime DateUpdated { get; set; } = DateTime.Now; // Defaults to current date

        // Navigation property
        public virtual Claim Claim { get; set; }
    }
}
