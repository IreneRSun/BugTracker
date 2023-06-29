using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using BugTracker.Models.EntityModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace BugTracker.Models.DatabaseContexts
{
    /// <summary>
    /// Class <c>AuthManagementContext</c> models an API context that manages connections to the Auth0 Management API.
    /// </summary>
    public class UserManagementContext
    {
        /// <value>
        /// Property <c>_domain</c> is the domain associated with the Auth0 account.
        /// </value>
        private readonly string _domain;

        /// <value>
        /// Property <c>_clientId</c> is the application ID.
        /// </value>
        private readonly string _clientId;

        /// <value>
        /// Property <c>_clientSecret</c> is the password associated with the application ID.
        /// </value>
        private readonly string _clientSecret;

        /// <value>
        /// Property <c>_audience</c> is the unique identifier of the API.
        /// </value>
        private readonly string _audience;

        /// <summary>
        /// Method <c>AuthManagementContext</c> initializes the class with the relevant configurations.
        /// </summary>
        /// <param name="domain">The domain to initialize with.</param>
        /// <param name="clientId">The client ID to initialize with.</param>
        /// <param name="clientSecret">The client secret to initialize with.</param>
        /// <param name="audience">The audience to initialize with.</param>
        public UserManagementContext(string domain, string clientId, string clientSecret, string audience)
        {
            _domain = domain;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _audience = audience;
        }

        /// <summary>
        /// Method <c>GetToken</c> gets the Auth0 access token.
        /// Returns null if none found.
        /// </summary>
        /// <returns>The access token.</returns>
        private string? GetToken()
        {
            // request token
            var client = new RestClient($"https://{_domain}");
            var request = new RestRequest("/oauth/token", Method.Post);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json",
                $"{{\"client_id\":\"{_clientId}\",\"client_secret\":\"{_clientSecret}\",\"audience\":\"{_audience}\",\"grant_type\":\"client_credentials\"}}",
                ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            // get token from request
            string? token = null;
            if (response.Content != null)
            {
                var jsonResponse = JsonConvert.DeserializeObject<JObject>(response.Content);
                if (jsonResponse != null)
                {
                    JToken? jsonToken = jsonResponse["access_token"];
                    if (jsonToken != null)
                    {
                        token = jsonToken.ToString();
                    }
                }
            }

            return token;
        }

        /// <summary>
        /// Method <c>GetUser</c> gets a UserModel with user data from the Auth0 database.
        /// </summary>
        /// <param name="userId">The ID of the user to get the data of.</param>
        /// <returns>The UserModel with the user data.</returns>
        public async Task<UserModel> GetUser(string userId)
        {
            string? token = GetToken();
            var client = new ManagementApiClient(token, _domain);
            var userData = await client.Users.GetAsync(userId);

            return new UserModel(userId)
            {
                Name = userData.NickName,
                Email = userData.Email,
                Avatar = userData.Picture
            };
        }

        /// <summary>
        /// Method <c>GetUserData</c> fills a UserModel with corresponding user data from the Auth0 database.
        /// </summary>
        /// <param name="user">The UserModel to fill with the user's data.
        /// User data retrieved corresponds to the ID attribute of the class.</param>
        public async Task GetUserData(UserModel user)
        {
			string? token = GetToken();
			var client = new ManagementApiClient(token, _domain);
			var userData = await client.Users.GetAsync(user.ID);

            user.Name = userData.NickName;
            user.Email = userData.Email;
            user.Avatar ??= userData.Picture;
		}

		/// <summary>
		/// Method <c>UpdateUsername</c> updates the username of the user in the Auth0 database.
		/// </summary>
		/// <param name="userId">The ID of the user to update the name of.</param>
		/// <param name="newName">The new name to update to.</param>
		public async Task UpdateUsername(string userId, string newName)
        {
            string? token = GetToken();
            var client = new ManagementApiClient(token, _domain);

            var request = new UserUpdateRequest
            {
                NickName = newName
            };
            await client.Users.UpdateAsync(userId, request);
        }

        /// <summary>
        /// Method <c>DeleteUser</c> deletes a user from the Auth0 database.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        public async Task DeleteUser(string userId)
        {
            string? token = GetToken();
            var client = new ManagementApiClient(token, _domain);
            await client.Users.DeleteAsync(userId);
        }

        /// <summary>
        /// Method <c>SearchUsers</c> searches for users whose usernames match the query (case-insensitive).
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns>The list of users satisfying the search criteria.</returns>
        public List<UserModel> SearchUsers(string searchQuery)
        {
            // search for users
            string query = searchQuery.Length > 2 ? $"*{searchQuery}*" : $"{searchQuery}*";
            string? token = GetToken();
            var client = new RestClient($"https://{_domain}");
            var request = new RestRequest($"/api/v2/users?q=nickname%3A{query}&search_engine=v3", Method.Get);
            request.AddHeader("authorization", $"Bearer {token}");
            RestResponse response = client.Execute(request);

            // parse results
            var users = new List<UserModel>();
            if (response.Content != null)
            {
                var json = JsonConvert.DeserializeObject<JArray>(response.Content);
                if (json != null)
                {
                    foreach (JToken userJObject in json)
                    {
                        if (userJObject != null)
                        {
							var userId = userJObject["user_id"]?.ToString();
                            if (userId != null)
                            {
								var user = new UserModel(userId)
								{
									Email = userJObject["email"]?.ToString(),
									Name = userJObject["nickname"]?.ToString(),
									Avatar = userJObject["picture"]?.ToString()
								};
								users.Add(user);
							}
						}
                    }
                }
            }

            return users;
        }
    }
}
