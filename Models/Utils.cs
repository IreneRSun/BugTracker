using System.Drawing;
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
        /// <returns>The generated avatar image source.</returns>
        public static string GetSeededAvatar(string seed, string backgroundType = "gradientLinear", int radius = 50)
        {
            return $"https://api.dicebear.com/6.x/shapes/svg?seed={seed}&backgroundType={backgroundType}&radius={radius}";
        }

        /// <summary>
        /// Method <c>GenerateColor</c> generates a random color that can be used in a cshtml page.
        /// </summary>
        /// <returns>The generated color.</returns>
        public static string GenerateColor()
        {
            var random = new Random();
            var color = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
            var htmlColor = ColorTranslator.ToHtml(color);
            return htmlColor;
        }
    }
}
