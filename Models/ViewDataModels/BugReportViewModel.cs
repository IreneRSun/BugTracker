using BugTracker.Models.EntityModels;

namespace BugTracker.Models.ViewDataModels
{
    public class BugReportViewModel
    {
        public BugReportModel BugReport { get; set; }
        public List<UserModel> Assignees { get; set; }
        public List<UserModel> AvailableDevelopers { get; set; }
        public string CurrentUserId { get; set; }
        public bool UserUpvoted { get; set; }
        public List<CommentModel> Comments { get; set; }
    }
}
