using System.Reflection.Metadata;

namespace BugTracker.Models
{
    public class UserModel
    {
        public string UserId { get; set; }

        public string Username { get; set; }

        // replace this, get from Dicebears instead
        public string ProfileImage { get; set; }
        // remove this
        public string EmailAddress { get; set; }
    }
}
