using Inventory.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryWeb.Areas.Identity.Pages.Account
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class SessionConflictModel : PageModel
    {


        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public SessionConflictModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }




        //public void OnGet()
        //{
        //}
        public IActionResult OnGet()
        {
            // 2. If the user hits "Back" but the data is already gone, kick them to login
            // We use Peek() to check without consuming the data
            if (TempData.Peek("UserId") == null)
            {
                return RedirectToPage("./Login");
            }
            return Page();
        }




        public async Task<IActionResult> OnPostAsync(string choice)
        {
            



            if (choice != "ok")
            {
                TempData.Clear(); // 🔥 IMPORTANT
                return RedirectToPage("./Login");
            }



            string userId = TempData["UserId"]?.ToString();
            string newSessionId = TempData["NewSessionId"]?.ToString();
            string returnUrl = TempData["ReturnUrl"]?.ToString() ?? "~/";

            // TempData must be valid exactly once
            if (userId == null || newSessionId == null)
                return RedirectToPage("./Login");

            TempData.Clear(); // 🔥 Consume immediately

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToPage("./Login");

            // Replace the session
            user.CurrentSessionId = newSessionId;
            user.SessionExpiresAt = DateTime.UtcNow.AddMinutes(30);

            // LOG OUT PREVIOUS SESSIONS
            await _userManager.UpdateSecurityStampAsync(user);
            await _userManager.UpdateAsync(user);

            // Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);
            HttpContext.Session.SetString("SessionId", newSessionId);

            return LocalRedirect(returnUrl ?? "~/");
        }
    }
}
