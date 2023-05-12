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
using System.Net.Mail;

namespace BugTracker.Controllers
{
    public class AccountController : Controller
    {
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

        [Authorize]
        public IActionResult Dashboard()
        {
            return View(new UserModel()
            {
                UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                EmailAddress = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                UserName = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value,
                ProfileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            });
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View(new UserModel()
            {
                UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                EmailAddress = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                UserName = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value,
                ProfileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            });
        }

        [Authorize]
        public IActionResult Tasks()
        {
            return View(new UserModel()
            {
                UserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                EmailAddress = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                UserName = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value,
                ProfileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            });
        }

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

        private string getAccessToken()
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
            var token = json["access_token"];
            return token;
        }

        [Authorize]
        private async Task UpdateUsername(string newName)
        {
            // create API client
            var accessToken = getAccessToken();
            var client = new ManagementApiClient(accessToken, "dev-pa5n40m7s26hur07.us.auth0.com");

            /* check if the new name is the same as the old one
            var oldName = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            if (oldName == name) return; */

            // request username update
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var request = new UserUpdateRequest
            {
                FullName = newName
            };
            await client.Users.UpdateAsync(userId, request);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile()
        {
            // get input from form
            string username = Request.Form["username"];

            // update fields
            await UpdateUsername(username);

            // return to updated profile page
            await Logout();
            await Login();
            return RedirectToAction("Profile", "Account");
        }

        [Authorize]
        public async Task DeleteAccount()
        {
            // create API client
            var accessToken = getAccessToken();
            var client = new ManagementApiClient(accessToken, "dev-pa5n40m7s26hur07.us.auth0.com");

            // request account deletion
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            await client.Users.DeleteAsync(userId);

            // log the user out
            await Logout();
        }
    }
}
