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
		/// <param name="uid">The ID of the user to get the dashboard of.
		/// Defaults to the current user's ID if empty.</param>
        /// <returns>The ViewResult for the user dashboard page.</returns>
        [Authorize]
        public async Task<IActionResult> Dashboard(string uid)
		{
			// get database access
			UserManagementContext usrCx = GetUserManagementCx();
			DatabaseContext dbCx = GetDbCx();

            // get representative models for the user and their projects
            string userId = uid.IsNullOrEmpty() ? GetUserId() : uid;

            UserModel userModel = await usrCx.GetUser(userId);
            await dbCx.GetUserData(userModel);

            List<ProjectModel> userProjectModels = await dbCx.GetProjects(userId);

            // return ViewResult
            var viewModel = new DashboardViewModel(userModel, userId == GetUserId(), userProjectModels);
            return View(viewModel);
		}

		/// <summary>
		/// Method <c>Profile</c> gets the ViewResult for the current user's profile.
		/// </summary>
		/// <param name="uid">The ID of the user to get the dashboard of.
		/// Defaults to the current user's ID if empty.</param>
		/// <returns>The ViewResult for the user's profile page.</returns>
		[Authorize]
        public async Task<IActionResult> Profile(string uid)
		{
			// get database access
			UserManagementContext usrCx = GetUserManagementCx();
            DatabaseContext dbCx = GetDbCx();

            // get representative model of current user
            string userId = uid.IsNullOrEmpty() ? GetUserId() : uid;
            UserModel userModel = await usrCx.GetUser(userId);
            await dbCx.GetUserData(userModel);

			// return ViewResult
			var viewModel = new ProfileViewModel(userModel, userId == GetUserId());
            return View(viewModel);
		}

        /// <summary>
        /// Method <c>Dashboard</c> gets the ViewResult for a project's dashboard.
        /// </summary>
        /// <param name="projectId">The ID of the project to get the dashboard for.</param>
        /// <returns>The ViewResult for the project's dashboard page.</returns>
        [Authorize]
        public async Task<IActionResult> Project(string projectId)
		{
			// get database access
            UserManagementContext usrCx = GetUserManagementCx();
            DatabaseContext dbCx = GetDbCx();

            // get representative models for the project and its developers
            string userId = GetUserId();

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
			var viewModel = new ProjectViewModel(project, newBugs, pendingBugs, fixedBugs, developers, userIsDeveloper);
            return View(viewModel);
        }

        /// <summary>
        /// Method <c>Tasks</c> gets the ViewResult for the current user's tasks(assigned bugs) page.
        /// </summary>
        /// <returns>The ViewResult of the current user's tasks page.</returns>
        [Authorize]
        public async Task<IActionResult> Tasks()
		{
            // get user assignments from database
            DatabaseContext dbCx = GetDbCx();
            string userId = GetUserId();
            List<BugReportModel> tasks = await dbCx.GetAssignments(userId);

			// return ViewResult
            var viewModel = new TasksViewModel(tasks);
            return View(viewModel);
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
            // get the project's sorted and filtered bug reports and page information
            DatabaseContext dbCx = GetDbCx();
            List<BugReportModel> tasks;

            if (sortType == "Upvotes")
            {
                tasks = await dbCx.GetReports(projectId, filter, "Date", "Descending");

                if (sortOrder == "Descending")
                {
                    tasks.Sort((report1, report2) => report2.Upvotes.CompareTo(report1.Upvotes));
                }
                else
                {
                    tasks.Sort((report1, report2) => report1.Upvotes.CompareTo(report2.Upvotes));
                }
            }
            else
            {
                tasks = await dbCx.GetReports(projectId, filter, sortType, sortOrder);
            }

			// return ViewResult
            var viewModel = new ReportsViewModel(projectId, tasks, filter, sortType, sortOrder);
            return View(viewModel);
        }

        /// <summary>
        /// Method <c>BugReport<c> gets the ViewResult for a page displaying a bug report.
        /// </summary>
        /// <param name="reportId">The ID of the bug report to display.</param>
        /// <returns>The ViewResult of the bug report page.</returns>
        [Authorize]
        public async Task<IActionResult> BugReport(string reportId)
		{
            // get database access
            UserManagementContext usrCx = GetUserManagementCx();
            DatabaseContext dbCx = GetDbCx();

            // get bug report data
            BugReportModel report = await dbCx.GetReport(reportId) ?? throw new Exception($"No reports with the ID {reportId} found.");

            // get developers assigned to the bug report
            List<UserModel> assignees = await dbCx.GetAssignees(reportId);
            foreach (var assignee in assignees)
            {
                await usrCx.GetUserData(assignee);
            }

            // get of developers of the project
            List<UserModel> developers = await dbCx.GetDevelopers(report.ProjectID);
            foreach (var developer in developers)
            {
                await usrCx.GetUserData(developer);
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

            // get whether the user has upvoted the bug report and whether it is a developer of the project
            string userId = GetUserId();
            bool userUpvoted = await dbCx.IsUserUpvoted(reportId, userId);
            bool isDeveloper = await dbCx.IsDeveloper(userId, report.ProjectID);

            // return the ViewResult with the page data
            var viewModel = new BugReportViewModel(report, assignees, userUpvoted, comments, developers, userId, isDeveloper);
            return View(viewModel);
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
