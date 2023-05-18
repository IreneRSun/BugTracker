using MySql.Data.MySqlClient;
using System.Data;

namespace BugTracker.Models
{
    public class DatabaseContext
    {
        public string ConnectionString { get; set; }

        public DatabaseContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        private string FindUniqueHash(string table, string field)
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // find unique hash
            var uniqueHashFound = false;
            string hash = Utils.GenerateHash();
            var query = "SELECT * FROM @table WHERE @field = @hash";
            while (!uniqueHashFound)
            {
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@table", table);
                cmd.Parameters.AddWithValue("@field", field);
                cmd.Parameters.AddWithValue("@hash", hash);
                using MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows) {
                    hash = Utils.GenerateHash();
                } else
                {
                    uniqueHashFound = true;
                }
            }

            // close the connection
            connection.Close();

            return hash;
        }

        public string? GetAvatar(string userId)
        {
            System.Diagnostics.Debug.WriteLine("getting avatar");
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // query database
            var query = "SELECT * FROM users WHERE uid = @uid";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@uid", userId);
            using MySqlDataReader reader = cmd.ExecuteReader();

            // get avatar
            string? avatar = null;
            if (reader.HasRows)
            {
                reader.Read();
                var avatarData = reader.GetFieldValue<byte[]>("avatar");
                avatar = "data:image/png;base64," + Convert.ToBase64String(avatarData);
            }

            // close the connection
            reader.Close();
            connection.Close();

            return avatar;
        }

        public void SetAvatar(string userId, byte[] imageData)
        {
            System.Diagnostics.Debug.WriteLine("setting avatar");
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // query database for the user
            var user_query = "SELECT * FROM users WHERE uid = @uid";
            using var query_cmd = new MySqlCommand(user_query, connection);
            query_cmd.Parameters.AddWithValue("@uid", userId);
            using MySqlDataReader reader = query_cmd.ExecuteReader();
            bool userExists = reader.HasRows;
            reader.Close();

            if (userExists)
            {
                // update user data
                var sql = "UPDATE users SET avatar = @imageData WHERE uid = @uid";
                using var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@imageData", imageData).DbType = DbType.Binary;  // tells database to treat the byte array as a blob
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.ExecuteNonQuery();
            }
            else
            {
                // add user data
                var sql = "INSERT INTO users (uid, avatar) VALUES (@uid, @imageData)";
                using var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@imageData", imageData).DbType = DbType.Binary;  // ditto
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.ExecuteNonQuery();
            }

            // close the connection
            connection.Close();
        }

        public List<ProjectModel> GetProjects(string userId)
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // query database
            var query = "SELECT * FROM projects WHERE pid IN (SELECT project FROM developments WHERE developer == @uid)";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@uid", userId);
            using MySqlDataReader reader = cmd.ExecuteReader();

            // get list of projects
            var projects = new List<ProjectModel>();
            while (reader.Read())
            {
                projects.Add(new ProjectModel()
                {
                    ProjectId = reader.GetString("pid"),
                    ProjectName = reader.GetString("name")
                });
            };

            // close the connection
            reader.Close();
            connection.Close();

            return projects;
        }

        public void SearchProjects(string query)
        {

        }

        public void AddProject(string projectName, string userId)
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // find a unique hash
            string projectId = FindUniqueHash("projects", "pid");

            // add project data to database
            var sql = "INSERT INTO projects (pid, avatar) VALUES (@pid, @pname)";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@pid", projectId);
            cmd.Parameters.AddWithValue("@pname", projectName);
            cmd.ExecuteNonQuery();

            // assign user to project in database
            AddDeveloper(projectId, userId);

            // close the connection
            connection.Close();
        }

        public void DeleteProject(string projectId, string developerId)
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // check if user is the only developer
            var query = "SELECT COUNT(*) AS count FROM developments WHERE project = @pid";
            using var query_cmd = new MySqlCommand(query, connection);
            query_cmd.Parameters.AddWithValue("@pid", projectId);
            using MySqlDataReader reader = query_cmd.ExecuteReader();
            reader.Read();
            var numDevelopers = reader.GetInt64("count");

            if (numDevelopers > 1)
            {
                // remove developer from project
                var sql = "DELETE FROM developments WHERE developer = @uid";
                using var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@uid", developerId);
                cmd.ExecuteNonQuery();
            } 
            else
            {
                // remove project from database
                var sql = "DELETE FROM projects WHERE pid = @pid";
                using var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@pid", projectId);
                cmd.ExecuteNonQuery();
            }

            // close the connection
            reader.Close();
            connection.Close();
        }

        public List<UserModel> GetDevelopers(string projectId)
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // query database
            var query = "SELECT * FROM users WHERE uid IN (SELECT developer FROM developments WHERE project = @pid)";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@pid", projectId);
            using MySqlDataReader reader = cmd.ExecuteReader();

            // get developers
            var developers = new List<UserModel>();
            while (reader.Read())
            {
                developers.Add(new UserModel()
                {
                    UserId = reader.GetString("uid")
                    // ...
                });
            }

            // close the connection
            reader.Close();
            connection.Close();

            return developers;
        }

        public void AddDeveloper(string projectId, string developerId)
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // find a unique hash
            string developmentId = FindUniqueHash("developments", "did");

            // add project developer data to database
            var sql = "INSERT INTO developments (did, project, developer) VALUES (@did, @pid, @uid)";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@did", developmentId);
            cmd.Parameters.AddWithValue("@pid", projectId);
            cmd.Parameters.AddWithValue("@uid", developerId);
            cmd.ExecuteNonQuery();

            // close the connection
            connection.Close();
        }

        public void AddReport(string projectId, string userId)  // add other arguments
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // find a unique hash
            string developmentId = FindUniqueHash("developments", "did");

            // add report data to database
            var sql = "INSERT INTO bug_reports (bid, reportee, project) VALUES (@did, @uid, @pid)";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@did", developmentId);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@pid", projectId);
            cmd.ExecuteNonQuery();

            // close the connection
            connection.Close();
        }

        public void AddComment(string reportId, string userId)  // add other arguments
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // find a unique hash
            string commentId = FindUniqueHash("comments", "cid");

            // add comment data to database
            var sql = "INSERT INTO comments (cid, commenter, bug_report) VALUES (@cid, @uid, @bid)";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@cid", commentId);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@bid", reportId);
            cmd.ExecuteNonQuery();

            // close the connection
            connection.Close();
        }

        public List<BugReportModel> GetAssignments(string userId)
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // query database for the bug reports assigned to the user
            var user_query = "SELECT * FROM bug_reports WHERE bid IN (SELECT bug_report FROM assignments WHERE assignee = @uid)";
            using var query_cmd = new MySqlCommand(user_query, connection);
            query_cmd.Parameters.AddWithValue("@uid", userId);
            using MySqlDataReader reader = query_cmd.ExecuteReader();

            // get bug reports assigned to the user
            var bugs = new List<BugReportModel>();
            while (reader.Read())
            {
                bugs.Add(new BugReportModel()
                {
                    ReportId = reader.GetString("bid"),
                    ReporterId = reader.GetString("reportee"),
                    ProjectId = reader.GetString("project"),
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

            // close the connection
            reader.Close();
            connection.Close();

            return bugs;
        }

        public void AddAssignment(string reportId, string userId)
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // find a unique hash
            string assignmentId = FindUniqueHash("comments", "cid");

            // add comment data to database
            var sql = "INSERT INTO comments (cid, assignee, bug_report) VALUES (@cid, @uid, @bid)";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@cid", assignmentId);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@bid", reportId);
            cmd.ExecuteNonQuery();

            // close the connection
            connection.Close();
        }

        public void RemoveAssignment(string reportId, string userId)
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // add comment data to database
            var sql = "DELETE FROM comments WHERE assignee = @uid AND bug_report = @bid";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@bid", reportId);
            cmd.ExecuteNonQuery();

            // close the connection
            connection.Close();
        }

        public void UpdateBugPriority(string reportId, string priority)
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // add comment data to database
            var sql = "UPDATE bug_reports SET priority = @priority WHERE bug_report = @bid";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@priority", priority);
            cmd.Parameters.AddWithValue("@bid", reportId);
            cmd.ExecuteNonQuery();

            // close the connection
            connection.Close();
        }

        public void UpdateBugSeverity(string reportId, string severity)
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // add comment data to database
            var sql = "UPDATE bug_reports SET severity = @severity WHERE bug_report = @bid";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@severity", severity);
            cmd.Parameters.AddWithValue("@bid", reportId);
            cmd.ExecuteNonQuery();

            // close the connection
            connection.Close();
        }

        public void UpdateBugStatus(string reportId, string status)
        {
            // connect to database
            using MySqlConnection connection = GetConnection();
            connection.Open();

            // add comment data to database
            var sql = "UPDATE bug_reports SET status = @status WHERE bug_report = @bid";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@bid", reportId);
            cmd.ExecuteNonQuery();

            // close the connection
            connection.Close();
        }
    }
}
