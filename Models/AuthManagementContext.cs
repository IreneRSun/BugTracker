using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace BugTracker.Models
{
    /// <summary>
    /// Class <c>AuthManagementContext</c> models an API context that manages connections to the Auth0 Management API.
    /// </summary>
    public class AuthManagementContext
    {
        /// <value>
        /// Property <c>domain</c> is the domain associated with the Auth0 account.
        /// </value>
        private readonly string domain;

        /// <value>
        /// Property <c>clientId</c> is the application ID.
        /// </value>
        private readonly string clientId;

        /// <value>
        /// Property <c>clientSecret</c> is the password associated with the application ID.
        /// </value>
        private readonly string clientSecret;

        /// <value>
        /// Property <c>audience</c> is the unique identifier of the API.
        /// </value>
        private readonly string audience;

        /// <summary>
        /// Method <c>AuthManagementContext</c> initializes the class with the relevant appsettings.json configurations.
        /// </summary>
        /// <param name="configuration">The appsettings.json configuration.</param>
        public AuthManagementContext(IConfiguration configuration) {
            domain = configuration["Auth0:Domain"];
            clientId = configuration["Auth0:ClientId"];
            clientSecret = configuration["Auth0:ClientSecret"];
            audience = configuration["Auth0:Audience"];
        }

        /// <summary>
        /// Method <c>GetClient</c> gets the Auth0 Management API client.
        /// </summary>
        /// <returns>The api client.</returns>
        private ManagementApiClient GetClient()
        {
            // request token
            var client = new RestClient($"https://{domain}");
            var request = new RestRequest("/oauth/token", Method.Post);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", 
                $"{{\"client_id\":\"{clientId}\",\"client_secret\":\"{clientSecret}\",\"audience\":\"{audience}\",\"grant_type\":\"client_credentials\"}}", 
                ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            // get token from request
            string? token = null;
            if (response.Content != null)
            {
                var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content);
                if (json != null)
                {
                    token = json["access_token"];
                }
            }

            // get management api client
            return new ManagementApiClient(token, domain);
        }

        /// <summary>
        /// Method <c>UpdateUsername</c> updates the username of the user.
        /// </summary>
        /// <param name="userId">The ID of the user to update the name of.</param>
        /// <param name="newName">The new name to update to.</param>
        public async Task UpdateUsername(string? userId, string newName)
        {
            if (userId != null)
            {
                ManagementApiClient client = GetClient();
                var request = new UserUpdateRequest
                {
                    UserMetadata = new
                    {
                        name = newName
                    }
                };
                await client.Users.UpdateAsync(userId, request);
            }  
        }

        /// <summary>
        /// Method <c>DeleteUser</c> deletes a user from the Auth0 database.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        public async Task DeleteUser(string? userId)
        {
            if (userId != null)
            {
                ManagementApiClient client = GetClient();
                await client.Users.DeleteAsync(userId);
            }
        }

        /// <summary>
        /// Method <c>FillUserData</c> fills a list of UserModel objects with their corresponding data from the Auth0 database.
        /// </summary>
        /// <param name="users">The list of users.</param>
        public async Task FillUserData(List<UserModel> users)
        {
            ManagementApiClient client = GetClient();
            foreach (var user in users)
            {
                var userData = await client.Users.GetAsync(user.UserId);
                user.UserName = userData.UserMetadata.name;
                user.EmailAddress = userData.Email;
            }
        }

        /// <summary>
        /// Method <c>SearchUsers</c> searches for users whose usernames match the query (case-sensitive).
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>The list of users satisfying the criteria.</returns>
        public async Task<List<UserModel>> SearchUsers(string searchQuery)
        {
            // search for users
            ManagementApiClient client = GetClient();
            var request = new GetUsersRequest
            {
                Query = $"name: {searchQuery.ToLower()}*"
            };

            var queryResults = await client.Users.GetAllAsync(request);

            // parse result
            var users = new List<UserModel>();
            foreach (var result in queryResults)
            {
                var user = new UserModel()
                {
                    UserId = result.UserId,
                    EmailAddress = result.Email,
                    UserName = result.UserMetadata.name
                };
                users.Add(user);
            }

            return users;
        }
    }
}
