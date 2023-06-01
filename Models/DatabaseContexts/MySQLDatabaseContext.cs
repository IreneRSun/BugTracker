using BugTracker.Models.EntityModels;
using MySql.Data.MySqlClient;
using System.Data;

namespace BugTracker.Models.DatabaseContexts
{
    /// <summary>
    /// Class <c>DatabaseContext</c> models a database context that manages connections to the database.
    /// </summary>
    public class MySQLDatabaseContext
    {
        /// <value>
        /// Property <c>ConnectionString</c> is the connection string used for connecting to the database.
        /// </value>
        private readonly string _connectionString;

        /// <summary>
        /// Constructor <c>DatabaseContext</c> creates a DatabaseContext initialized with the given connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public MySQLDatabaseContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Method <c>GetConnection</c> gets the connection to the database
        /// </summary>
        /// <returns>The MySqlConnection that represents the connection to the database.</returns>
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
            // connect to database
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            // find an unique hash
            var uniqueHashFound = false;
            string hash = Utils.GenerateHash();
            var query = $"SELECT * FROM {table} WHERE {field} = @hash";
            while (!uniqueHashFound)
            {
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@hash", hash);
                using var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();

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
        /// <param name="query">The query string to use.</param>
        /// <param name="parameters">The parameters of the query string.</param>
        /// <param name="queryParserCallback">The function that parses the database query result.</param>
        /// <returns>The parsed query result.</returns>
        private async Task<T> QueryDatabase<T>(string query, Dictionary<string, string> parameters, Func<MySqlDataReader, Task<T>> queryParserCallback)
        {
            // open connection to database
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            // query database
            using var cmd = new MySqlCommand(query, connection);
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
        /// Method <c>UpdateDatabase</c> executes a non-query on the database.
        /// </summary>
        /// <param name="sqlCmd">The command string to execute.</param>
        /// <param name="parameters">The parameters to use.</param>
        private async Task UpdateDatabase(string sqlCmd, Dictionary<string, string> parameters)
        {
            // open connection to database
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            // execute sql command
            using var cmd = new MySqlCommand(sqlCmd, connection);
            foreach (var parameter in parameters)
            {
                var parameterName = parameter.Key;
                var parameterValue = parameter.Value;
                cmd.Parameters.AddWithValue(parameterName, parameterValue);
            }
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Method <c>UpdateDatabase</c> executes a non-query on the database.
        /// </summary>
        /// <param name="sqlCmd">The command string to execute.</param>
        /// <param name="addParametersFunction">The function that sets the parameters of the command executable.</param>
        private async Task UpdateDatabase(string sqlCmd, Action<MySqlCommand> addParametersFunction)
        {
            // open connection to database
            using MySqlConnection connection = GetConnection();
            await connection.OpenAsync();

            // execute sql command
            using var cmd = new MySqlCommand(sqlCmd, connection);
            addParametersFunction(cmd);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Method <c>AddUserIfNone</c> adds a user if one does not exist in the database (avoids constraint errors).
        /// </summary>
        /// <param name="userId">The ID of the user to add.</param>
        public async Task AddUserIfNone(string userId)
        {
            // build database update command
            var sqlCmd = "INSERT IGNORE INTO users (uid, avatar) VALUES (@uid, NULL)";
            var parameters = new Dictionary<string, string>
            {
                { "@uid", userId }
            };

            // add new user
            await UpdateDatabase(sqlCmd, parameters);
        }

        /// <summary>
        /// Method <c>GetAvatarFromReader</c> converts avatar BLOB data from a MySqlDataReader to a string.
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
        /// Method <c>GetAvatar</c> gets the avatar of the user, if one exists.
        /// </summary>
        /// <param name="userId">The user ID of the user to get the avatar for.</param>
        /// <returns>The avatar of the user or null if the user does not exist(in the database).</returns>
        public async Task<string?> GetAvatar(string userId)
        {
            // build query
            var query = "SELECT * FROM users WHERE uid = @uid";
            var parameters = new Dictionary<string, string> {
                { "@uid", userId }
            };

            // build query result parser
            Func<MySqlDataReader, Task<string?>> queryParser = async (reader) =>
            {
                // convert avatar byte array to string
                await reader.ReadAsync();
                return GetAvatarFromReader(reader);
            };
            
            return await QueryDatabase(query, parameters, queryParser);
        }

        /// <summary>
        /// Method <c>SetAvatar</c> sets the avatar of the user.
        /// </summary>
        /// <param name="userId">The user ID of the user to set the avatar of.</param>
        /// <param name="imageData">The byte array containing the data of the image to set the avatar to.</param>
        public async Task SetAvatar(string userId, byte[] imageData)
        {
            // build database update command
            var sqlCmd = "UPDATE users SET avatar = @imageData WHERE uid = @uid";
            Action<MySqlCommand> addParameters = (cmd) =>
            {
                cmd.Parameters.AddWithValue("@imageData", imageData).DbType = DbType.Binary;  // tells database to treat the byte array as a blob
                cmd.Parameters.AddWithValue("@uid", userId);
            };

            // add new user data
            await UpdateDatabase(sqlCmd, addParameters);
        }

        /// <summary>
        /// Method <c>SearchProjects</c> searches for projects that match the search query (case insensitive).
        /// </summary>
        /// <param name="searchInput">The search query.</param>
        /// <returns>The list of projects matching the search.</returns>
        public async Task<List<ProjectModel>> SearchProjects(string searchInput)
        {
            // build query
            var query = "SELECT * FROM projects WHERE LOWER(name) LIKE LOWER(@search)";
            var parameters = new Dictionary<string, string> {
                { "@search", $"%{searchInput}%" }
            };

            // build query result parser
            Func<MySqlDataReader, Task<List<ProjectModel>>> queryParser = async (reader) =>
            {
                var projects = new List<ProjectModel>();
                while (await reader.ReadAsync())
                {
                    var projectId = reader.GetString("pid");
                    projects.Add(new ProjectModel(projectId)
                    {
                        Name = reader.GetString("name")
                    });
                };
                return projects;
            };

            return await QueryDatabase(query, parameters, queryParser);
        }

        /// <summary>
        /// Method <c>GetProjects</c> gets the projects that the user is a developer of.
        /// </summary>
        /// <param name="userId">The user ID of the user to get the projects of.</param>
        /// <returns>The list of projects that the user is a developer of.</returns>
        public async Task<List<ProjectModel>> GetProjects(string userId)
        {
            // build query
            var query = "SELECT * FROM projects WHERE pid IN (SELECT project FROM developments WHERE developer = @uid)";
            var parameters = new Dictionary<string, string> {
                { "@uid", userId }
            };

            // build query result parser
            Func<MySqlDataReader, Task<List<ProjectModel>>> queryParser = async (reader) =>
            {
                var projects = new List<ProjectModel>();
                while (await reader.ReadAsync())
                {
                    var projectId = reader.GetString("pid");
                    projects.Add(new ProjectModel(projectId)
                    {
                        Name = reader.GetString("name")
                    });
                };
                return projects;
            };

            return await QueryDatabase(query, parameters, queryParser);
        }

        /// <summary>
        /// Method <c>GetProject</c> gets the project associated with the given ID.
        /// </summary>
        /// <param name="projectId">The user ID of project to get.</param>
        /// <returns>The project associated with the given ID.</returns>
        public async Task<ProjectModel> GetProject(string projectId)
        {
            // build query
            var query = "SELECT * FROM projects WHERE pid = @pid";
            var parameters = new Dictionary<string, string> {
                { "@pid", projectId }
            };

            // build query result parser
            Func<MySqlDataReader, Task<ProjectModel>> queryParser = async (reader) =>
            {
                var a = await reader.ReadAsync();
                
                var project = new ProjectModel(projectId)
                {
                    Name = reader.GetString("name")
                };
                return project;
            };

            return await QueryDatabase(query, parameters, queryParser);
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
            var parameters = new Dictionary<string, string>
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
        /// Method <c>DeleteProject</c> either deletes a project if there is only one developer, or deletes a developer from a project
        /// </summary>
        /// <param name="projectId">The ID of the project that is being deleted.</param>
        /// <param name="developerId">The ID of the developer that is deleting the project.</param>
        public async Task DeleteProject(string projectId, string developerId)
        {
            // query database for the number of developers the project has
            var query = "SELECT COUNT(*) AS count FROM developments WHERE project = @pid";
            var queryParameters = new Dictionary<string, string> {
                { "@pid", projectId }
            };

            Func<MySqlDataReader, Task<int>> queryParser = async (reader) =>
            {
                await reader.ReadAsync();
                return reader.GetInt32("count");
            };

            int numDevelopers = await QueryDatabase(query, queryParameters, queryParser);

            // delete project if user is the only developer, otherwise remove developer from the project
            var sqlCmd = numDevelopers > 1 ? "DELETE FROM developments WHERE developer = @uid" : "DELETE FROM projects WHERE pid = @pid";
            var cmdParameters = new Dictionary<string, string>
            {
                { "@uid", developerId },
                { "@pid", projectId }
            };

            await UpdateDatabase(sqlCmd, cmdParameters);
        }

        /// <summary>
        /// Method <c>GetDevelopers</c> gets the developers of a project.
        /// </summary>
        /// <param name="projectId">The ID of the project to get the developers of.</param>
        /// <returns>The list of developers that are part of a project.</returns>
        public async Task<List<UserModel>> GetDevelopers(string projectId)
        {
            // build query
            var query = "SELECT * FROM users WHERE uid IN (SELECT developer FROM developments WHERE project = @pid)";
            var parameters = new Dictionary<string, string> {
                { "@pid", projectId }
            };

            // build query result parser
            Func<MySqlDataReader, Task<List<UserModel>>> queryParser = async (reader) =>
            {
                var developers = new List<UserModel>();
                while (await reader.ReadAsync())
                {
                    var userId = reader.GetString("uid");
                    developers.Add(new UserModel(userId)
                    {
                        Avatar = GetAvatarFromReader(reader)
                    });
                };
                return developers;
            };

            return await QueryDatabase(query, parameters, queryParser);
        }

        /// <summary>
        /// Method <c>IsDeveloper</c> checks if a user is a developer of a project.
        /// </summary>
        /// <param name="userId">The ID of the user to check.</param>
        /// <param name="projectId">The ID of the project to check.</param>
        /// <returns>Whether the user is a developer of the project.</returns>
        public async Task<bool> IsDeveloper(string userId, string projectId)
        {
            // query database for the user
            var query = "SELECT * FROM developments WHERE project = @pid AND developer = @uid";
            var parameters = new Dictionary<string, string> {
                { "@pid", projectId },
                { "@uid", userId }
            };

            Func<MySqlDataReader, Task<bool>> queryParser = async (reader) =>
            {
                return reader.HasRows;
            };

            return await QueryDatabase(query, parameters, queryParser);
        }

        /// <summary>
        /// Method <c>AddDeveloper</c> adds a developer to a project.
        /// </summary>
        /// <param name="projectId">The ID of the project to add a developer to.</param>
        /// <param name="developerId">The ID of the developer to add.</param>
        /// <returns></returns>
        public async Task AddDeveloper(string projectId, string developerId)
        {
            // check if user is already a developer of this project
            if (await IsDeveloper(developerId, projectId))
            {
                return;
            }

            // find a unique hash for the new development relationship
            string developmentId = await FindUniqueHash("developments", "did");

            // build database update command
            var sqlCmd = "INSERT INTO developments (did, project, developer) VALUES (@did, @pid, @uid)";
            var parameters = new Dictionary<string, string>
            {
                { "@did", developmentId },
                { "@pid", projectId },
                { "@uid", developerId }
            };

            // add new developer
            await UpdateDatabase(sqlCmd, parameters);
        }

        /// <summary>
        /// Method <c>GetReports</c> gets the bug reports of a project.
        /// </summary>
        /// <param name="projectId">The ID of the project to get the reports for.</param>
        /// <returns>The list of bug reports for the project.</returns>
        public async Task<List<BugReportModel>> GetReports(string projectId)
        {
            // build query
            var query = "SELECT * FROM bug_reports WHERE project = @pid";
            var parameters = new Dictionary<string, string> {
                { "@pid", projectId }
            };

            // build query result parser
            Func<MySqlDataReader, Task<List<BugReportModel>>> queryParser = async (reader) =>
            {
                var bugs = new List<BugReportModel>();
                while (await reader.ReadAsync())
                {
                    var reportId = reader.GetString("bid");
                    bugs.Add(new BugReportModel(reportId)
                    {
                        ReporterID = reader.GetString("reportee"),
                        ProjectID = reader.GetString("project"),
                        Summary = reader.GetString("summary"),
                        SoftwareVersion = reader.GetDecimal("software_version"),
                        Device = reader.GetString("device"),
                        OS = reader.GetString("os"),
                        ExpectedResult = reader.GetString("expected_result"),
                        ActualResult = reader.GetString("actual_result"),
                        Steps = reader.GetString("steps"),
                        Details = reader.GetString("details"),
                        Priority = reader.GetString("priority"),
                        Severity = reader.GetInt16("severity"),
                        Status = reader.GetString("status"),
                        Date = reader.GetDateTime("date")
                    });
                }
                return bugs;
            };

            return await QueryDatabase(query, parameters, queryParser);
        }

        /// <summary>
        /// Method <c>AddReport</c> creates a bug report.
        /// </summary>
        /// <param name="projectId">The ID of the project that the bug report is for.</param>
        /// <param name="userId">The ID of the user that is reporting the bug.</param>
        /// <param name="reportFields">The bug report fields.</param>
        public async Task AddReport(string projectId, string userId, Dictionary<string, string> reportFields)
        {
            // find a unique hash for the new development relationship
            string reportId = await FindUniqueHash("bug_reports", "bid");

            // build database update command
            var sqlCmd = "INSERT INTO bug_reports " +
                "(bid, reportee, project, summary, software_version, device, os, expected_result, actual_result, steps, details) " +
                "VALUES (@bid, @uid, @pid, @summary, @version, @device, @os, @expected, @actual, @steps, @details)";
            var parameters = new Dictionary<string, string>
            {
                { "@bid", reportId },
                { "@uid", userId },
                { "@pid", projectId },
                { "@summary", reportFields["summary"] },
                { "@version", reportFields["software_version"] },
                { "@device", reportFields["device"] },
                { "@os", reportFields["os"] },
                { "@expected", reportFields["expected_result"] },
                { "@actual", reportFields["actual_result"] },
                { "@steps", reportFields["steps"] },
                { "@details", reportFields["details"] }
            };

            await UpdateDatabase(sqlCmd, parameters);
        }

        /// <summary>
        /// Method <c>AddComment</c> adds a comment to a bug report.
        /// </summary>
        /// <param name="reportId">The ID of the bug report the comment was made on.</param>
        /// <param name="userId">The ID of the user that made the comment.</param>
        /// <param name="comment">The comment the user made.</param>
        /// <param name="replyToID">The ID of the comment this comment is replying to, if any.</param>
        public async Task AddComment(string reportId, string userId, string comment, string? replyToID = null)
        {
            // find a unique hash for the comment
            string commentId = await FindUniqueHash("comments", "cid");

            // add comment data to database
            // build database update command
            var sqlCmd = "INSERT INTO comments (cid, commenter, bug_report, reply_to, comment) VALUES (@cid, @uid, @bid, @rt, @comment)";
            var parameters = new Dictionary<string, string>
            {
                { "@cid", commentId },
                { "@uid", userId },
                { "@bid", reportId },
                { "@rt", replyToID },
                { "@comment", comment }
            };

            await UpdateDatabase(sqlCmd, parameters);
        }

        /// <summary>
        /// Method <c>GetAssignments</c> gets the bugs assigned to the user.
        /// </summary>
        /// <param name="userId">The ID of the user to get the assignments for.</param>
        /// <returns>The list of bug reports that are assigned to the user.</returns>
        public async Task<List<BugReportModel>> GetAssignments(string userId)
        {
            // build query
            var query = "SELECT * FROM bug_reports WHERE bid IN (SELECT bug_report FROM assignments WHERE assignee = @uid)";
            var parameters = new Dictionary<string, string> {
                { "@uid", userId }
            };

            // build query result parser
            Func<MySqlDataReader, Task<List<BugReportModel>>> queryParser = async (reader) =>
            {
                var bugs = new List<BugReportModel>();
                while (await reader.ReadAsync())
                {
                    var reportId = reader.GetString("bid");
                    bugs.Add(new BugReportModel(reportId)
                    {
                        ReporterID = reader.GetString("reportee"),
                        ProjectID = reader.GetString("project"),
                        Summary = reader.GetString("summary"),
                        SoftwareVersion = reader.GetDecimal("software_version"),
                        Device = reader.GetString("device"),
                        OS = reader.GetString("os"),
                        ExpectedResult = reader.GetString("expected_result"),
                        ActualResult = reader.GetString("actual_result"),
                        Steps = reader.GetString("steps"),
                        Details = reader.GetString("details"),
                        Priority = reader.GetString("priority"),
                        Severity = reader.GetInt16("severity"),
                        Status = reader.GetString("status"),
                        Date = reader.GetDateTime("date")
                    });
                }
                return bugs;
            };

            return await QueryDatabase(query, parameters, queryParser);
        }

        /// <summary>
        /// Method <c>AddAssignment</c> assigns a developer to a bug report.
        /// </summary>
        /// <param name="reportId">The ID of the bug report to assign.</param>
        /// <param name="developerId">The ID of the developer to assign the bug to.</param>
        public async Task AddAssignment(string reportId, string developerId)
        {
            // find a unique hash for the assignment
            string assignmentId = await FindUniqueHash("assignments", "aid");

            // build database update command
            var sqlCmd = "INSERT INTO assignments (aid, assignee, bug_report) VALUES (@aid, @uid, @bid)";
            var parameters = new Dictionary<string, string>
            {
                { "@aid", assignmentId },
                { "@uid", developerId },
                { "@bid", reportId }
            };

            await UpdateDatabase(sqlCmd, parameters);
        }

        /// <summary>
        /// Method <c>RemoveAssignment</c> removes an assignment from a bug report.
        /// </summary>
        /// <param name="reportId">The ID of bug report to remove the assignment from.</param>
        /// <param name="developerId">The ID of the developer to remove from the bug report.</param>
        /// <returns></returns>
        public async Task RemoveAssignment(string reportId, string developerId)
        {
            var sqlCmd = "DELETE FROM assignments WHERE assignee = @uid AND bug_report = @bid";
            var parameters = new Dictionary<string, string>
            {
                { "@uid", developerId },
                { "@bid", reportId }
            };

            await UpdateDatabase(sqlCmd, parameters);
        }

        /// <summary>
        /// Method <c>UpdateBugTag</c> updates one of the bug report's tags (i.e. priority, severity, or status)
        /// </summary>
        /// <param name="reportId">The ID of the bug report to update the tag of.</param>
        /// <param name="tagName">The tag to update.</param>
        /// <param name="tagValue">The new value of the tag.</param>
        public async Task UpdateBugTag(string reportId, string tagName, string tagValue)
        {
            var sqlCmd = "UPDATE bug_reports SET @tag_name = @tag_value WHERE bug_report = @bid";
            var parameters = new Dictionary<string, string>
            {
                { "@tag_name", tagName },
                { "@tag_value", tagValue },
                { "@bid", reportId }
            };

            await UpdateDatabase(sqlCmd, parameters);
        }
    }
}
