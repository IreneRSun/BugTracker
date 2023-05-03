namespace BugTracker.Models
{
    public class Project
    {
        private DatabaseContext context;
        
        public string ProjectId { get; set; }

        public string ProjectName { get; set; }
    }
}
