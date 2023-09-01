using BugTracker.Models.EntityModels;

namespace BugTracker.Models.DatabaseContexts
{
    /// <summary>
    /// Class <c>DatabaseContext</c> models a database context that manages connections to the database handling app data.
    /// </summary>
    public abstract class DatabaseContext
    {
        /// <summary>
        /// Method <c>RegisterUser</c> adds a user if they do not exist in the database.
        /// </summary>
        /// <param name="userId">The ID of the user to add.</param>
        public abstract Task RegisterUser(string userId);

        /// <summary>
        /// Method <c>DeleteUser</c> deletes a user from the database.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        public abstract Task DeleteUser(string userId);

        /// <summary>
        /// Method <c>GetUserData</c> fills a UserModel with corresponding user data from the database, if any found.
        /// </summary>
        /// <param name="user">The UserModel to fill with the user's data.
        /// User data retrieved corresponds to the ID attribute of the class.</param>
        public abstract Task GetUserData(UserModel user);

        /// <summary>
        /// Method <c>SetAvatar</c> sets the avatar of the user.
        /// </summary>
        /// <param name="userId">The user ID of the user to set the avatar of.</param>
        /// <param name="imageData">The byte array containing the data of the image to set the avatar to.</param>
        public abstract Task SetAvatar(string userId, byte[] imageData);

        /// <summary>
        /// Method <c>SetUserStatus</c> sets the status of a user.
        /// </summary>
        /// <param name="userId">The ID of the user to set the status of.</param>
        /// <param name="status">The status to set the user to.</param>
        public abstract Task SetUserStatus(string userId, string status);

        /// <summary>
        /// Method <c>SearchProjects</c> searches for projects that match the search query (case insensitive).
        /// </summary>
        /// <param name="searchInput">The search query.</param>
        /// <returns>The list of projects matching the search.</returns>
        public abstract Task<List<ProjectModel>> SearchProjects(string searchInput);

        /// <summary>
        /// Method <c>GetProjects</c> gets the projects that the user is a developer of.
        /// </summary>
        /// <param name="userId">The user ID of the user to get the projects of.</param>
        /// <returns>The list of projects that the user is a developer of.</returns>
        public abstract Task<List<ProjectModel>> GetProjects(string userId);

        /// <summary>
        /// Method <c>GetProject</c> gets the project associated with the given ID.
        /// Returns null if none found.
        /// </summary>
        /// <param name="projectId">The user ID of project to get.</param>
        /// <returns>The project associated with the given ID.</returns>
        public abstract Task<ProjectModel?> GetProject(string projectId);

        /// <summary>
        /// Method <c>AddProject</c> creates a project.
        /// </summary>
        /// <param name="projectName">The name to create the project with.</param>
        /// <param name="userId">The user ID of the user that created the project.</param>
        public abstract Task AddProject(string projectName, string userId);

        /// <summary>
        /// Method <c>DeleteProject</c> either deletes a project if there is only one developer, or deletes a developer from a project.
        /// </summary>
        /// <param name="projectId">The ID of the project that is being deleted.</param>
        /// <param name="developerId">The ID of the developer that is deleting the project.</param>
        public abstract Task DeleteProject(string projectId, string developerId);

        /// <summary>
        /// Method <c>GetDevelopers</c> gets the developers of a project.
        /// </summary>
        /// <param name="projectId">The ID of the project to get the developers of.</param>
        /// <returns>The list of developers that are part of a project.</returns>
        public abstract Task<List<UserModel>> GetDevelopers(string projectId);

        /// <summary>
        /// Method <c>IsDeveloper</c> checks if a user is a developer of a project.
        /// </summary>
        /// <param name="userId">The ID of the user to check.</param>
        /// <param name="projectId">The ID of the project to check.</param>
        /// <returns>Whether the user is a developer of the project.</returns>
        public abstract Task<bool> IsDeveloper(string userId, string projectId);

        /// <summary>
        /// Method <c>AddDeveloper</c> adds a developer to a project.
        /// </summary>
        /// <param name="projectId">The ID of the project to add a developer to.</param>
        /// <param name="developerId">The ID of the developer to add.</param>
        public abstract Task AddDeveloper(string projectId, string developerId);

        /// <summary>
        /// Method <c>GetReports</c> gets the bug reports of a project.
        /// </summary>
        /// <param name="projectId">The ID of the project to get the reports for.</param>
        /// <param name="filter">The status to filter for.</param>
        /// <param name="sortType">The column to sort by.</param>
        /// <param name="sortOrder">The order of the sort.</param>
        /// <returns>The list of bug reports for the project.</returns>
        public abstract Task<List<BugReportModel>> GetReports(string projectId, string filter, string sortType, string sortOrder);

        /// <summary>
        /// Method <c>GetReport</c> gets a bug report.
        /// Returns null if none found.
        /// </summary>
        /// <param name="reportId">The ID of the report to get.</param>
        /// <returns>The bug report.</returns>
        public abstract Task<BugReportModel?> GetReport(string reportId);

        /// <summary>
        /// Method <c>AddReport</c> creates a bug report.
        /// </summary>
        /// <param name="projectId">The ID of the project that the bug report is for.</param>
        /// <param name="userId">The ID of the user that is reporting the bug.</param>
        /// <param name="summary">The summary of the report.</param>
        /// <param name="softwareVersion">The software version of the project on which the bug occurred.</param>
        /// <param name="device">The device used when the bug occurred.</param>
        /// <param name="os">The operating system used when the bug occurred.</param>
        /// <param name="details">The details regarding the bug.</param>
        /// <returns></returns>
        public abstract Task AddReport(string projectId, string userId,
            string summary, decimal softwareVersion, string? device, string os, string details);

        /// <summary>
        /// Method <c>GetComments</c> gets the comments for a bug report.
        /// </summary>
        /// <param name="reportId">The ID of the report to get the comments of.</param>
        /// <returns>The list of comments for a bug report (ordered by date, descending).</returns>
        public abstract Task<List<CommentModel>> GetComments(string reportId);

        /// <summary>
        /// Method <c>AddComment</c> adds a comment to a bug report.
        /// </summary>
        /// <param name="reportId">The ID of the bug report the comment was made on.</param>
        /// <param name="userId">The ID of the user that made the comment.</param>
        /// <param name="comment">The comment the user made.</param>
        public abstract Task AddComment(string reportId, string userId, string comment);

        /// <summary>
        /// Method <c>GetTasks</c> gets all new or open bugs assigned to the user.
        /// </summary>
        /// <param name="userId">The ID of the user to get the assignments for.</param>
        /// <returns>The list of new or open bug reports that are assigned to the user.</returns>
        public abstract Task<List<BugReportModel>> GetTasks(string userId);

        /// <summary>
        /// Method <c>GetAssignees</c> gets the users assigned to a bug.
        /// </summary>
        /// <param name="reportId">The ID of the bug report to get the assigees for.</param>
        /// <returns>The list of assignees for a bug report.</returns>
        public abstract Task<List<UserModel>> GetAssignees(string reportId);

        /// <summary>
        /// Method <c>AddAssignment</c> assigns a developer to a bug report.
        /// </summary>
        /// <param name="reportId">The ID of the bug report to assign.</param>
        /// <param name="developerId">The ID of the developer to assign the bug to.</param>
        public abstract Task AddAssignment(string reportId, string developerId);

        /// <summary>
        /// Method <c>DeleteAssignment</c> removes an assignment from a bug report.
        /// </summary>
        /// <param name="reportId">The ID of bug report to remove the assignment from.</param>
        /// <param name="developerId">The ID of the developer to remove from the bug report.</param>
        public abstract Task DeleteAssignment(string reportId, string developerId);

        /// <summary>
        /// Method <c>UpdateBugTag</c> updates one of the bug report's tags (i.e. priority, severity, status, or help_wanted)
        /// </summary>
        /// <param name="reportId">The ID of the bug report to update the tag of.</param>
        /// <param name="tagName">The tag to update.</param>
        /// <param name="tagValue">The new value of the tag.</param>
        public abstract Task UpdateBugTag(string reportId, string tagName, string tagValue);

        /// <summary>
        /// Method <c>AddHelpWanted</c> sets the "Help Wanted" tag of a bug report to true.
        /// </summary>
        /// <param name="reportId">The ID of the report to set the tag of.</param>
        public abstract Task AddHelpWanted(string reportId);

        /// <summary>
        /// Method <c>RemoveHelpWanted</c> sets the "Help Wanted tag of a bug report to false.
        /// </summary>
        /// <param name="reportId">The ID of the report to set the tag of.</param>
        public abstract Task RemoveHelpWanted(string reportId);

        /// <summary>
        /// Method <c>GetProjectStatisitic</c> gets a statistic for a project as a number of bug reports that are part of the statistic.
        /// </summary>
        /// <param name="projectId">The ID of the project to get the statistic of.</param>
        /// <param name="statisticType">The type of statistic to retrieve (i.e. fixed, pending, or new).</param>
        /// <returns>The statistic retrieved.</returns>
        public abstract Task<int> GetProjectStatistic(string projectId, string statisticType);

        /// <summary>
        /// Method <c>GetUpvotes</c> gets the number of upvotes a report has.
        /// </summary>
        /// <param name="reportId">The ID of the report to get the number of upvotes of.</param>
        /// <returns>The number of upvotes.</returns>
        public abstract Task<int> GetUpvotes(string reportId);

        /// <summary>
        /// Method <c>IsUserUpvoted</c> checks whether an user upvoted a report.
        /// </summary>
        /// <param name="reportId">The ID of the report to check the upvotes of.</param>
        /// <param name="userId">The ID of the user to check the upvotes of.</param>
        /// <returns>Whether the user upvoted the report.</returns>
        public abstract Task<bool> IsUserUpvoted(string reportId, string userId);

        /// <summary>
        /// Method <c>AddUpvote</c> adds an upvote given by a user to a report.
        /// </summary>
        /// <param name="reportId">The ID of the report to add an upvote to.</param>
        /// <param name="userId">The ID of the user who made the upvote.</param>
        public abstract Task AddUpvote(string reportId, string userId);

        /// <summary>
        /// Method <c>DeleteUpvote</c> removes an upvote from a report.
        /// </summary>
        /// <param name="reportId">The ID of the report to remove an upvote from.</param>
        /// <param name="userId">The ID of the user to remove the upvote for.</param>
        public abstract Task DeleteUpvote(string reportId, string userId);
    }
}
