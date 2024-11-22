using Microsoft.AspNetCore.Mvc;

namespace Claim_System.Controllers
{
    public class UserTypeController : Controller
    {
        // Add this action method to return the userType.cshtml view
        public IActionResult UserType() => View("~/Views/AppViews/userType.cshtml");


    }
}