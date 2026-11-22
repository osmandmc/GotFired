namespace GotFired.Api.Features.Auth
{
    /// <summary>Bound from the "Jwt" configuration section.</summary>
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        /// <summary>HMAC-SHA256 signing key, at least 32 bytes. Supply via the Jwt__Key environment variable.</summary>
        public string Key { get; set; }
        public string Issuer { get; set; } = "gotfired";
        public string Audience { get; set; } = "gotfired";
        /// <summary>The OWIN server issued 24 hour tokens.</summary>
        public TimeSpan Lifetime { get; set; } = TimeSpan.FromHours(24);
    }
}
