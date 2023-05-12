namespace BugTracker.Models
{
    public class UserModel
    {
        public string UserId { get; set; }
        public string EmailAddress { get; set; }

        public string UserName { get; set; }

        // replace this, get from Dicebears instead
        public string ProfileImage { get; set; }
    }
}
