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
        /// Method <c>GetDBContext</c> gets the current MySQL database context.
        /// </summary>
        /// <returns>The database context.</returns>
        private DatabaseContext GetDBContext()
        {
            return HttpContext.RequestServices.GetService(typeof(DatabaseContext)) as DatabaseContext;
        }

        /// <summary>
        /// Method <c>GetUser</c> gets a UserModel representing the currently logged in user.
        /// </summary>
        /// <returns>The UserModel of the logged in user.</returns>
        [Authorize]
        private async Task<UserModel> GetUser()
        {
            // get user data from Auth0
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            string emailAddress = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            string userName = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

            // get corresponding avatar from database, if any
            DatabaseContext dbContext = GetDBContext();            
            string? avatar = await dbContext.GetAvatar(userId);
            avatar ??= User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            // return the model of the user
            return new UserModel()
            {
                UserId = userId,
                EmailAddress = emailAddress,
                UserName = userName,
                Avatar = avatar
            };
        }

        /// <summary>
        /// Method <c>Dashboard</c> gets the dashboard for the user.
        /// </summary>
        /// <returns>The user dashboard action result.</returns>
        [Authorize]
        public async Task<IActionResult> UserDashboard()
        {
            UserModel userModel = await GetUser();
            DatabaseContext dbContext = GetDBContext();
            List<ProjectModel> projectModels = await dbContext.GetProjects(userModel.UserId);
            var viewPair = new Tuple<UserModel, List<ProjectModel>>(userModel, projectModels);
            return View(viewPair);
        }

        /// <summary>
        /// Method <c>Dashboard</c> gets the project dashboard.
        /// </summary>
        /// <param name="projectId">The ID of the project to get the dashboard for.</param>
        /// <returns>The project dashboard action result.</returns>
        [Authorize]
        public async Task<IActionResult> ProjectDashboard(string projectId)
        {
            DatabaseContext dbContext = GetDBContext();
            ProjectModel project = await dbContext.GetProject(projectId);
            List<UserModel> developers = await dbContext.GetDevelopers(projectId);
            var viewPair = new Tuple<ProjectModel, List<UserModel>>(project, developers);
            return View(viewPair);
        }

        /// <summary>
        /// Method <c>Profile</c> gets the profile for the user.
        /// </summary>
        /// <returns>The user profile action result.</returns>
        [Authorize]
        public async Task<IActionResult> UserProfile()
        {
            return View(await GetUser());
        }

        /// <summary>
        /// Method <c>Tasks</c> gets the tasks page for the user.
        /// </summary>
        /// <returns>The user tasks action result.</returns>
        [Authorize]
        public async Task<IActionResult> UserTasks()
        {
            return View(GetUser());
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
        /// Method <c>CreateProject</c> handles project creation from user input.
        /// </summary>
        /// <returns>The updated user dashboard action result.</returns>
        [Authorize, HttpPost]
        public async Task<IActionResult> CreateProject()
        {
            string projectName = Request.Form["project-name"];
            var user = await GetUser();
            var dbContext = GetDBContext();
            await dbContext.AddProject(projectName, user.UserId);
            return RedirectToAction("UserDashboard", "Account");
        }

        /// <summary>
        /// Method <c>DeleteProject</c> deletes the user from a project (if user is the only developer, the project is deleted as well)
        /// </summary>
        /// <param name="projectId">The ID of the project to delete the user from.</param>
        /// <returns>The updated user dashboard action result.</returns>
        [Authorize, HttpPost]
        public async Task<IActionResult> DeleteProject(string projectId)
        {
            string userId = GetUser().Result.UserId;
            DatabaseContext dbContext = GetDBContext();
            await dbContext.DeleteProject(projectId, userId);
            return RedirectToAction("UserDashboard", "Account");
        }

        /// <summary>
        /// Method <c>getAccessToken</c> fetches the Auth0 access token.
        /// </summary>
        /// <returns>The Auth0 access token.</returns>
        private static string GetAccessToken()
        {
            // request token
            var client = new RestClient("https://dev-pa5n40m7s26hur07.us.auth0.com");
            var request = new RestRequest("/oauth/token", Method.Post);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", "{\"client_id\":\"KPnNd3t1fcZ7YStDTSLNI12jy3W4B1Ue\",\"client_secret\":\"LEdemPNNQanSER5vvzPszqGrBU6HA_KOaA957daICIl7Cz81pDGeIc6ITRPo_aMs\",\"audience\":\"https://dev-pa5n40m7s26hur07.us.auth0.com/api/v2/\",\"grant_type\":\"client_credentials\"}", ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

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
        /// Method <c>UpdateAvatar</c> updates the avatar of the user in the MySQL database with the uploaded image.
        /// </summary>
        /// <param name="fileInput">The uploaded image.</param>
        private async Task UpdateAvatar(IFormFile? fileInput)
        {
            long fileSize = fileInput.Length;
            double fileFizeMB = fileSize / (1024.0 * 1024.0);
            if (fileInput != null && fileSize > 0 && fileFizeMB < 16)
            {
                // convert input into byte array
                using var memoryStream = new MemoryStream();
                await fileInput.CopyToAsync(memoryStream);
                byte[] fileData = memoryStream.ToArray();
                // add data to database
                DatabaseContext dbContext = GetDBContext();
                dbContext.SetAvatar(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, fileData);
            }
        }

        /// <summary>
        /// Method <c>UpdateProfile</c> handles user profile updates.
        /// </summary>
        /// <returns>The updated user profile action result.</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateProfile()
        {
            // get input from form
            string username = Request.Form["username"];
            IFormFile? fileInput = HttpContext.Request.Form.Files["image-file"];

            // update fields in database
            await UpdateUsername(username);
            await UpdateAvatar(fileInput);

            // return to updated profile page
            await Logout();  // to refresh User.Claims data
            await Login();  // to refresh User.Claims data
            return RedirectToAction("UserProfile", "Account");
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
