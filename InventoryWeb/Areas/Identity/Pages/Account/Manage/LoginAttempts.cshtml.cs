using Inventory.DataAccess.Repository.IRepository;
using Inventory.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryWeb.Areas.Identity.Pages.Account.Manage
{
    public class LoginAttemptsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILoginAttemptRepository _loginAttemptsRepo;

        public LoginAttemptsModel(
            UserManager<ApplicationUser> userManager,
            ILoginAttemptRepository loginAttemptsRepo)
        {
            _userManager = userManager;
            _loginAttemptsRepo = loginAttemptsRepo;
        }

        public IEnumerable<LoginAttempt> Attempts { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound("User not found.");

            Attempts = await _loginAttemptsRepo.GetLast10AttemptsAsync(user.UserName);

            return Page();
        }
    }
}
