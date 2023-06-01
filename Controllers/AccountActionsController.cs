using BugTracker.Models.DatabaseContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BugTracker.Controllers
{
    /// <summary>
    /// Class <c>AccountController</c> is a controller responsible for handling user input that 
    /// require database updates while the user is authenticated.
    /// </summary>
    [Authorize]
    public class AccountActionsController : Controller
    {
        /// <summary>
        /// Method <c>CreateProject</c> handles project creation.
        /// </summary>
        /// <returns>The action result for the updated user dashboard page.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateProject()
        {
            // get project data from user input
            string projectName = Request.Form["project-name"];

            // add project to database
            DatabaseContext dbContext = HttpContext.RequestServices.GetService(typeof(DatabaseContext)) as DatabaseContext;
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            await dbContext.SqlDb.AddProject(projectName, userId);

            // direct to the user dashboard page
            return RedirectToAction("Dashboard", "Account");
        }

        /// <summary>
        /// Method <c>DeleteProject</c> deletes the current user from a project (if user is the only developer, the project is deleted as well)
        /// </summary>
        /// <param name="projectId">The ID of the project to delete the user from.</param>
        /// <returns>The action result of the updated user dashboard page.</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteProject(string projectId)
        {
            // delete project
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            DatabaseContext dbContext = HttpContext.RequestServices.GetService(typeof(DatabaseContext)) as DatabaseContext;
            await dbContext.SqlDb.DeleteProject(projectId, userId);

            // direct to the user dashboard page
            return RedirectToAction("Dashboard", "Account");
        }

        /// <summary>
        /// Method <c>AddDeveloper</c> adds a user as a developer to a project.
        /// </summary>
        /// <returns>The action result of the updated project dashboard.</returns>
        [HttpPost]
        public async Task<IActionResult> AddDeveloper(string projectId, string userId)
        {
            // add developer
            DatabaseContext dbContext = HttpContext.RequestServices.GetService(typeof(DatabaseContext)) as DatabaseContext;
            await dbContext.SqlDb.AddDeveloper(projectId, userId);

            // direct to project dashboard page
            return RedirectToAction("Project", "Account", new { projectId });
        }

        /// <summary>
        /// Method<c>ReportBug</c> handles bug report creation.
        /// </summary>
        /// <param name="projectId">The ID of the project the bug report is for.</param>
        /// <returns>The action result of the updated project dashboard.</returns>
        [HttpPost]
        public async Task<IActionResult> ReportBug(string projectId)
        {
            // get user input for bug report data
            var report_fields = new Dictionary<string, string>
            {
                { "summary", Request.Form["report-summary"] },
                { "software_version", Request.Form["software-version"] },
                { "device", Request.Form["device"] },
                { "os", Request.Form["os"] },
                { "expected_result", Request.Form["expected-result"] },
                { "actual_result", Request.Form["actual-result"] },
                { "steps", Request.Form["steps"] },
                { "details", Request.Form["details"] }
            };

            // add bug report to database
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            DatabaseContext dbContext = HttpContext.RequestServices.GetService(typeof(DatabaseContext)) as DatabaseContext;
            await dbContext.SqlDb.AddReport(projectId, userId, report_fields);

            // redirect to the updated project dashboard
            return RedirectToAction("Project", "Account", new { projectId });
        }

        /// <summary>
        /// Method <c>UpdateAvatar</c> updates the avatar of the user in the MySQL database with the uploaded image.
        /// </summary>
        /// <param name="fileInput">The uploaded image.</param>
        private async Task UpdateAvatar(IFormFile? fileInput)
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
                DatabaseContext dbContext = HttpContext.RequestServices.GetService(typeof(DatabaseContext)) as DatabaseContext;
                string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                await dbContext.SqlDb.SetAvatar(userId, fileData);
            }
        }

        /// <summary>
        /// Method <c>UpdateProfile</c> handles user profile updates.
        /// </summary>
        /// <returns>The action result of the updated user profile.</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateProfile()
        {
            // get input from form
            string username = Request.Form["username"];
            IFormFile? fileInput = HttpContext.Request.Form.Files["image-file"];

            // update username in database
            string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            DatabaseContext dbContext = HttpContext.RequestServices.GetService(typeof(DatabaseContext)) as DatabaseContext;
            await dbContext.AuthDb.UpdateUsername(userId, username);

            // request avatar update
            if (fileInput != null)
            {
                await UpdateAvatar(fileInput);
            }

            // direct to updated profile page
            return RedirectToAction("Profile", "Account");
        }

        /// <summary>
        /// Method <c>DeleteAccount</c> handles the deletion of the user account.
        /// </summary>
        public async Task<IActionResult> DeleteAccount()
        {
            // delete account from database
            string? userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            DatabaseContext dbContext = HttpContext.RequestServices.GetService(typeof(DatabaseContext)) as DatabaseContext;
            await dbContext.AuthDb.DeleteUser(userId);

            // log the user out
            return RedirectToAction("Logout", "Account");
        }
    }
}
