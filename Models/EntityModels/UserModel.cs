namespace BugTracker.Models.EntityModels
{
    /// <summary>
    /// Class <c>UserModel</c> models a user that uses this bug tracker.
    /// </summary>
    public class UserModel
    {
        /// <value>
        /// Property <c>ID</c> represents the user's id.
        /// </value>
        public string ID { get; }
        /// <value>
        /// Property <c>EmailAddress</c> represents the user's email.
        /// </value>
        public string? Email { get; set; }
        /// <value>
        /// Property <c>Name</c> represents the user's username in the context of the bug tracker.
        /// </value>
        public string? Name { get; set; }
        /// <value>
        /// Property <c>Avatar</c> represents the user's profile picture.
        /// </value>
        public string? Avatar { get; set; }
		/// <value>
		/// Property <c>Avatar</c> represents the user's status.
		/// </value>
		public string? Status { get; set; }

        /// <summary>
        /// Method <c>UserModel</c> initalizes the class with a user's ID.
        /// </summary>
        /// <param name="userID">The ID of the user.</param>
        public UserModel(string userID)
        {
            ID = userID;
        }
    }
}
