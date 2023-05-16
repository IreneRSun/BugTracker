using System.Security.Cryptography;

namespace BugTracker.Models
{
    /// <summary>
    /// Class <c>Utils</c> is a helper class with miscellaneous utility functions.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Method <c>generateHash</c> generates a random SHA-256 hash.
        /// </summary>
        /// <returns>The generated hash.</returns>
        public static string GenerateHash()
        {
            // create geneerators
            var sha256 = SHA256.Create();
            var random = RandomNumberGenerator.Create();

            // generate the hash
            var bytes = new byte[32];
            random.GetBytes(bytes);
            var hash = sha256.ComputeHash(bytes);

            // convert hash to string
            return Convert.ToBase64String(hash);
        }
    }
}
