using System.Security.Cryptography;

namespace GotFired.Business
{
    /// <summary>
    /// Secret tokens for the applicant's link (userView.html?guid=&lt;token&gt;). 256 random bits, base64url so it is
    /// safe in a URL. Only the SHA-256 hash is stored, so a database leak does not expose working links.
    /// </summary>
    public static class ApplicantAccessToken
    {
        public static (string Token, byte[] Hash) Create()
        {
            string token = Base64Url(RandomNumberGenerator.GetBytes(32));
            return (token, Hash(token));
        }

        /// <summary>Hash of a token from a request, or null when it cannot be a token we issued.</summary>
        public static byte[] TryHash(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length != 43 || token.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
                return null;
            return Hash(token);
        }

        private static byte[] Hash(string token) => SHA256.HashData(System.Text.Encoding.ASCII.GetBytes(token));

        private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
