using BugTracker.Models.EntityModels;

namespace BugTracker.Models.ViewDataModels
{
    public class DashboardViewModel
    {
        public UserModel User { get; set; }
        public List<ProjectModel> UserProjects { get; set; }
    }
}
