using System.Security.Cryptography;

namespace BugTracker.Models
{
    /// <summary>
    /// Class <c>Utils</c> is a helper class with miscellaneous utility functions.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Method <c>generateHash</c> generates a random hexadecimal SHA-256 hash.
        /// </summary>
        /// <returns>The generated hash.</returns>
        public static string GenerateHash()
        {
            // create generators
            var sha256 = SHA256.Create();
            var random = RandomNumberGenerator.Create();

            // generate the hash
            var bytes = new byte[32];
            random.GetBytes(bytes);
            var hash = sha256.ComputeHash(bytes);

            // convert hash to string
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Method <c>GetSeededAvatar</c> gets the website link (image source) for an avatar from DiceBearAPI.
        /// </summary>
        /// <param name="seed">The seed to generate the avatar from.</param>
        /// <param name="backgroundType">The background type of the avatar (gradientLinear or solid).</param>
        /// <param name="radius">The border radius of the avatar, default gives a round avatar.</param>
        /// <returns></returns>
        public static string GetSeededAvatar(string seed, string backgroundType = "gradientLinear", int radius = 50)
        {
            return $"https://api.dicebear.com/6.x/shapes/svg?seed={seed}&backgroundType={backgroundType}&radius={radius}";
        }
    }
}
