using BugTracker.Models.EntityModels;

namespace BugTracker.Models.ViewDataModels
{
    public class BugReportViewModel
    {
        public BugReportModel BugReport { get; }
        public List<UserModel> Assignees { get; }
        public bool UserUpvoted { get; }
        public List<CommentModel> Comments { get; }
        public List<UserModel> AvailableDevelopers { get; }
        public string CurrentUserId { get; }
        public bool IsDeveloper { get; }

        public BugReportViewModel(BugReportModel bugReport, List<UserModel> assignees, bool userUpvoted, List<CommentModel> comments, List<UserModel> availableDevelopers, string currentUserId, bool isDeveloper)
        {
            BugReport = bugReport;
            Assignees = assignees;
            UserUpvoted = userUpvoted;
            Comments = comments;
            AvailableDevelopers = availableDevelopers;
            CurrentUserId = currentUserId;
            IsDeveloper = isDeveloper;
        }
    }
}
