using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Auth0.AspNetCore.Authentication;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using RestSharp;
using Newtonsoft.Json;

namespace BugTracker.Controllers
{
    /// <summary>
    /// Class <c>AccountController</c> is a controller handling user-account related actions.
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Method <c>Login</c> handles the login of the user.
        /// </summary>
        /// <param name="returnUrl">The location to send the user after authentication.</param>
        public async Task Login(string returnUrl = "/")
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
        /// Method <c>Dashboard</c> gets the dashboard for the user.
        /// </summary>
        /// <returns>The user dashboard action result.</returns>
        [Authorize]
        public IActionResult Dashboard()
        {
            return View(new UserModel()
            {
                UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                EmailAddress = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                UserName = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value,
                Avatar = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            });
        }

        /// <summary>
        /// Method <c>Profile</c> gets the profile for the user.
        /// </summary>
        /// <returns>The user profile action result.</returns>
        [Authorize]
        public IActionResult Profile()
        {
            return View(new UserModel()
            {
                UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                EmailAddress = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                UserName = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value,
                Avatar = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            });
        }

        /// <summary>
        /// Method <c>Tasks</c> gets the tasks for the user.
        /// </summary>
        /// <returns>The user tasks action result.</returns>
        [Authorize]
        public IActionResult Tasks()
        {
            return View(new UserModel()
            {
                UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                EmailAddress = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                UserName = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value,
                Avatar = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            });
        }

        /// <summary>
        /// Method <c>Logout</c> handles the logout of the user.
        /// </summary>
        [Authorize]
        public async Task Logout()
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
              .WithRedirectUri(Url.Action("Logout", "Home"))
              .Build();

            // Logout from Auth0
            await HttpContext.SignOutAsync(
              Auth0Constants.AuthenticationScheme,
              authenticationProperties
            );
            // Logout from the application
            await HttpContext.SignOutAsync(
              CookieAuthenticationDefaults.AuthenticationScheme
            );
        }

        /// <summary>
        /// Method <c>getAccessToken</c> fetches the Auth0 access token.
        /// </summary>
        /// <returns>The Auth0 access token.</returns>
        private static string GetAccessToken()
        {
            // request token
            var client = new RestClient("https://dev-pa5n40m7s26hur07.us.auth0.com");
            var request = new RestRequest("/oauth/token", RestSharp.Method.Post);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", "{\"client_id\":\"KPnNd3t1fcZ7YStDTSLNI12jy3W4B1Ue\",\"client_secret\":\"LEdemPNNQanSER5vvzPszqGrBU6HA_KOaA957daICIl7Cz81pDGeIc6ITRPo_aMs\",\"audience\":\"https://dev-pa5n40m7s26hur07.us.auth0.com/api/v2/\",\"grant_type\":\"client_credentials\"}", ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            System.Diagnostics.Debug.WriteLine(response.Content);

            // get token from request
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content);
            string? token = json["access_token"];
            return token;
        }

        /// <summary>
        /// Method <c>UpdateUsername</c> updates the username of the user in the Auth0 database.
        /// </summary>
        /// <param name="newName">The new name to update the username to.</param>
        [Authorize]
        private async Task UpdateUsername(string newName)
        {
            // create API client
            string accessToken = GetAccessToken();
            var client = new ManagementApiClient(accessToken, "dev-pa5n40m7s26hur07.us.auth0.com");

            // request username update
            string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var request = new UserUpdateRequest
            {
                FullName = newName
            };
            await client.Users.UpdateAsync(userId, request);
        }

        /// <summary>
        /// Method <c>UpdateProfile</c> handles user profile updates.
        /// </summary>
        /// <returns>The updated profile action result.</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateProfile()
        {
            // get input from form
            string username = Request.Form["username"];

            // update fields
            await UpdateUsername(username);

            // return to updated profile page
            await Logout();  // to refresh User.Claims data
            await Login();  // to refresh User.Claims data
            return RedirectToAction("Profile", "Account");
        }

        /// <summary>
        /// Method <c>DeleteAccount</c> handles the deletion of the user account.
        /// </summary>
        [Authorize]
        public async Task DeleteAccount()
        {
            // create API client
            string accessToken = GetAccessToken();
            var client = new ManagementApiClient(accessToken, "dev-pa5n40m7s26hur07.us.auth0.com");

            // request account deletion
            string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            await client.Users.DeleteAsync(userId);

            // log the user out
            await Logout();
        }
    }
}
