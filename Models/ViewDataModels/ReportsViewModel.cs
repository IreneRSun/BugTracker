using BugTracker.Models.EntityModels;

namespace BugTracker.Models.ViewDataModels
{
    public class ReportsViewModel
    {
        public string? ProjectId { get; set; }
        public List<BugReportModel>? BugReports { get; set; }
        public string? FilterType { get; set; }
        public string? SortType { get; set; }
        public string? SortOrder { get; set; }
    }
}
