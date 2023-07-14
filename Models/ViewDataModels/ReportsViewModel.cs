using BugTracker.Models.EntityModels;

namespace BugTracker.Models.ViewDataModels
{
    public class ReportsViewModel
    {
        public string ProjectId { get; }
        public List<BugReportModel> BugReports { get; }
        public string FilterType { get; }
        public string SortType { get; }
        public string SortOrder { get; }

        public ReportsViewModel(string projectId, List<BugReportModel> bugReports, string filterType, string sortType, string sortOrder)
        {
            ProjectId = projectId;
            BugReports = bugReports;
            FilterType = filterType;
            SortType = sortType;
            SortOrder = sortOrder;
        }
    }
}
