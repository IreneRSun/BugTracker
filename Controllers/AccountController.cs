using BugTracker.Models.DatabaseContexts;
using BugTracker.Models.EntityModels;
using BugTracker.Models.ViewDataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BugTracker.Controllers
{
	/// <summary>
	/// Class <c>AccountController</c> is a controller handling the website pages when a user is logged in.
	/// </summary>
	public class AccountController : DatabaseAccessingController
	{
        /// <summary>
        /// Method <c>Dashboard</c> gets the ViewResult for a user's dashboard.
        /// </summary>
		/// <param name="id">The ID of the user to get the dashboard of.
		/// Defaults to the current user's ID if empty.</param>
        /// <returns>The ViewResult for the user dashboard page.</returns>
        [Authorize]
        public async Task<IActionResult> Dashboard(string id)
		{
			string? userId = id.IsNullOrEmpty() ? GetUserId() : id;
			UserManagementContext? usrCx = GetUserManagementCx();
			DatabaseContext? dbCx = GetDbCx();

			if (usrCx != null && dbCx != null && userId != null) {
				// get representative models for the user and their projects
				UserModel userModel = await usrCx.GetUser(userId);
				await dbCx.GetUserData(userModel);

				List<ProjectModel> userProjectModels = await dbCx.GetProjects(userId);

				// return ViewResult
				var viewModel = new DashboardViewModel()
				{
					User = userModel,
					UserProjects = userProjectModels
				};

				return View(viewModel);
			}
			else
			{
				throw new Exception("Could not access database services.");
			}
		}

		/// <summary>
		/// Method <c>Profile</c> gets the ViewResult for the current user's profile.
		/// </summary>
		/// <param name="id">The ID of the user to get the dashboard of.
		/// Defaults to the current user's ID if empty.</param>
		/// <returns>The ViewResult for the user's profile page.</returns>
		[Authorize]
        public async Task<IActionResult> Profile(string id)
		{
			string? userId = id.IsNullOrEmpty() ? GetUserId() : id;
			UserManagementContext? usrCx = GetUserManagementCx();
            DatabaseContext? dbCx = GetDbCx();

            if (usrCx != null && dbCx != null && userId != null) {
                // get representative model of current user
                UserModel userModel = await usrCx.GetUser(userId);
				await dbCx.GetUserData(userModel);

				// return ViewResult
				var viewModel = new ProfileViewModel()
				{
					User = userModel
				};

				return View(viewModel);
			}
			else
			{
				throw new Exception("Could not access database services.");
			}
		}

        /// <summary>
        /// Method <c>Dashboard</c> gets the ViewResult for a project's dashboard.
        /// </summary>
        /// <param name="projectId">The ID of the project to get the dashboard for.</param>
        /// <returns>The ViewResult for the project's dashboard page.</returns>
        [Authorize]
        public async Task<IActionResult> Project(string projectId)
		{
            string? userId = GetUserId();
            UserManagementContext? usrCx = GetUserManagementCx();
            DatabaseContext? dbCx = GetDbCx();

            if (usrCx != null && dbCx != null && userId != null)
			{
				// get representative models for the project and its developers
				ProjectModel project = await dbCx.GetProject(projectId) ?? throw new Exception($"No projects with the ID {projectId} found.");
				List<UserModel> developers = await dbCx.GetDevelopers(projectId);
				foreach (UserModel developer in developers)
				{
					await usrCx.GetUserData(developer);
				}

				// get project statisics
				int newBugs = await dbCx.GetProjectStatistic(projectId, "new");
				int pendingBugs = await dbCx.GetProjectStatistic(projectId, "pending");
				int fixedBugs = await dbCx.GetProjectStatistic(projectId, "fixed");

				// check if the current user is a developer of this project
				bool userIsDeveloper = await dbCx.IsDeveloper(userId, projectId);

				// get ViewResult
				var viewModel = new ProjectViewModel()
				{
					Project = project,
					Developers = developers,
					NewBugs = newBugs,
					PendingBugs = pendingBugs,
					FixedBugs = fixedBugs,
					IsDeveloper = userIsDeveloper
				};

				return View(viewModel);
			}
			else
			{
				throw new Exception("Could not access database services.");
			}
		}

        /// <summary>
        /// Method <c>Tasks</c> gets the ViewResult for the current user's tasks(assigned bugs) page.
        /// </summary>
        /// <returns>The ViewResult of the current user's tasks page.</returns>
        [Authorize]
        public async Task<IActionResult> Tasks()
		{
            string? userId = GetUserId();
            DatabaseContext? dbCx = GetDbCx();

            if (dbCx != null && userId != null)
			{
				// return ViewResult with data on user assignments
				List<BugReportModel> tasks = await dbCx.GetAssignments(userId);
				var viewModel = new TasksViewModel()
				{
					BugReports = tasks
				};
				return View(viewModel);
			}
			else
			{
				throw new Exception("Could not access database services.");
			}
		}

		/// <summary>
		/// Method <c>Reports</c> gets the ViewResult for the page displaying a project's reports.
		/// </summary>
		/// <param name="projectId">The ID of the project to display the reports of.</param>
		/// <param name="filter">What status to filter the reports by.</param>
		/// <param name="sortType">What to sort the reports by.</param>
		/// <param name="sortOrder">What order to sort the reports in.</param>
		/// <returns>The ViewResult of the reports page.</returns>
		[Authorize, HttpGet]
		public async Task<IActionResult> Reports(string projectId, string filter = "All", string sortType = "Date", string sortOrder = "Descending")
		{
            DatabaseContext? dbCx = GetDbCx();

            if (dbCx != null)
			{
				// return ViewResult with the project's sorted and filtered bug reports and page information
				List<BugReportModel> tasks = await dbCx.GetReports(projectId, filter, sortType, sortOrder);
				var viewModel = new ReportsViewModel()
				{
					ProjectId = projectId,
					BugReports = tasks,
					FilterType = filter,
					SortType = sortType,
					SortOrder = sortOrder
				};
				return View(viewModel);
			}
			else
			{
				throw new Exception("Could not access database services.");
			}
		}

        /// <summary>
        /// Method <c>BugReport<c> gets the ViewResult for a page displaying a bug report.
        /// </summary>
        /// <param name="reportId">The ID of the bug report to display.</param>
        /// <returns>The ViewResult of the bug report page.</returns>
        [Authorize]
        public async Task<IActionResult> BugReport(string reportId)
		{
			string? userId = GetUserId();
            UserManagementContext? usrCx = GetUserManagementCx();
            DatabaseContext? dbCx = GetDbCx();

            if (userId != null && usrCx != null && dbCx != null)
			{
				// get bug report data
				BugReportModel report = await dbCx.GetReport(reportId) ?? throw new Exception($"No reports with the ID {reportId} found.");

				// get developers assigned to the bug report
				List<UserModel> assignees = await dbCx.GetAssignees(reportId);
				foreach (var assignee in assignees)
				{
					await dbCx.GetUserData(assignee);
				}

				// get of developers of the project
				List<UserModel> developers = await dbCx.GetDevelopers(report.ProjectID);
				foreach (var developer in developers)
				{
					await dbCx.GetUserData(developer);
				}

				// get the comments of the bug report
				List<CommentModel> comments = await dbCx.GetComments(report.ID);
				foreach (var comment in comments)
				{
					if (comment.Commenter != null)
					{
						await usrCx.GetUserData(comment.Commenter);
					}
				}

				// return the ViewResult with the page data
				var viewModel = new BugReportViewModel()
				{
					BugReport = report,
					Assignees = assignees,
					AvailableDevelopers = developers,
					CurrentUserId = userId,
					Comments = comments
				};
				return View(viewModel);
			}
			else
			{
				throw new Exception("Could not access database services.");
			}
		}

		/// <summary>
		/// Method <c>LoggedOut</c> gets the ViewResult for the page indicating that the user has logged out.
		/// </summary>
		/// <returns>The ViewResult of the logged out page.</returns>
		public IActionResult LoggedOut()
		{
			return View();
		}
	}
}
