using BugTracker.Models.EntityModels;

namespace BugTracker.Models.ViewDataModels
{
	public class TasksViewModel
	{
		public List<BugReportModel> BugReports { get; }

		public TasksViewModel(List<BugReportModel> bugReports)
		{
			BugReports = bugReports;
		}
	}
}
