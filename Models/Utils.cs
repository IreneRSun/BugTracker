using System.Drawing;
using System.Security.Cryptography;
using System.Text;

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
        /// Method <c>GenerateSeededColor</c> generates a color that can be used in a cshtml page from the first 12 bytes of string seed (if available).
        /// </summary>
        /// <param name="seed">The seed to generate the color from.</param>
        /// <returns>The generated color.</returns>
        public static string GenerateSeededColor(string seed)
        {
			// convert the seed to an appropriate byte array of the required length
			byte[] bytes = Encoding.UTF8.GetBytes(seed);
            Array.Resize(ref bytes, 12);

			// get three integers between 0 to 255 from the seed
			int rValue = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 256;
            int gValue = Math.Abs(BitConverter.ToInt32(bytes, 4)) % 256;
            int bValue = Math.Abs(BitConverter.ToInt32(bytes, 8)) % 256;

            // convert integers to an html color
            var color = Color.FromArgb(rValue, gValue, bValue);
            var htmlColor = ColorTranslator.ToHtml(color);
            return htmlColor;
        }
    }
}
