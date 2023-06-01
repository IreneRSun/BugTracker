using BugTracker.Models.EntityModels;

namespace BugTracker.Models.DatabaseContexts
{
    /// <summary>
    /// Class <c>DatabaseContext</c> is a wrapper class for all database contexts.
    /// </summary>
    public class DatabaseContext
    {
        public MySQLDatabaseContext SqlDb { get; }
        public Auth0ManagementContext AuthDb { get; }
        public DatabaseContext(MySQLDatabaseContext sqlDbCx, Auth0ManagementContext authDbCx) {
            SqlDb = sqlDbCx;
            AuthDb = authDbCx;
        }

        /// <summary>
        /// Method <c>GetUsers</c> gets the UserModel representing a user.
        /// </summary>
        /// <param name="userId">The ID of the user to get a UserModel for.</param>
        /// <returns>The UserModel of the user.</returns>
        public async Task<UserModel> GetUser(string userId)
        {
            UserModel user = await AuthDb.getUser(userId);
            string? avatar = await SqlDb.GetAvatar(userId);
            if (avatar != null)
            {
                user.Avatar = avatar;
            }

            return user;
        }
    }
}
