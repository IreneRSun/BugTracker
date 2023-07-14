using BugTracker.Models.DatabaseContexts;
using BugTracker.Models.EntityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Web.WebPages;

namespace BugTracker.Controllers
{
    /// <summary>
    /// Class <c>AccountController</c> is a controller responsible for handling user input that 
    /// require database updates while the user is authenticated.
    /// </summary>
    [Authorize]
    public class AccountActionsController : DatabaseAccessingController
    {
        /// <summary>
        /// Method <c>CreateProject</c> handles project creation.
		/// Throws an exception if no project name is found.
        /// </summary>
        /// <returns>The action result for the updated user dashboard page.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateProject()
        {
            var projectName = Request.Form["project-name"];

            if (!projectName.IsNullOrEmpty())
            {
                // add project to database
                string userId = GetUserId();
                DatabaseContext dbCx = GetDbCx();
                await dbCx.AddProject(projectName, userId);

                // direct to the updated dashboard
                return RedirectToAction("Dashboard", "Account");
			}

            throw new Exception("Invalid database action with null values: No project name found.");
        }

        /// <summary>
        /// Method <c>DeleteProject</c> handles deleting the current user from a project.
		/// If user is the only developer, the project is deleted as well.
        /// </summary>
		/// <param name="projectId">The ID of the project to delete.</param>
        /// <returns>The action result of the updated user dashboard page.</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteProject(string projectId)
        {
			// delete project
            string userId = GetUserId();
            DatabaseContext dbCx = GetDbCx();
            await dbCx.DeleteProject(projectId, userId);

            // redirect to the current user's dashboard
            return RedirectToAction("Dashboard", "Account");
		}

		/// <summary>
		/// Method <c>SearchProjects</c> searches for projects that match the search query.
		/// </summary>
		/// <param name="search">The search query used for the search.</param>
		/// <param name="page">The page number of the search results.</param>
		/// <returns>The JsonResult with the search results.</returns>
		[HttpGet]
		public async Task<IActionResult> SearchProjects(string search, int page)
		{
            DatabaseContext dbCx = GetDbCx();

            List<ProjectModel> searchResults = await dbCx.SearchProjects(search);
            string resultJson = JsonConvert.SerializeObject(searchResults);

            return Json(resultJson);
		}

        /// <summary>
        /// Method <c>SelectProject</c> directs the website to the dashboard of the selected project.
		/// Throws an exception if no project is found.
        /// </summary>
        /// <returns>The action result of the selected project's dashboard.</returns>
        [HttpPost]
		public IActionResult SelectProject()
		{
			var projectId = Request.Form["project-search"];

			if (!projectId.IsNullOrEmpty())
			{
                return RedirectToAction("Project", "Account", new { projectId });
            }

			throw new Exception("Invalid action with null values: No selected project found.");
		}

        /// <summary>
        /// Method <c>SearchProjects</c> searches for users that match the search query.
        /// </summary>
        /// <param name="search">The search query used for the search.</param>
        /// <param name="page">The page number of the search results.</param>
        /// <returns>The JsonResult with the search results.</returns>
        [HttpGet]
		public async Task<IActionResult> SearchUsers(string search, int page)
		{
            UserManagementContext usrCx = GetUserManagementCx();
            DatabaseContext dbCx = GetDbCx();

            List<UserModel> searchResults = usrCx.SearchUsers(search);
            foreach (var user in searchResults)
            {
                await dbCx.GetUserData(user);
            }

            return Json(searchResults);
        }

		/// <summary>
		/// Method <c>AddDeveloper</c> adds a user as a developer to a project.
		/// Throws an exception if no selected user is found.
		/// </summary>
		/// <param name="projectId">The ID of the project to add the developer to.</param>
		/// <returns>The action result of the updated project dashboard.</returns>
		[HttpPost]
        public async Task<IActionResult> AddDeveloper(string projectId)
        {
			var userId = Request.Form["user-search"];

            if (!userId.IsNullOrEmpty())
            {
                // add the developer
                DatabaseContext dbCx = GetDbCx();
                await dbCx.AddDeveloper(projectId, userId);

                // redirect to the updated project dashboard
                return RedirectToAction("Project", "Account", new { projectId });
			}

            throw new Exception("Invalid database action with null values: No selected user found.");
        }

        /// <summary>
        /// Method<c>ReportBug</c> handles bug report creation.
		/// Throws an exception if the required input for creating the bug report is not found.
        /// </summary>
        /// <param name="projectId">The ID of the project the bug report is for.</param>
        /// <returns>The action result of the updated project dashboard.</returns>
        [HttpPost]
        public async Task<IActionResult> ReportBug(string projectId)
        {
			var summary = Request.Form["report-summary"];
			var softwareVersion = Request.Form["software-version"];
			var device = Request.Form["device"];
			var os = Request.Form["os"];
			var details = Request.Form["details"];
            
			if (!summary.IsNullOrEmpty() && !softwareVersion.IsNullOrEmpty() && !os.IsNullOrEmpty() && !details.IsNullOrEmpty())
            {
                // add bug report to database
                string userId = GetUserId();
                DatabaseContext dbCx = GetDbCx();
                await dbCx.AddReport(projectId, userId, summary, softwareVersion.ToString().AsDecimal(), device, os, details);

                // redirect to the updated project dashboard
                return RedirectToAction("Project", "Account", new { projectId });
			}

            throw new Exception("Invalid database action with null values: Required bug information not found.");
        }

		/// <summary>
		/// Method<c>AddAssignment</c> handles the assignment of the selected developer to a bug report.
		/// </summary>
		/// <param name="reportId">The ID of the bug report to assign the developer to.</param>
		/// <returns>The action result of the updated bug report.</returns>
		[HttpPost]
		public async Task<IActionResult> AddAssignment(string reportId)
		{
			// update report status
			var developerId = Request.Form["developer-select"];
			DatabaseContext dbCx = GetDbCx();
			await dbCx.AddAssignment(reportId, developerId);

			// redirect to the updated bug report
			return RedirectToAction("BugReport", "Account", new { reportId });
		}

		/// <summary>
		/// Method<c>DeleteAssignment</c> handles the deletion of an assignment of the selected developer to a bug report.
		/// </summary>
		/// <param name="reportId">The ID of the bug report to remove the assignment of.</param>
		/// <param name="developerId">The ID of the developer to delete the assignment of.</param>
		/// <returns>The action result of the updated bug report.</returns>
		[HttpPost]
		public async Task<IActionResult> DeleteAssignment(string reportId, string developerId)
		{
			// update report status and redirect to the updated bug report
			DatabaseContext dbCx = GetDbCx();			
			await dbCx.DeleteAssignment(reportId, developerId);
			return RedirectToAction("BugReport", "Account", new { reportId });
		}

		/// <summary>
		/// Method<c>UpdateBugTag</c> handles bug report tag updating.
		/// Throws an exception if the tag is not found.
		/// </summary>
		/// <param name="reportId">The ID of the report to update the tag of.</param>
		/// <param name="tagType">The tag(column) of the bug report to update (status, priority, or severity).</param>
		/// <returns>The action result of the updated bug report.</returns>
		[HttpPost]
		public async Task<IActionResult> UpdateBugTag(string reportId, string tagType)
        {
            string selectedStatus = Request.Form[$"{tagType}-select"];

            if (!selectedStatus.IsNullOrEmpty()) {
				// update report tag and redirect to updated bug report
				DatabaseContext dbCx = GetDbCx();
				await dbCx.UpdateBugTag(reportId, tagType, selectedStatus);
				return RedirectToAction("BugReport", "Account", new { reportId });

			}

			throw new Exception("Invalid database action with null values: Specified tag not found.");
		}

		/// <summary>
		/// Method <c>Comment</c> handles commenting on bug reports.
		/// Throws an exception if the comment is not found.
		/// </summary>
		/// <param name="reportId">The ID of the report to add a comment to.</param>
		/// <returns>The action result of the updated bug report.</returns>
		[HttpPost]
		public async Task<IActionResult> Comment(string reportId)
		{
			string comment = Request.Form["user-comment"];

			if (!comment.IsNullOrEmpty())
			{
				// update report tag
				DatabaseContext dbCx = GetDbCx();
				string userId = GetUserId();
				await dbCx.AddComment(reportId, userId, comment);

				// redirect to the updated bug report
				return RedirectToAction("BugReport", "Account", new { reportId });

			}

			throw new Exception("Invalid database action with null values: No comment found.");
		}

		/// <summary>
		/// Method <c>Upvote</c> adds an upvote to a bug report.
		/// </summary>
		/// <param name="reportId">The ID of the report to add an upvote to.</param>
		/// <returns>The action result of the updated report's page.</returns>
		[HttpPost]
		public async Task<IActionResult> Upvote(string reportId)
		{
			// upvote bug report
			DatabaseContext dbCx = GetDbCx();
			string userId = GetUserId();
			await dbCx.AddUpvote(reportId, userId);

			// redirect to updated bug report page
			return RedirectToAction("BugReport", "Account", new { reportId });
		}

		/// <summary>
		/// Method <c>Upvote</c> removes an upvote from a bug report.
		/// </summary>
		/// <param name="reportId">The ID of the report to remove an upvote from.</param>
		/// <returns>The action result of the updated report's page.</returns>
		[HttpPost]
		public async Task<IActionResult> RemoveUpvote(string reportId)
		{
			// un-upvote bug report
			DatabaseContext dbCx = GetDbCx();
			string userId = GetUserId();
			await dbCx.DeleteUpvote(reportId, userId);

			// redirect to updated bug report page
			return RedirectToAction("BugReport", "Account", new { reportId });
		}

		/// <summary>
		/// Method <c>UpdateHelpWanted</c> updates the help wanted status of a bug report.
		/// </summary>
		/// <param name="reportId">The ID of the report to update the help wanted status of.</param>
		/// <returns>The action result of the updated bug report's page.</returns>
		[HttpPost]
		public async Task<IActionResult> UpdateHelpWanted(string reportId)
		{
			bool helpWanted = Request.Form["help-wanted"] == "on";

			// add help wanted label to bug report
			DatabaseContext dbCx = GetDbCx();

			if (helpWanted)
			{
				await dbCx.AddHelpWanted(reportId);
			}
			else
			{
				await dbCx.RemoveHelpWanted(reportId);
			}

			// redirect to updated bug report page
			return RedirectToAction("BugReport", "Account", new { reportId });
		}

		/// <summary>
		/// Method <c>UpdateAvatar</c> updates the avatar of the user in the database context with the uploaded image.
		/// </summary>
		/// <param name="userId">The ID of the user to update the avatar of.</param>
		/// <param name="fileInput">The uploaded image.</param>
		private async Task UpdateAvatar(string userId, IFormFile fileInput)
		{
            // check file size
            long fileSize = fileInput.Length;
            double fileFizeKB = fileSize / (1024.0);

            if (fileSize > 0 && fileFizeKB < 64)
            {
                // convert input into byte array
                using var memoryStream = new MemoryStream();
                await fileInput.CopyToAsync(memoryStream);
                byte[] fileData = memoryStream.ToArray();

                // add data to database
                DatabaseContext dbCx = GetDbCx();
                await dbCx.SetAvatar(userId, fileData);
            }
		}

        /// <summary>
        /// Method <c>UpdateProfile</c> handles user profile updates.
        /// </summary>
        /// <returns>The action result of the updated user profile.</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateProfile()
        {
            var username = Request.Form["username-input"];
			var status = Request.Form["user-status-select"];
			IFormFile? fileInput = HttpContext.Request.Form.Files["image-file-upload"];

            string userId = GetUserId();
            DatabaseContext dbCx = GetDbCx();
            UserManagementContext usrCx = GetUserManagementCx();

            // update username in database
            await usrCx.UpdateUsername(userId, username);

            // update user status in database
            await dbCx.SetUserStatus(userId, status);

            // update user avatar in database
            if (fileInput != null)
            {
                await UpdateAvatar(userId, fileInput);
            }

            // redirect to updated profile page
            return RedirectToAction("Profile", "Account");
        }

        /// <summary>
        /// Method <c>DeleteAccount</c> handles the deletion of the current user's account.
        /// </summary>
        public async Task<IActionResult> DeleteAccount()
        {
            // delete account from databases
            UserManagementContext usrCx = GetUserManagementCx();
            DatabaseContext dbCx = GetDbCx();

            string userId = GetUserId();
            await usrCx.DeleteUser(userId);
            await dbCx.DeleteUser(userId);

            // log the user out
            return RedirectToAction("LoggedOut", "Account");
        }
    }
}
