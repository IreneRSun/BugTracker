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

        /// <summary>
        /// Method <c>GetSeededAvatar</c> gets the website link for an avatar from DiceBearAPI.
        /// </summary>
        /// <param name="seed">The seed to generate the avatar from.</param>
        /// <param name="backgroundType">The background type of the avatar (gradientLinear or solid).</param>
        /// <param name="radius">The border radius of the avatar, default gives a round avatar.</param>
        /// <returns></returns>
        public static string GetSeededAvatar(string seed, string backgroundType = "gradientLinear", int radius=50)
        {
            return $"https://api.dicebear.com/6.x/shapes/svg?seed={seed}&backgroundType={backgroundType}&radius={radius}";
        }

        /// <summary>
        /// Method <c>GetSeededAvatar</c> gets the website link for an icon from DiceBearAPI.
        /// </summary>
        /// <param name="iconName">The name of the icon to get. Refer to https://www.dicebear.com/styles/icons for available icons.</param>
        /// <param name="backgroundType">The background type of the icon (gradientLinear or solid).</param>
        /// <param name="backgroundColor">The background color of the icon.</param>
        /// <param name="backgroundRotation">The background rotation of the icon (affects the gradient).</param>
        /// <param name="flip">Whether to flip the icon or not.</param>
        /// <param name="rotate">The rotation degree of the icon.</param>
        /// <param name="radius">The border radius of the avatar.</param>
        /// <returns></returns>
        public static string GetIcon(string iconName, 
            string backgroundType = "gradientLinear", string backgroundColor = "b6e3f4,c0aed", int backgroundRotation = 115,
            bool flip = false, int rotate = 0, int radius=50)
        {
            return $"https://api.dicebear.com/6.x/icons/svg?icon={iconName}" +
                $"&backgroundType={backgroundType}" +
                $"&backgroundColor={backgroundColor}" +
                $"&backgroundRotation={backgroundRotation}" +
                $"&rotate={rotate}" +
                $"&flip={flip}" +
                $"&radius={radius}";
        }
    }
}
