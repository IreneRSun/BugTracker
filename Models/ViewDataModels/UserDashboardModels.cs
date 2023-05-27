using BugTracker.Models.EntityModels;

namespace BugTracker.Models.ViewDataModels
{
    public class UserDashboardModels
    {
        public UserModel User { get; set; }
        public List<ProjectModel> UserProjects { get; set; }
        public string? SearchQuery { get; set; }
        public List<ProjectModel> ProjectSearchResults { get; set; }
    }
}
