using BugTracker.Models.EntityModels;

namespace BugTracker.Models.ViewDataModels
{
    public class ProjectViewModel
    {
        public ProjectModel Project { get; }
        public int NewBugs { get; }
        public int PendingBugs { get; }
        public int FixedBugs { get; }
        public List<UserModel> Developers { get; }
        public bool IsDeveloper { get; }

        public ProjectViewModel(ProjectModel project, int newBugs, int pendingBugs, int fixedBugs, List<UserModel> developers, bool isDeveloper)
        {
            Project = project;
            NewBugs = newBugs;
            PendingBugs = pendingBugs;
            FixedBugs = fixedBugs;
            Developers = developers;
            IsDeveloper = isDeveloper;
        } 
    }
}
