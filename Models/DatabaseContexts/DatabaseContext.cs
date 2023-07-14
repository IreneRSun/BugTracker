using BugTracker.Models.EntityModels;
using MySql.Data.MySqlClient;
using System.Data;

namespace BugTracker.Models.DatabaseContexts
{
    /// <summary>
    /// Class <c>DatabaseContext</c> models a database context that manages connections to the database.
    /// Assumes the database schema is as defined in the README.
    /// </summary>
    public class DatabaseContext
    {
        /// <value>
        /// Property <c>_connectionString</c> is the connection string used for connecting to the database.
        /// </value>
        private readonly string _connectionString;

        /// <summary>
        /// Constructor <c>DatabaseContext</c> creates a DatabaseContext initialized with the given connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public DatabaseContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Method <c>GetConnection</c> gets the connection to the database using the class instance's connection string.
        /// </summary>
        /// <returns>The connection to the database.</returns>
        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        /// <summary>
        /// Method <c>FindUniqueHash</c> generates an unique sha-256 hash for a table column.
        /// </summary>
        /// <param name="table">The table to find an unique hash for.</param>
        /// <param name="field">The name of the column to find an unique hash for.</param>
        /// <returns>A unique hash for the table column.</returns>
        private async Task<string> FindUniqueHash(string table, string field)
        {
            // open connection to database
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            // find an unique hash
            var uniqueHashFound = false;
            string hash = Utils.GenerateHash();
            var query = $"SELECT * FROM {table} WHERE {field} = @hash";
            while (!uniqueHashFound)
            {
                // query generated hash
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@hash", hash);
                using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();

                // generate a new hash if hash is not unique
                if (reader.HasRows)
                {
                    hash = Utils.GenerateHash();
                }
                else
                {
                    uniqueHashFound = true;
                }
            }

            // close the connection
            await connection.CloseAsync();

            return hash;
        }

        /// <summary>
        /// Method <c>QueryDatabase</c> queries the database and parses the resulting query.
        /// </summary>
        /// <typeparam name="T">The type of the parsed query result.</typeparam>
        /// <param name="queryString">The query string to use.</param>
        /// <param name="parameters">The parameters of the query string.</param>
        /// <param name="queryParserCallback">The function that parses the database query result.</param>
        /// <returns>The parsed query result.</returns>
        private async Task<T> QueryDatabase<T>(string queryString, Dictionary<string, object?> parameters, Func<MySqlDataReader, Task<T>> queryParserCallback)
        {
            // open connection to database
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            // query database
            using var cmd = new MySqlCommand(queryString, connection);
            foreach (var parameter in parameters)
            {
                var parameterName = parameter.Key;
                var parameterValue = parameter.Value;
                cmd.Parameters.AddWithValue(parameterName, parameterValue);
            }
            using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();

            // parse query results
            T result = await queryParserCallback(reader);

            // close the connection
            await reader.CloseAsync();
            await connection.CloseAsync();

            return result;
        }

        /// <summary>
        /// Method <c>UpdateDatabase</c> executes a non-query command on the database.
        /// </summary>
        /// <param name="cmdString">The command string to execute.</param>
        /// <param name="parameters">The parameters of the command string.</param>
        private async Task UpdateDatabase(string cmdString, Dictionary<string, object?> parameters)
        {
            // open connection to database
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            // execute sql command
            using var cmd = new MySqlCommand(cmdString, connection);
            foreach (var parameter in parameters)
            {
                var parameterName = parameter.Key;
                var parameterValue = parameter.Value;
                cmd.Parameters.AddWithValue(parameterName, parameterValue);
            }
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Method <c>UpdateDatabase</c> executes a non-query command on the database.
        /// </summary>
        /// <param name="cmdString">The command string to execute.</param>
        /// <param name="addParametersFunction">The function that sets the parameters of the command executable.</param>
        private async Task UpdateDatabase(string cmdString, Action<MySqlCommand> addParametersFunction)
        {
            // open connection to database
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            // execute sql command
            using var cmd = new MySqlCommand(cmdString, connection);
            addParametersFunction(cmd);
            await cmd.ExecuteNonQueryAsync();
        }

		/// <summary>
		/// Method <c>GetAvatarFromReader</c> converts avatar BLOB data from a MySqlDataReader to a string.
		/// Returns null is no avatar data found.
		/// </summary>
		/// <param name="reader">The MySqlDataReader that contains the avatar data.</param>
		/// <returns>The converted string avatar from the reader.</returns>
		private static string? GetAvatarFromReader(MySqlDataReader reader)
		{
			if (!reader.IsDBNull("avatar"))
			{
				var avatarData = reader.GetFieldValue<byte[]>("avatar");
				return "data:image/png;base64," + Convert.ToBase64String(avatarData);
			}

			return null;
		}

		/// <summary>
		/// Method<c>ParseUsers</c> parses the user data from a MySqlDataReader into a UserModel class.
		/// Assumes the reader contains only data of rows from the users table.
		/// </summary>
		/// <param name="reader">The MySqlDataReader that contains the users' data.</param>
		/// <returns>The list of users and their data read from the reader.</returns>
		private static async Task<List<UserModel>> ParseUsers(MySqlDataReader reader)
        {
            var users = new List<UserModel>();
            while (await reader.ReadAsync())
            {
                var userId = reader.GetString("uid");
				users.Add(new UserModel(userId)
				{
					Avatar = GetAvatarFromReader(reader),
                    Status = reader.IsDBNull("status") ? null : reader.GetString("status")
				});
			}
            return users;
		}

		/// <summary>
		/// Method<c>ParseProjects</c> parses the user data from a MySqlDataReader into a ProjectModel class.
        /// Assumes the reader contains only data of rows from the projects table.
		/// </summary>
		/// <param name="reader">The MySqlDataReader that contains the projects' data.</param>
		/// <returns>The list of projects and their data read from the reader.</returns>
		private static async Task<List<ProjectModel>> ParseProjects(MySqlDataReader reader)
        {
			var projects = new List<ProjectModel>();
			while (await reader.ReadAsync())
			{
				var projectId = reader.GetString("pid");
                var projectName = reader.GetString("name");
                var creationDate = reader.GetDateTime("date");
				projects.Add(new ProjectModel(projectId, projectName, creationDate));
			};
			return projects;
		}

		/// <summary>
		/// Method<c>ParseReports</c> parses the report data from a MySqlDataReader into a BugReportModel class.
		/// Assumes the reader contains only data of rows from the reports table.
		/// </summary>
		/// <param name="reader">The MySqlDataReader that contains the reports' data.</param>
		/// <returns>The list of reports and their data read from the reader.</returns>
		private async Task<List<BugReportModel>> ParseReports(MySqlDataReader reader)
        {
			var bugs = new List<BugReportModel>();
			while (await reader.ReadAsync())
			{
				var reportId = reader.GetString("bid");
                var reporterId = reader.IsDBNull("reportee") ? null : reader.GetString("reportee");
                var projectId = reader.GetString("project");
                var summary = reader.GetString("summary");
                var sver = reader.GetDecimal("software_version");
                var device = reader.IsDBNull("device") ? null : reader.GetString("device");
                var os = reader.GetString("os");
                var details = reader.GetString("details");
                var priority = reader.IsDBNull("priority") ? null : reader.GetString("priority");
                var severity = reader.IsDBNull("severity") ? null : reader.GetString("severity");
                var status = reader.IsDBNull("status") ? null : reader.GetString("status");
                var helpWanted = reader.GetBoolean("help_wanted");
                var date = reader.GetDateTime("date");
                int upvotes = await GetUpvotes(reportId);
				bugs.Add(new BugReportModel(reportId, reporterId, projectId, 
                    summary, sver, device, os, details, 
                    priority, severity, status, helpWanted, date, upvotes));
			}
			return bugs;
		}

		/// <summary>
		/// Method<c>ParseComments</c> parses the comment data from a MySqlDataReader into a CommentModel class.
		/// Assumes the reader contains only data of rows from the comments table.
		/// </summary>
		/// <param name="reader">The MySqlDataReader that contains the comments' data.</param>
		/// <returns>The list of comments and their data read from the reader.</returns>
		private async Task<List<CommentModel>> ParseComments(MySqlDataReader reader)
        {
			var comments = new List<CommentModel>();
			while (await reader.ReadAsync())
			{
				var commentId = reader.GetString("cid");

				var commenter = reader.IsDBNull("commenter") ? null : new UserModel(reader.GetString("commenter"));
                if (commenter != null)
                {
					await GetUserData(commenter);
				}

                var reportId = reader.GetString("report");
                var comment = reader.GetString("comment");
                var date = reader.GetDateTime("date");

				comments.Add(new CommentModel(commentId, commenter, reportId, comment, date));
			}
			return comments;
		}

        /// <summary>
        /// Method <c>ParseCount</c> parses the count data from a MySqlDataReader into an integer.
        /// Requires the name or alias of the column with the count data to be "count".
        /// </summary>
        /// <param name="reader">The MySqlDataReader that contains the count data.</param>
        /// <returns>The count.</returns>
        private async Task<int> ParseCount(MySqlDataReader reader)
        {
            await reader.ReadAsync();
            return reader.GetInt32("count");
        }

        /// <summary>
        /// Method <c>ParseHasRows</c> returns whether a MySqlDataReader has one or more rows as a boolean.
        /// </summary>
        /// <param name="reader">The MySqlDataReader that contains the row data.</param>
        /// <returns>Whether the reader contains one or more rows.</returns>
        private async Task<bool> ParseHasRows(MySqlDataReader reader)
        {
            return reader.HasRows;
        }

        /// <summary>
        /// Method <c>RegisterUser</c> adds a user if they do not exist in the database.
        /// </summary>
        /// <param name="userId">The ID of the user to add.</param>
        public async Task RegisterUser(string userId)
        {
            var cmd = "INSERT IGNORE INTO users (uid, avatar, status) VALUES (@uid, NULL, NULL)";
            var parameters = new Dictionary<string, object?>
            {
                { "@uid", userId }
            };
            await UpdateDatabase(cmd, parameters);
        }

		/// <summary>
		/// Method <c>DeleteUser</c> deletes a user from the database.
		/// </summary>
		/// <param name="userId">The ID of the user to delete.</param>
		public async Task DeleteUser(string userId)
        {
			var cmd = "DELETE FROM users WHERE uid = @uid";
			var cmdParameters = new Dictionary<string, object?>
			{
				{ "@uid", userId }
			};
			await UpdateDatabase(cmd, cmdParameters);
		}

		/// <summary>
		/// Method <c>GetUserData</c> fills a UserModel with corresponding user data from the database, if any found.
		/// </summary>
		/// <param name="user">The UserModel to fill with the user's data.
        /// User data retrieved corresponds to the ID attribute of the class.</param>
		public async Task GetUserData(UserModel user)
        {
			var query = "SELECT * FROM users WHERE uid = @uid";
			var parameters = new Dictionary<string, object?> {
				{ "@uid", user.ID }
			};
            List<UserModel> matches = await QueryDatabase(query, parameters, ParseUsers);

            if (matches.Count > 0)
            {
				UserModel userData = matches[0];

				if (userData.Avatar != null)
				{
					user.Avatar = userData.Avatar;
				}
				user.Status = userData.Status;
			}
		}

        /// <summary>
        /// Method <c>SetAvatar</c> sets the avatar of the user.
        /// </summary>
        /// <param name="userId">The user ID of the user to set the avatar of.</param>
        /// <param name="imageData">The byte array containing the data of the image to set the avatar to.</param>
        public async Task SetAvatar(string userId, byte[] imageData)
        {
            var cmdString = "UPDATE users SET avatar = @imageData WHERE uid = @uid";
            Action<MySqlCommand> addParameters = (cmd) =>
            {
                cmd.Parameters.AddWithValue("@imageData", imageData).DbType = DbType.Binary;  // tells database to treat the byte array as a blob
                cmd.Parameters.AddWithValue("@uid", userId);
            };
            await UpdateDatabase(cmdString, addParameters);
        }

        /// <summary>
        /// Method <c>SetUserStatus</c> sets the status of a user.
        /// </summary>
        /// <param name="userId">The ID of the user to set the status of.</param>
        /// <param name="status">The status to set the user to.</param>
        public async Task SetUserStatus(string userId, string status)
        {
			var cmd = "UPDATE users SET status = @status WHERE uid = @uid";
			var parameters = new Dictionary<string, object?>
			{
				{ "@uid", userId },
				{ "@status", status }
			};
			await UpdateDatabase(cmd, parameters);
		}

        /// <summary>
        /// Method <c>SearchProjects</c> searches for projects that match the search query (case insensitive).
        /// </summary>
        /// <param name="searchInput">The search query.</param>
        /// <returns>The list of projects matching the search.</returns>
        public async Task<List<ProjectModel>> SearchProjects(string searchInput)
        {
            var query = "SELECT * FROM projects WHERE LOWER(name) LIKE LOWER(@search)";
            var parameters = new Dictionary<string, object?> {
                { "@search", $"%{searchInput}%" }
            };
            return await QueryDatabase(query, parameters, ParseProjects);
        }

        /// <summary>
        /// Method <c>GetProjects</c> gets the projects that the user is a developer of.
        /// </summary>
        /// <param name="userId">The user ID of the user to get the projects of.</param>
        /// <returns>The list of projects that the user is a developer of.</returns>
        public async Task<List<ProjectModel>> GetProjects(string userId)
        {
            var query = "SELECT * FROM projects WHERE pid IN (SELECT project FROM developments WHERE developer = @uid)";
            var parameters = new Dictionary<string, object?> {
                { "@uid", userId }
            };
            return await QueryDatabase(query, parameters, ParseProjects);
        }

        /// <summary>
        /// Method <c>GetProject</c> gets the project associated with the given ID.
        /// Returns null if none found.
        /// </summary>
        /// <param name="projectId">The user ID of project to get.</param>
        /// <returns>The project associated with the given ID.</returns>
        public async Task<ProjectModel?> GetProject(string projectId)
        {
            var query = "SELECT * FROM projects WHERE pid = @pid";
            var parameters = new Dictionary<string, object?> {
                { "@pid", projectId }
            };
            List<ProjectModel> matches = await QueryDatabase(query, parameters, ParseProjects);

            return matches.Count > 0 ? matches[0] : null;
        }

        /// <summary>
        /// Method <c>AddProject</c> creates a project.
        /// </summary>
        /// <param name="projectName">The name to create the project with.</param>
        /// <param name="userId">The user ID of the user that created the project.</param>
        public async Task AddProject(string projectName, string userId)
        {
            // find a unique hash for the new project
            string projectId = await FindUniqueHash("projects", "pid");

            // build database update command
            var sqlCmd = "INSERT INTO projects (pid, name) VALUES (@pid, @pname)";
            var parameters = new Dictionary<string, object?>
            {
                { "@pid", projectId },
                { "@pname", projectName }
            };

            // add new project
            await UpdateDatabase(sqlCmd, parameters);

            // assign user to project in database
            await AddDeveloper(projectId, userId);
        }

        /// <summary>
        /// Method <c>DeleteProject</c> either deletes a project if there is only one developer, or deletes a developer from a project.
        /// </summary>
        /// <param name="projectId">The ID of the project that is being deleted.</param>
        /// <param name="developerId">The ID of the developer that is deleting the project.</param>
        public async Task DeleteProject(string projectId, string developerId)
        {
            // query database for the number of developers the project has
            var query = "SELECT COUNT(*) AS count FROM developments WHERE project = @pid";
            var queryParameters = new Dictionary<string, object?> {
                { "@pid", projectId }
            };
            int numDevelopers = await QueryDatabase(query, queryParameters, ParseCount);

            // delete project if user is the only developer, otherwise remove developer from the project
            var cmd = numDevelopers > 1 ? "DELETE FROM developments WHERE developer = @uid" : "DELETE FROM projects WHERE pid = @pid";
            var cmdParameters = new Dictionary<string, object?>
            {
                { "@uid", developerId },
                { "@pid", projectId }
            };

            await UpdateDatabase(cmd, cmdParameters);
        }

        /// <summary>
        /// Method <c>GetDevelopers</c> gets the developers of a project.
        /// </summary>
        /// <param name="projectId">The ID of the project to get the developers of.</param>
        /// <returns>The list of developers that are part of a project.</returns>
        public async Task<List<UserModel>> GetDevelopers(string projectId)
        {
            var query = "SELECT * FROM users WHERE uid IN (SELECT developer FROM developments WHERE project = @pid)";
            var parameters = new Dictionary<string, object?> {
                { "@pid", projectId }
            };
            return await QueryDatabase(query, parameters, ParseUsers);
        }

        /// <summary>
        /// Method <c>IsDeveloper</c> checks if a user is a developer of a project.
        /// </summary>
        /// <param name="userId">The ID of the user to check.</param>
        /// <param name="projectId">The ID of the project to check.</param>
        /// <returns>Whether the user is a developer of the project.</returns>
        public async Task<bool> IsDeveloper(string userId, string projectId)
        {
            var query = "SELECT * FROM developments WHERE project = @pid AND developer = @uid";
            var parameters = new Dictionary<string, object?> {
                { "@pid", projectId },
                { "@uid", userId }
            };
            return await QueryDatabase(query, parameters, ParseHasRows);
        }

        /// <summary>
        /// Method <c>AddDeveloper</c> adds a developer to a project.
        /// </summary>
        /// <param name="projectId">The ID of the project to add a developer to.</param>
        /// <param name="developerId">The ID of the developer to add.</param>
        public async Task AddDeveloper(string projectId, string developerId)
        {
            // check if user is already a developer of this project
            if (await IsDeveloper(developerId, projectId))
            {
                return;
            }

            // otherwise add developer
            var sqlCmd = "INSERT INTO developments (project, developer) VALUES (@pid, @uid)";
            var parameters = new Dictionary<string, object?>
            {
                { "@pid", projectId },
                { "@uid", developerId }
            };
            await UpdateDatabase(sqlCmd, parameters);
        }

        /// <summary>
        /// Method <c>GetReports</c> gets the bug reports of a project.
        /// </summary>
        /// <param name="projectId">The ID of the project to get the reports for.</param>
        /// <param name="filter">The status to filter for.</param>
        /// <param name="sortType">The column to sort by.</param>
        /// <param name="sortOrder">The order of the sort.</param>
        /// <returns>The list of bug reports for the project.</returns>
        public async Task<List<BugReportModel>> GetReports(string projectId, string filter, string sortType, string sortOrder)
        {
            // build query
            var query = "SELECT * FROM reports WHERE project = @pid";
            var parameters = new Dictionary<string, object?> {
                { "@pid", projectId }
            };

            if (!(filter.ToLower() == "all"))
            {
                query += " AND status = @status";
                parameters.Add("@status", filter);
            }

            query += $" ORDER BY {sortType.ToLower()}";

            if (sortOrder.ToLower() == "descending")
            {
                query += $" DESC";
            }

            return await QueryDatabase(query, parameters, ParseReports);
        }

		/// <summary>
		/// Method <c>GetReport</c> gets a bug report.
        /// Returns null if none found.
		/// </summary>
		/// <param name="reportId">The ID of the report to get.</param>
		/// <returns>The bug report.</returns>
		public async Task<BugReportModel?> GetReport(string reportId)
		{
			var query = "SELECT * FROM reports WHERE bid = @bid";
			var parameters = new Dictionary<string, object?> {
				{ "@bid", reportId }
			};
            List<BugReportModel> matches = await QueryDatabase(query, parameters, ParseReports);
			
			return matches.Count > 0 ? matches[0] : null;
		}

		/// <summary>
		/// Method <c>AddReport</c> creates a bug report.
		/// </summary>
		/// <param name="projectId">The ID of the project that the bug report is for.</param>
		/// <param name="userId">The ID of the user that is reporting the bug.</param>
		/// <param name="summary">The summary of the report.</param>
		/// <param name="softwareVersion">The software version of the project on which the bug occurred.</param>
		/// <param name="device">The device used when the bug occurred.</param>
		/// <param name="os">The operating system used when the bug occurred.</param>
		/// <param name="details">The details regarding the bug.</param>
		/// <returns></returns>
		public async Task AddReport(string projectId, string userId, 
            string summary, decimal softwareVersion, string? device, string os, string details)
        {
            string reportId = await FindUniqueHash("reports", "bid");

            var cmd = "INSERT INTO reports " +
                "(bid, reportee, project, summary, software_version, device, os, details) " +
                "VALUES (@bid, @uid, @pid, @summary, @version, @device, @os, @details)";
            var parameters = new Dictionary<string, object?>
            {
                { "@bid", reportId },
                { "@uid", userId },
                { "@pid", projectId },
                { "@summary", summary },
                { "@version", softwareVersion },
                { "@device", device },
                { "@os", os },
                { "@details", details }
            };

            await UpdateDatabase(cmd, parameters);
        }

		/// <summary>
		/// Method <c>GetComments</c> gets the comments for a bug report.
		/// </summary>
		/// <param name="reportId">The ID of the report to get the comments of.</param>
		/// <returns>The list of comments for a bug report (ordered by date, descending).</returns>
		public async Task<List<CommentModel>> GetComments(string reportId)
		{
			// build query
			var query = "SELECT * FROM comments WHERE report = @bid ORDER BY date DESC";
			var parameters = new Dictionary<string, object?> {
				{ "@bid", reportId }
			};
			return await QueryDatabase(query, parameters, ParseComments);
		}

		/// <summary>
		/// Method <c>AddComment</c> adds a comment to a bug report.
		/// </summary>
		/// <param name="reportId">The ID of the bug report the comment was made on.</param>
		/// <param name="userId">The ID of the user that made the comment.</param>
		/// <param name="comment">The comment the user made.</param>
		public async Task AddComment(string reportId, string userId, string comment)
        {
            string commentId = await FindUniqueHash("comments", "cid");

            var cmd = "INSERT INTO comments (cid, commenter, report, comment) VALUES (@cid, @uid, @bid, @comment)";
            var parameters = new Dictionary<string, object?>
            {
                { "@cid", commentId },
                { "@uid", userId },
                { "@bid", reportId },
                { "@comment", comment }
            };

            await UpdateDatabase(cmd, parameters);
        }

        /// <summary>
        /// Method <c>GetAssignments</c> gets the bugs assigned to the user.
        /// </summary>
        /// <param name="userId">The ID of the user to get the assignments for.</param>
        /// <returns>The list of bug reports that are assigned to the user.</returns>
        public async Task<List<BugReportModel>> GetAssignments(string userId)
        {
            var query = "SELECT * FROM reports WHERE bid IN (SELECT report FROM assignments WHERE assignee = @uid)";
            var parameters = new Dictionary<string, object?> {
                { "@uid", userId }
            };
            return await QueryDatabase(query, parameters, ParseReports);
        }

		/// <summary>
		/// Method <c>GetAssignees</c> gets the users assigned to a bug.
		/// </summary>
		/// <param name="reportId">The ID of the bug report to get the assigees for.</param>
		/// <returns>The list of assignees for a bug report.</returns>
		public async Task<List<UserModel>> GetAssignees(string reportId)
		{
			var query = "SELECT * FROM users WHERE uid IN (SELECT assignee FROM assignments WHERE report = @bid)";
			var parameters = new Dictionary<string, object?> {
				{ "@bid", reportId }
			};
			return await QueryDatabase(query, parameters, ParseUsers);
		}

		/// <summary>
		/// Method <c>AddAssignment</c> assigns a developer to a bug report.
		/// </summary>
		/// <param name="reportId">The ID of the bug report to assign.</param>
		/// <param name="developerId">The ID of the developer to assign the bug to.</param>
		public async Task AddAssignment(string reportId, string developerId)
        {
            var cmd = "INSERT INTO assignments (assignee, report) VALUES (@uid, @bid)";
            var parameters = new Dictionary<string, object?>
            {
                { "@uid", developerId },
                { "@bid", reportId }
            };
            await UpdateDatabase(cmd, parameters);
        }

        /// <summary>
        /// Method <c>DeleteAssignment</c> removes an assignment from a bug report.
        /// </summary>
        /// <param name="reportId">The ID of bug report to remove the assignment from.</param>
        /// <param name="developerId">The ID of the developer to remove from the bug report.</param>
        public async Task DeleteAssignment(string reportId, string developerId)
        {
            var cmd = "DELETE FROM assignments WHERE assignee = @uid AND report = @bid";
            var parameters = new Dictionary<string, object?>
            {
                { "@uid", developerId },
                { "@bid", reportId }
            };
            await UpdateDatabase(cmd, parameters);
        }

        /// <summary>
        /// Method <c>UpdateBugTag</c> updates one of the bug report's tags (i.e. priority, severity, status, or help_wanted)
        /// </summary>
        /// <param name="reportId">The ID of the bug report to update the tag of.</param>
        /// <param name="tagName">The tag to update.</param>
        /// <param name="tagValue">The new value of the tag.</param>
        public async Task UpdateBugTag(string reportId, string tagName, string tagValue)
        {
            var cmd = $"UPDATE reports SET {tagName} = @tag_value WHERE bid = @bid";
            var parameters = new Dictionary<string, object?>
            {
                { "@tag_value", tagValue },
                { "@bid", reportId }
            };
            await UpdateDatabase(cmd, parameters);
        }

        /// <summary>
        /// Method <c>AddHelpWanted</c> sets the "Help Wanted" tag of a bug report to true.
        /// </summary>
        /// <param name="reportId">The ID of the report to set the tag of.</param>
        public async Task AddHelpWanted(string reportId)
        {
			var cmd = $"UPDATE reports SET help_wanted = true WHERE bid = @bid";
			var parameters = new Dictionary<string, object?>
			{
				{ "@bid", reportId }
			};
			await UpdateDatabase(cmd, parameters);
		}

        /// <summary>
        /// Method <c>RemoveHelpWanted</c> sets the "Help Wanted tag of a bug report to false.
        /// </summary>
        /// <param name="reportId">The ID of the report to set the tag of.</param>
        public async Task RemoveHelpWanted(string reportId)
        {
			var cmd = $"UPDATE reports SET help_wanted = false WHERE bid = @bid";
			var parameters = new Dictionary<string, object?>
			{
				{ "@bid", reportId }
			};
			await UpdateDatabase(cmd, parameters);
		}

        /// <summary>
        /// Method <c>GetProjectStatisitic</c> gets a statistic for a project as a number of bug reports that are part of the statistic.
        /// </summary>
        /// <param name="projectId">The ID of the project to get the statistic of.</param>
        /// <param name="statisticType">The type of statistic to retrieve (i.e. fixed, pending, or new).</param>
        /// <returns>The statistic retrieved.</returns>
        public async Task<int> GetProjectStatistic(string projectId, string statisticType)
        {
			var query = "SELECT COUNT(*) AS count FROM reports WHERE project = @pid AND status = @status";
            var parameters = new Dictionary<string, object?>
            {
                { "@pid", projectId },
                { "@status", "" }
            };

            // set query parameter based on desired statistic type
			var type = statisticType.ToLower();
            if (type == "fixed")
            {
                parameters["@status"] = "Closed";
            } else if (type == "pending")
            {
				parameters["@status"] = "Open";
			} else if (type == "new")
            {
				parameters["@status"] = "NEW";
			}
			return await QueryDatabase(query, parameters, ParseCount);
		}

        /// <summary>
        /// Method <c>GetUpvotes</c> gets the number of upvotes a report has.
        /// </summary>
        /// <param name="reportId">The ID of the report to get the number of upvotes of.</param>
        /// <returns>The number of upvotes.</returns>
        public async Task<int> GetUpvotes(string reportId)
        {
			var query = "SELECT COUNT(*) AS count FROM upvotes WHERE report = @bid";
			var parameters = new Dictionary<string, object?>
			{
				{ "@bid", reportId }
			};
			return await QueryDatabase(query, parameters, ParseCount);
		}

        /// <summary>
        /// Method <c>IsUserUpvoted</c> checks whether an user upvoted a report.
        /// </summary>
        /// <param name="reportId">The ID of the report to check the upvotes of.</param>
        /// <param name="userId">The ID of the user to check the upvotes of.</param>
        /// <returns>Whether the user upvoted the report.</returns>
        public async Task<bool> IsUserUpvoted(string reportId, string userId)
        {
			var query = "SELECT * FROM upvotes WHERE user = @uid AND report = @bid";
			var parameters = new Dictionary<string, object?> {
				{ "@bid", reportId },
				{ "@uid", userId }
			};
			return await QueryDatabase(query, parameters, ParseHasRows);
		}

        /// <summary>
        /// Method <c>AddUpvote</c> adds an upvote given by a user to a report.
        /// </summary>
        /// <param name="reportId">The ID of the report to add an upvote to.</param>
        /// <param name="userId">The ID of the user who made the upvote.</param>
        public async Task AddUpvote(string reportId, string userId)
        {
			var cmd = "INSERT INTO upvotes (user, report) VALUES (@uid, @bid)";
			var parameters = new Dictionary<string, object?>
			{
				{ "@bid", reportId },
				{ "@uid", userId }
			};
			await UpdateDatabase(cmd, parameters);
		}

		/// <summary>
		/// Method <c>DeleteUpvote</c> removes an upvote from a report.
		/// </summary>
		/// <param name="reportId">The ID of the report to remove an upvote from.</param>
		/// <param name="userId">The ID of the user to remove the upvote for.</param>
		public async Task DeleteUpvote(string reportId, string userId)
        {
			var cmd = "DELETE FROM upvotes WHERE user = @uid AND report = @bid";
			var parameters = new Dictionary<string, object?>
			{
				{ "@bid", reportId },
				{ "@uid", userId }
			};
			await UpdateDatabase(cmd, parameters);
		}
    }
}
