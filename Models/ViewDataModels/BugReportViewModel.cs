using BugTracker.Models.EntityModels;

namespace BugTracker.Models.ViewDataModels
{
    public class BugReportViewModel
    {
        public BugReportModel BugReport { get; set; }
        public List<UserModel> Assignees { get; set; }
        public List<UserModel> AvailableDevelopers { get; set; }
    }
}
