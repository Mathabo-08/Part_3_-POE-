using System;
using System.Collections.Generic;

namespace Claim_System.Models
{
    public class ClaimStatusViewModel
    {
        public string LecturerEmail { get; set; }
        public List<Claim> Claims { get; set; }
        public List<ClaimStatus> ClaimStatuses { get; set; }
        public ClaimStatus LatestStatus { get; set; }
    }
}
