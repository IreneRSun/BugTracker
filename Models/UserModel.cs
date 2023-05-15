namespace BugTracker.Models
{
    public class UserModel
    {
        private DatabaseContext context;
        public string UserId { get; set; }
        public string EmailAddress { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
    }
}
