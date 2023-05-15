namespace BugTracker.Models
{
    /// <summary>
    /// Class <c>UserModel</c> models a user that uses this bug tracker.
    /// </summary>
    public class UserModel
    {
        /// <value>
        /// Property <c>UserId</c> represents the user's id.
        /// </value>
        public string UserId { get; set; }
        /// <value>
        /// Property <c>EmailAddress</c> represents the user's email.
        /// </value>
        public string EmailAddress { get; set; }
        /// <value>
        /// Property <c>UserName</c> represents the user's username in the context of the bug tracker.
        /// </value>
        public string UserName { get; set; }
        /// <value>
        /// Property <c>Avatar</c> represents the user's profile picture.
        /// </value>
        public string Avatar { get; set; }

    }
}
