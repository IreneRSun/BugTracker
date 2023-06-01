using BugTracker.Models.EntityModels;

namespace BugTracker.Models.ViewDataModels
{
    public class ProjectViewModel
    {
        public ProjectModel Project { get; set; }
        public List<UserModel> Developers { get; set; }
        public bool IsDeveloper { get; set; }
        public string? SearchQuery { get; set; }
        public List<UserModel> UserSearchResults { get; set; }
    }
}
