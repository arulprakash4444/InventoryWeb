using Inventory.Models;
using Microsoft.AspNetCore.Identity;

namespace InventoryWeb.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            // Only validate for authenticated users
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);

                if (user == null)
                {
                    await signInManager.SignOutAsync();
                    return;
                }

                // Read sessionId from cookie/session
                var sessionIdFromClient =
                    context.Session.GetString("SessionId");
                // or Request.Cookies["SessionId"]

                // If no sessionId on client → invalid
                if (string.IsNullOrEmpty(sessionIdFromClient))
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Identity/Account/Login");
                    return;
                }

                // If DB session expired → invalidate
                if (!user.SessionExpiresAt.HasValue ||
                    user.SessionExpiresAt <= DateTime.UtcNow)
                {
                    user.CurrentSessionId = null;
                    user.SessionExpiresAt = null;
                    await userManager.UpdateAsync(user);

                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Identity/Account/SessionExpired");
                    return;
                }

                // If sessionId mismatch → logged in elsewhere
                if (user.CurrentSessionId != sessionIdFromClient)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Identity/Account/SessionExpired");
                    return;
                }

                // Sliding expiration (refresh)
                user.SessionExpiresAt = DateTime.UtcNow.AddMinutes(30);
                await userManager.UpdateAsync(user);
            }

            await _next(context);
        }
    }
}
