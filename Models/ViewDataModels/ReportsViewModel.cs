using BugTracker.Models.EntityModels;

namespace BugTracker.Models.ViewDataModels
{
    public class ReportsViewModel
    {
        public List<BugReportModel> BugReports { get; set; }
        public string FilterType { get; set; }

        public ReportsViewModel()
        {
            FilterType = "All";
        }
    }
}
