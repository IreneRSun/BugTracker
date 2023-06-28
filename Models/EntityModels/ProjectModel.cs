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
        public string Name { get; }
		/// <value>
		/// Property <c>CreationDate</c> represents the project's date of creation.
		/// </value>
		public DateTime Date { get; }
        /// <value>
        /// Property <c>Avatar</c> represents the project's avatar image source.
        /// </value>
        public string Avatar { get; }

        /// <summary>
        /// Method <c>ProjectModel</c> initializes this class with a project's ID, name, and creation date.
        /// </summary>
        /// <param name="projectID">The ID of the project. The project's avatar is constructed with this as its seed.</param>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="creationDate">The creation date of the project</param>
        public ProjectModel(string projectID, string projectName, DateTime creationDate)
        {
            ID = projectID;
            Name = projectName;
            Date = creationDate;
            Avatar = Utils.GetSeededAvatar(ID, radius: 0);
        }
    }
}
