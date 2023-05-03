using System.Reflection.Metadata;

namespace BugTracker.Models
{
    public class User
    {
        private DatabaseContext context;

        public string UserId { get; set; }

        public string Username { get; set; }

        public Blob profilePhoto { get; set; }
    }
}
