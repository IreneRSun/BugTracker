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
        /// </summary>
        /// <returns>The action result for the updated user dashboard page.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateProject()
        {
            // get data for creating project
            string? projectName = Request.Form["project-name"];
            string? userId = GetUserId();
            DatabaseContext? dbCx = GetDbCx();

            if (dbCx != null && userId != null && projectName != null)
            {
				// add project to database and direct to the updated dashboard
				await dbCx.AddProject(projectName, userId);
				return RedirectToAction("Dashboard", "Account");
			}
            else if (dbCx == null)
            {
				throw new Exception("Could not access database services.");
			}
			else
			{
				throw new Exception("Invalid database action with null values.");
			}
		}

        /// <summary>
        /// Method <c>DeleteProject</c> handles deleting the current user from a project.
		/// If user is the only developer, the project is deleted as well.
        /// </summary>
        /// <returns>The action result of the updated user dashboard page.</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteProject()
        {
			string? projectId = Request.Form["project-id"];
            string? userId = GetUserId();
            DatabaseContext? dbCx = GetDbCx();

            if (dbCx != null && userId != null && projectId != null)
            {
				// delete project and redirect to the current user's dashboard
				await dbCx.DeleteProject(projectId, userId);
				return RedirectToAction("Dashboard", "Account");
			}
			else if (dbCx == null)
			{
				throw new Exception("Could not access database services.");
			}
			else
			{
				throw new Exception("Invalid database action with null values.");
			}
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
            DatabaseContext? dbCx = GetDbCx();

            if (dbCx != null)
			{
				List<ProjectModel> searchResults = await dbCx.SearchProjects(search);
                string resultJson = JsonConvert.SerializeObject(searchResults);
                return Json(resultJson);
			}
			else
			{
				throw new Exception("Could not access database services.");
			}
		}

        /// <summary>
        /// Method <c>SelectProject</c> directs the website to the dashboard of the selected project.
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
			throw new Exception("Invalid action with null values.");
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
            UserManagementContext? usrCx = GetUserManagementCx();
            DatabaseContext? dbCx = GetDbCx();

            if (usrCx != null && dbCx != null)
			{
				List<UserModel> searchResults = usrCx.SearchUsers(search);
				foreach (var user in searchResults)
				{
					await dbCx.GetUserData(user);
				}
				return Json(searchResults);
			}
			else
			{
				throw new Exception("Could not access database services.");
			}
		}

		/// <summary>
		/// Method <c>AddDeveloper</c> adds a user as a developer to a project.
		/// </summary>
		/// <returns>The action result of the updated project dashboard.</returns>
		[HttpPost]
        public async Task<IActionResult> AddDeveloper()
        {
			// get form input
			string? projectId = Request.Form["project-id"];
			string? userId = Request.Form["user-search"];

            DatabaseContext? dbCx = GetDbCx();

            if (dbCx != null && projectId != null && userId != null)
            {
                // add the developer and redirect to the updated project dashboard
				await dbCx.AddDeveloper(projectId, userId);
				return RedirectToAction("Project", "Account", new { projectId });
			}
			else if (dbCx == null)
			{
				throw new Exception("Could not access database services.");
			}
			else
			{
				throw new Exception("Invalid database action with null values.");
			}
        }

        /// <summary>
        /// Method<c>ReportBug</c> handles bug report creation.
        /// </summary>
        /// <param name="projectId">The ID of the project the bug report is for.</param>
        /// <returns>The action result of the updated project dashboard.</returns>
        [HttpPost]
        public async Task<IActionResult> ReportBug()
        {
			// get form input
			string? projectId = Request.Form["project-id"];
			string? summary = Request.Form["report-summary"];
			string? softwareVersion = Request.Form["software-version"];
			string? device = Request.Form["device"];
			string? os = Request.Form["os"];
			string? details = Request.Form["details"];

            // add bug report to database
            string? userId = GetUserId();
            DatabaseContext? dbCx = GetDbCx();

            if (dbCx != null && projectId != null && userId != null && summary != null && softwareVersion != null && os != null && details != null)
            {
                // add bug report to database and redirect to the updated project dashboard
				await dbCx.AddReport(projectId, userId, summary, softwareVersion.AsDecimal(), device, os, details);
				return RedirectToAction("Project", "Account", new { projectId });
			}
			else if (dbCx == null)
			{
				throw new Exception("Could not access database services.");
			}
			else
			{
				throw new Exception("Invalid database action with null values.");
			}
		}

		/// <summary>
		/// Method<c>AddAssignment</c> handles the assignment of the selected developer to a bug report.
		/// </summary>
		/// <returns>The action result of the updated bug report.</returns>
		[HttpPost]
		public async Task<IActionResult> AddAssignment()
		{
			// get form input
			string reportId = Request.Form["report-id"];
			string? userId = Request.Form["developer-select"];

            DatabaseContext? dbCx = GetDbCx();

            if (dbCx != null && reportId != null && userId != null)
            {
				// update report status and redirect to the updated bug report
				await dbCx.AddAssignment(reportId, userId);
				return RedirectToAction("BugReport", "Account", new { reportId });
			}
			else if (dbCx == null)
			{
				throw new Exception("Could not access database services.");
			}
			else
			{
				throw new Exception("Invalid database action with null values.");
			}
		}

		/// <summary>
		/// Method<c>DeleteAssignment</c> handles the deletion of an assignment of the selected developer to a bug report.
		/// </summary>
		/// <returns>The action result of the updated bug report.</returns>
		[HttpPost]
		public async Task<IActionResult> DeleteAssignment()
		{
			// get form input
			string reportId = Request.Form["report-id"];
			string developerId = Request.Form["developer-id"];

			DatabaseContext? dbCx = GetDbCx();

            if (dbCx != null && reportId != null && developerId != null)
            {
				// update report status and redirect to the updated bug report
				await dbCx.DeleteAssignment(reportId, developerId);
				return RedirectToAction("BugReport", "Account", new { reportId });
			}
			else if (dbCx == null)
			{
				throw new Exception("Could not access database services.");
			}
			else
			{
				throw new Exception("Invalid database action with null values.");
			}
		}

		/// <summary>
		/// Method<c>UpdateBugTag</c> handles bug report tag updating.
		/// </summary>
		/// <param name="tagType">The tag(column) of the bug report to update (status, priority, or severity)</param>
		/// <returns>The action result of the updated bug report.</returns>
		[HttpPost]
		public async Task<IActionResult> UpdateBugTag(string tagType)
        {
			// get form input
			string reportId = Request.Form["report-id"];
            string selectedStatus = Request.Form[$"{tagType}-select"];

            DatabaseContext? dbCx = GetDbCx();

            if (dbCx != null && reportId != null && selectedStatus != null) {
				// update report tag and redirect to updated bug report
				await dbCx.UpdateBugTag(reportId, tagType, selectedStatus);
				return RedirectToAction("BugReport", "Account", new { reportId });

			}
			else if (dbCx == null)
			{
				throw new Exception("Could not access database services.");
			}
			else
			{
				throw new Exception("Invalid database action with null values.");
			}
		}

		/// <summary>
		/// Method <c>Comment</c> handles commenting on bug reports.
		/// </summary>
		/// <returns>The action result of the updated bug report.</returns>
		[HttpPost]
		public async Task<IActionResult> Comment()
		{
			// get form input
			string reportId = Request.Form["report-id"];
			string comment = Request.Form["user-comment"];

			string? userId = GetUserId();
			DatabaseContext? dbCx = GetDbCx();

			if (dbCx != null && reportId != null && userId != null && comment != null)
			{
				// update report tag and redirect to the updated bug report
				await dbCx.AddComment(reportId, userId, comment);
				return RedirectToAction("BugReport", "Account", new { reportId });

			}
			else if (dbCx == null)
			{
				throw new Exception("Could not access database services.");
			}
			else
			{
				throw new Exception("Invalid database action with null values.");
			}
		}

		/// <summary>
		/// Method <c>UpdateAvatar</c> updates the avatar of the user in the database context with the uploaded image.
		/// </summary>
		/// <param name="userId">The ID of the user to update the avatar of.</param>
		/// <param name="fileInput">The uploaded image.</param>
		private async Task UpdateAvatar(string userId, IFormFile fileInput)
		{
            DatabaseContext? dbCx = GetDbCx();

            if (dbCx != null)
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
					await dbCx.SetAvatar(userId, fileData);
				}
			}
			else
			{
				throw new Exception("Could not access database services.");
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

            string? userId = GetUserId();
            UserManagementContext? usrCx = GetUserManagementCx();

            if (usrCx != null && userId != null && fileInput != null)
			{
				// update username in database
				await usrCx.UpdateUsername(userId, username);

				// request avatar update
				if (fileInput != null)
				{
					await UpdateAvatar(userId, fileInput);
				}

				// direct to updated profile page
				return RedirectToAction("Profile", "Account");
			}
			else if (usrCx == null)
			{
				throw new Exception("Could not access database services.");
			}
			else
			{
				throw new Exception("Invalid database action with null values.");
			}
		}

        /// <summary>
        /// Method <c>DeleteAccount</c> handles the deletion of the user account.
        /// </summary>
        public async Task<IActionResult> DeleteAccount()
        {
            string? userId = GetUserId();
            UserManagementContext? usrCx = GetUserManagementCx();
            DatabaseContext? dbCx = GetDbCx();

            if (usrCx != null && dbCx != null && userId != null)
			{
				// delete account from databases
				await usrCx.DeleteUser(userId);
				await dbCx.DeleteUser(userId);

				// log the user out
				return RedirectToAction("LoggedOut", "Account");
			}
			else if (userId == null)
			{
				throw new Exception("Could not find current user ID.");
			}
			else
			{
				throw new Exception("Could not access database services.");
			}
        }
    }
}
