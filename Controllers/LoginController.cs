using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BugTracker.Models.DatabaseContexts;

namespace BugTracker.Controllers
{
    /// <summary>
    /// Class <c>LoginController</c> is a controller handling login and logout.
    /// </summary>
    public class LoginController : DatabaseAccessingController
    {
        /// <summary>
        /// Method <c>Login</c> handles the login of the user.
        /// </summary>
        /// <param name="returnUrl">The location to send the user after authentication.</param>
        public async Task Login(string returnUrl = "/Login/LoginCallback")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
              .WithRedirectUri(returnUrl)
              .Build();

            await HttpContext.ChallengeAsync(
              Auth0Constants.AuthenticationScheme,
              authenticationProperties
            );
        }

        /// <summary>
        /// Method <c>LoginCallback</c> handles pipeline actions after user login.
        /// </summary>
        /// <returns>The action result of homepage the user sees after login.</returns>
        [Authorize]
        public async Task<IActionResult> LoginCallback()
        {
            // make sure the user is registered in the MySQL database
            string? userId = GetUserId();
            DatabaseContext? dbCx = GetDbCx();
            if (userId != null && dbCx != null)
            {
                await dbCx.AddUserIfNone(userId);
            }

            // go to user dashboard page
            return RedirectToAction("Dashboard", "Account");
        }

        /// <summary>
        /// Method <c>Logout</c> handles the logout of the user.
        /// </summary>
        [Authorize]
        public async Task Logout()
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
              .WithRedirectUri(Url.Action("LoggedOut", "Account"))
              .Build();

            await HttpContext.SignOutAsync(
              Auth0Constants.AuthenticationScheme,
              authenticationProperties
            );

            await HttpContext.SignOutAsync(
              CookieAuthenticationDefaults.AuthenticationScheme
            );
        }
    }
}
