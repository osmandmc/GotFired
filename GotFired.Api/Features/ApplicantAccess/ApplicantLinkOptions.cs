namespace GotFired.Api.Features.ApplicantAccess
{
    /// <summary>Bound from the "ApplicantLinks" configuration section.</summary>
    public class ApplicantLinkOptions
    {
        public const string SectionName = "ApplicantLinks";

        /// <summary>How long an emailed link stays valid. Each staff answer issues a new link.</summary>
        public TimeSpan Lifetime { get; set; } = TimeSpan.FromDays(90);
    }
}
