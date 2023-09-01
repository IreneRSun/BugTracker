using BugTracker.Models.EntityModels;

namespace BugTracker.Models.DatabaseContexts
{
    /// <summary>
    /// Class <c>UserManagementContext</c> models an API context that manages connections to the database handling user data.
    /// </summary>
    public abstract class UserManagementContext
    {
        /// <summary>
        /// Method <c>GetUser</c> gets a UserModel with user data from the user database.
        /// </summary>
        /// <param name="userId">The ID of the user to get the data of.</param>
        /// <returns>The UserModel with the user data.</returns>
        public abstract Task<UserModel> GetUser(string userId);

        /// <summary>
        /// Method <c>GetUserData</c> fills a UserModel with available user data from the user database.
        /// </summary>
        /// <param name="user">The UserModel to fill with the user's data.
        /// User data retrieved corresponds to the ID attribute of the class.</param>
        public abstract Task GetUserData(UserModel user);

        /// <summary>
        /// Method <c>UpdateUsername</c> updates the username of the user in the user database.
        /// </summary>
        /// <param name="userId">The ID of the user to update the name of.</param>
        /// <param name="newName">The new name to update to.</param>
        public abstract Task UpdateUsername(string userId, string newName);

        /// <summary>
        /// Method <c>DeleteUser</c> deletes a user from the user database.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        public abstract Task DeleteUser(string userId);

        /// <summary>
        /// Method <c>SearchUsers</c> searches for users whose usernames match the query from within the user database (case-insensitive).
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>The list of users satisfying the search criteria.</returns>
        public abstract List<UserModel> SearchUsers(string searchQuery);
    }
}
