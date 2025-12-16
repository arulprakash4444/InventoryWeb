using Inventory.Utility;
//using InventoryWeb.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryWeb.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class UserNotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
