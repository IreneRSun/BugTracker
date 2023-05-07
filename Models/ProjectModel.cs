namespace BugTracker.Models
{
    public class ProjectModel
    {
        private DatabaseContext context;
        
        public string ProjectId { get; set; }

        public string ProjectName { get; set; }
    }
}
