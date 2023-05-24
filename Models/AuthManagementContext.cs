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
        /// Method <c>GetToken</c> gets the Auth0 access token.
        /// </summary>
        /// <returns>The access token.</returns>
        private string? GetToken()
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
                var jsonResponse = JsonConvert.DeserializeObject<JObject>(response.Content);
                if (jsonResponse != null)
                {
                    token = jsonResponse["access_token"].ToString();
                }
            }

            return token;
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
                string? token = GetToken();
                var client = new ManagementApiClient(token, domain);
                var request = new UserUpdateRequest
                {
                    NickName = newName
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
                string? token = GetToken();
                var client = new ManagementApiClient(token, domain);
                await client.Users.DeleteAsync(userId);
            }
        }

        /// <summary>
        /// Method <c>FillUserData</c> fills a list of UserModel objects with their corresponding data from the Auth0 database.
        /// </summary>
        /// <param name="users">The list of users.</param>
        public async Task FillUserData(List<UserModel> users)
        {
            string? token = GetToken();
            var client = new ManagementApiClient(token, domain);
            foreach (var user in users)
            {
                var userData = await client.Users.GetAsync(user.UserId);
                user.UserName = userData.NickName;
                user.EmailAddress = userData.Email;
                user.Avatar ??= userData.Picture;
            }
        }

        /// <summary>
        /// Method <c>SearchUsers</c> searches for users whose usernames match the query (case-insensitive).
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="maxResults">The maximum number of users to return.</param>
        /// <returns>The list of users satisfying the criteria.</returns>
        public async Task<List<UserModel>> SearchUsers(string searchQuery, int maxResults = 10)
        {
            // search for users
            string query = searchQuery.Length > 2 ? $"*{searchQuery}*" : $"{searchQuery}*";

            string? token = GetToken();
            var client = new RestClient($"https://{domain}");
            var request = new RestRequest($"/api/v2/users?q=nickname%3A{query}&page=0&per_page={maxResults}&search_engine=v3", Method.Get);
            request.AddHeader("authorization", $"Bearer {token}");
            RestResponse response = client.Execute(request);

            // parse results
            var users = new List<UserModel>();
            if (response.Content != null)
            {
                var json = JsonConvert.DeserializeObject<JObject>(response.Content);
                if (json != null)
                {
                    var usersJObject = (JArray)json["users"];
                    foreach (JObject userJObject in usersJObject)
                    {
                        var user = new UserModel()
                        {
                            UserId = userJObject["user_id"].ToString(),
                            EmailAddress = userJObject["email"].ToString(),
                            UserName = userJObject["nickname"].ToString(),
                            Avatar = userJObject["picture"].ToString()
                        };
                        users.Add(user);
                    }
                }
            }

            return users;
        }
    }
}
