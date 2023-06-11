namespace BugTracker.Models.EntityModels
{
    /// <summary>
    /// Class <c>ProjectModel</c> models a project that developers work on.
    /// </summary>
    public class ProjectModel
    {
        /// <value>
        /// Property <c>ID</c> represents the project's id.
        /// </value>
        public string ID { get; }
        /// <value>
        /// Property <c>Name</c> represents the project's name.
        /// </value>
        public string? Name { get; set; }
        /// <value>
        /// Property <c>Avatar</c> represents the project's avatar image.
        /// </value>
        public string Avatar { get; }

        /// <summary>
        /// Method <c>ProjectModel</c> initializes the class with an ID and a name.
        /// </summary>
        /// <param name="projectID">The ID of the project. The project's avatar is constructed with this as its seed.</param>
        public ProjectModel(string projectID)
        {
            ID = projectID;
            Avatar = Utils.GetSeededAvatar(ID, radius: 0);
        }
    }
}
