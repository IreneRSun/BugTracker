﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BugTracker.Models.EntityModels;
using BugTracker.Models.ViewDataModels;

namespace BugTracker.Controllers
{
    /// <summary>
    /// Class <c>AccountController</c> is a controller handling user authentication
    /// and the pages that are available when the user is authenticated.
    /// </summary>
    public class AccountController : DatabaseAccessingController
    {
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
            // make sure the user is registered in the MySQL database
            string userId = GetUserId();
            var dbContext = GetDbCxt();
            if (userId != null && dbContext != null)
            {
                await dbContext.SqlDb.AddUserIfNone(userId);
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
        /// Method <c>DatabaseError</c> gets the ViewResult for the database error page. 
        /// </summary>
        /// <param name="errMsg">The error message to display on the page.</param>
        /// <returns>The ViewResult for the database error page.</returns>
        public IActionResult DatabaseError(string errMsg = "Unable to access database")
        {
            var viewModel = new DatabaseErrorViewModel()
            {
                ErrorMsg = errMsg
            };

            return View(viewModel);
        }

        /// <summary>
        /// Method <c>Dashboard</c> gets the ViewResult for the current user's dashboard.
        /// </summary>
        /// <param name="searchQuery">The search query the user has entered for project search.</param>
        /// <returns>The ViewResult for the user dashboard page.</returns>
        [Authorize, HttpGet]
        public async Task<IActionResult> Dashboard(string? searchQuery)
        {
            // get the current user's ID and the database context
            string? userId = GetUserId();
            var dbContext = GetDbCxt();

            // get representative models for the user and theeir projects
            UserModel userModel = await dbContext.GetUser(userId);
            List<ProjectModel> userProjectModels = await dbContext.SqlDb.GetProjects(userId);

            // get search results if the user entered a search query
            var searchProjectModels = new List<ProjectModel>();
            string? searchQueryString = null;
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQueryString = searchQuery;
                searchProjectModels = await dbContext.SqlDb.SearchProjects(searchQuery);
            }

            // return ViewResult
            var viewModel = new DashboardViewModel()
            {
                User = userModel,
                UserProjects = userProjectModels,
                SearchQuery = searchQueryString,
                ProjectSearchResults = searchProjectModels
            };

            return View(viewModel);
        }

        /// <summary>
        /// Method <c>Profile</c> gets the ViewResult for the current user's profile.
        /// </summary>
        /// <returns>The ViewResult for the user's profile page.</returns>
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            // get current user
            string userId = GetUserId();
            var dbContext = GetDbCxt();
            UserModel user = await dbContext.GetUser(userId);

            // return ViewResult
            var viewModel = new ProfileViewModel()
            {
                User = user
            };

            return View(viewModel);
        }

        /// <summary>
        /// Method <c>Dashboard</c> gets the ViewResult for a project's dashboard.
        /// </summary>
        /// <param name="projectId">The ID of the project to get the dashboard for.</param>
        /// <returns>The ViewResult for the project's dashboard page.</returns>
        [Authorize, HttpGet]
        public async Task<IActionResult> Project(string projectId, string searchQuery)
        {
            // get representative models for the project and its developers
            var dbContext = GetDbCxt();
            ProjectModel project = await dbContext.SqlDb.GetProject(projectId);
            List<UserModel> developers = await dbContext.SqlDb.GetDevelopers(projectId);
            foreach (var developer in developers)
            {
                developer.Name = await dbContext.AuthDb.GetName(developer.ID);
				developer.Avatar ??= await dbContext.AuthDb.GetDefaultAvatar(developer.ID);
            }

            // get project statisics
            int newBugs = await dbContext.SqlDb.GetProjectStatistic(projectId, "new");
            int pendingBugs = await dbContext.SqlDb.GetProjectStatistic(projectId, "pending");
            int fixedBugs = await dbContext.SqlDb.GetProjectStatistic(projectId, "fixed");

            // check if the current user is a developer of this project
            string userId = GetUserId();
            bool userIsDeveloper = await dbContext.SqlDb.IsDeveloper(userId, projectId);

            // search for users, if user entered a search query
            string? searchQueryString = null;
            var userSearchResults = new List<UserModel>();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQueryString = searchQuery;
                userSearchResults = dbContext.AuthDb.SearchUsers(searchQuery);
                foreach (var user in userSearchResults)
                {
                    string? databaseAvatar = await dbContext.SqlDb.GetAvatar(user.ID);
                    if (databaseAvatar != null)
                    {
                        user.Avatar = databaseAvatar;
                    }
                }
            }

            // get ViewResult
            var viewModel = new ProjectViewModel()
            {
                Project = project,
                Developers = developers,
                NewBugs = newBugs,
                PendingBugs = pendingBugs,
                FixedBugs = fixedBugs,
                IsDeveloper = userIsDeveloper,
                SearchQuery = searchQueryString,
                UserSearchResults = userSearchResults
            };

            return View(viewModel);
        }

        /// <summary>
        /// Method <c>Tasks</c> gets the ViewResult for the tasks page.
        /// </summary>
        /// <returns>The ViewResult of the issues page.</returns>
        [Authorize]
		public async Task<IActionResult> Tasks()
        {
            string? userId = GetUserId();
            var dbContext = GetDbCxt();
            List<BugReportModel> tasks = await dbContext.SqlDb.GetAssignments(userId);
            var viewModel = new TasksViewModel()
            {
                BugReports = tasks
            };
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
            var dbContext = GetDbCxt();
            List<BugReportModel> tasks = await dbContext.SqlDb.GetReports(projectId, filter, sortType, sortOrder);
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

        /// <summary>
        /// Method <c>BugReport<c> gets the ViewResult for the bug report page.
        /// </summary>
        /// <param name="reportId">The ID of the bug report to display.</param>
        /// <returns>The ViewResult of the issues page.</returns>
        [Authorize]
		public async Task<IActionResult> BugReport(string reportId)
        {
			var dbContext = GetDbCxt();
			BugReportModel report = await dbContext.SqlDb.GetReport(reportId);
            List<UserModel> assignees = await dbContext.SqlDb.GetAssignees(reportId);
			foreach (var assignee in assignees)
			{
				assignee.Name = await dbContext.AuthDb.GetName(assignee.ID);
				assignee.Avatar ??= await dbContext.AuthDb.GetDefaultAvatar(assignee.ID);
			}
			List<UserModel> developers = await dbContext.SqlDb.GetDevelopers(report.ProjectID);
			foreach (var developer in developers)
			{
				developer.Name = await dbContext.AuthDb.GetName(developer.ID);
				developer.Avatar ??= await dbContext.AuthDb.GetDefaultAvatar(developer.ID);
			}
			var viewModel = new BugReportViewModel()
            {
                BugReport = report,
                Assignees = assignees,
                AvailableDevelopers = developers
            };
            return View(viewModel);
        }
    }
}
