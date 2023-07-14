using BugTracker.Models.EntityModels;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models.ViewDataModels
{
    public class DashboardViewModel
    {
        public UserModel User { get; }
        public bool IsCurrentUser { get; }
        public List<ProjectModel> UserProjects { get; }

        public DashboardViewModel(UserModel user, bool isCurrentUser, List<ProjectModel> project) {
            User = user;
            IsCurrentUser = isCurrentUser;
            UserProjects = project;
        }
    }
}
