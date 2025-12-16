using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class SessionExpiredModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
