using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BugTracker.Models.DatabaseContexts;
using BugTracker.Models.EntityModels;
using BugTracker.Models.ViewDataModels;

namespace BugTracker.Controllers
{
    /// <summary>
    /// Class <c>AccountController</c> is a controller handling user-account related actions.
    /// </summary>
    public class AccountController : Controller
    {
        /// <value>
        /// Property <c>_configuration</c> is the appsettings configuration.
        /// </value>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Method <c>AccountController</c> initializes class with 
        /// </summary>
        /// <param name="configuration"></param>
        public AccountController(IConfiguration configuration) {
            _configuration = configuration;
        }

        /// <summary>
        /// Method <c>Login</c> handles the login of the user.
        /// </summary>
        /// <param name="returnUrl">The location to send the user after authentication.</param>
        public async Task Login(string returnUrl = "/Account/LoginCallback")
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
            // make sure the user is registered in the MySQL database as well
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var dbContext = GetDBContext();
            await dbContext.AddUserIfNone(userId);

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Method <c>GetDBContext</c> gets the current MySQL database context.
        /// </summary>
        /// <returns>The database context.</returns>
        private MySQLDatabaseContext GetDBContext()
        {
            return HttpContext.RequestServices.GetService(typeof(MySQLDatabaseContext)) as MySQLDatabaseContext;
        }

        /// <summary>
        /// Method <c>GetAuthContext</c> gets the current Auth0 database context.
        /// </summary>
        /// <returns>The database context.</returns>
        private Auth0ManagementContext GetAuthContext()
        {
            return new Auth0ManagementContext(_configuration);
        }

        /// <summary>
        /// Method <c>GetUserID</c> gets the ID of the currently logged in user.
        /// </summary>
        /// <returns>The logged in user's ID.</returns>
        private string GetUserID()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Method <c>GetUser</c> gets a UserModel representing the currently logged in user.
        /// </summary>
        /// <returns>The UserModel of the logged in user.</returns>
        [Authorize]
        private async Task<UserModel> GetUser()
        {
            // get user data from Auth0
            string userId = GetUserID();
            string emailAddress = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            string userName = User.Claims.FirstOrDefault(c => c.Type == "nickname")?.Value;

            // get corresponding avatar from database, if any
            MySQLDatabaseContext dbContext = GetDBContext();
            string? avatar = await dbContext.GetAvatar(userId);
            avatar ??= User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            // return the model of the user
            return new UserModel(userId)
            {
                Email = emailAddress,
                Name = userName,
                Avatar = avatar
            };
        }

        /// <summary>
        /// Method <c>Dashboard</c> gets the dashboard for the user.
        /// </summary>
        /// <param name="searchQuery">The search query the user has entered for project search.</param>
        /// <returns>The user dashboard action result.</returns>
        [Authorize, HttpGet]
        public async Task<IActionResult> UserDashboard(string? searchQuery)
        {
            MySQLDatabaseContext dbContext = GetDBContext();
            
            UserModel userModel = await GetUser();
            List<ProjectModel> userProjectModels = await dbContext.GetProjects(userModel.ID);
            var searchProjectModels = new List<ProjectModel>();
            string? searchQueryString = null;
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQueryString = searchQuery;
                searchProjectModels = await dbContext.SearchProjects(searchQuery);
            }

            return View(new UserDashboardModels()
            {
                User = userModel,
                UserProjects = userProjectModels,
                SearchQuery = searchQueryString,
                ProjectSearchResults = searchProjectModels
            });
        }

        /// <summary>
        /// Method <c>Dashboard</c> gets the project dashboard.
        /// </summary>
        /// <param name="projectId">The ID of the project to get the dashboard for.</param>
        /// <returns>The project dashboard action result.</returns>
        [Authorize, HttpGet]
        public async Task<IActionResult> ProjectDashboard(string projectId, string searchQuery)
        {
            MySQLDatabaseContext dbContext = GetDBContext();
            Auth0ManagementContext authContext = GetAuthContext();

            ProjectModel project = await dbContext.GetProject(projectId);
            List<UserModel> developers = await dbContext.GetDevelopers(projectId);
            await authContext.FillUserData(developers);

            string userId = GetUserID();
            bool userIsDeveloper = await dbContext.IsDeveloper(userId, projectId);

            string? searchQueryString = null;
            var userSearchResults = new List<UserModel>();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQueryString = searchQuery;
                userSearchResults = authContext.SearchUsers(searchQuery);
                foreach (var user in userSearchResults)
                {
                    string? databaseAvatar = await dbContext.GetAvatar(user.ID);
                    if (databaseAvatar != null)
                    {
                        user.Avatar = databaseAvatar;
                    }
                }
            }

            return View(new ProjectDashboardModels()
            {
                Project = project,
                Developers = developers,
                IsDeveloper = userIsDeveloper,
                SearchQuery = searchQueryString,
                UserSearchResults = userSearchResults
            });
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
            return View();
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
            MySQLDatabaseContext dbContext = GetDBContext();
            await dbContext.AddProject(projectName, user.ID);
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
            string userId = GetUser().Result.ID;
            MySQLDatabaseContext dbContext = GetDBContext();
            await dbContext.DeleteProject(projectId, userId);
            return RedirectToAction("UserDashboard", "Account");
        }

        /// <summary>
        /// Method <c>AddDeveloper</c> adds a user as a developer to a project.
        /// </summary>
        /// <returns>The updated project dashboard action result.</returns>
        [Authorize, HttpPost]
        public async Task<IActionResult> AddDeveloper(string projectId, string userId)
        {
            MySQLDatabaseContext dbContext = GetDBContext();
            await dbContext.AddDeveloper(projectId, userId);

            var parameters = new { projectId };
            return RedirectToAction("ProjectDashboard", "Account", parameters);
        }

        /// <summary>
        /// Method <c>UpdateAvatar</c> updates the avatar of the user in the MySQL database with the uploaded image.
        /// </summary>
        /// <param name="fileInput">The uploaded image.</param>
        private async Task UpdateAvatar(IFormFile? fileInput)
        {
            if (fileInput != null)
            {
                // check file size
                long fileSize = fileInput.Length;
                double fileFizeMB = fileSize / (1024.0 * 1024.0);

                if (fileSize > 0 && fileFizeMB < 16)
                {
                    // convert input into byte array
                    using var memoryStream = new MemoryStream();
                    await fileInput.CopyToAsync(memoryStream);
                    byte[] fileData = memoryStream.ToArray();

                    // add data to database
                    MySQLDatabaseContext dbContext = GetDBContext();
                    dbContext.SetAvatar(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, fileData);
                }
            }
        }

        /// <summary>
        /// Method <c>UpdateProfile</c> handles user profile updates.
        /// </summary>
        /// <returns>The updated user profile action result.</returns>
        [Authorize, HttpPost]
        public async Task<IActionResult> UpdateProfile()
        {
            // get input from form
            string username = Request.Form["username"];
            IFormFile? fileInput = HttpContext.Request.Form.Files["image-file"];

            // request username update
            string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            Auth0ManagementContext authContext = GetAuthContext();
            await authContext.UpdateUsername(userId, username);

            // request avatar update
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
            // request account deletion
            string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            Auth0ManagementContext authContext = GetAuthContext();
            await authContext.DeleteUser(userId);

            // log the user out
            await Logout();
        }
    }
}
