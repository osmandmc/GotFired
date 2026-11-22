using System.Linq.Expressions;
using System.Security.Cryptography;
using GotFired.Model.Entities.Enums;

namespace GotFired.Model.Entities
{
    /// <summary>
    /// A dismissal case and its conversation: the aggregate root. All state changes go through its methods, which
    /// enforce the workflow:
    /// <list type="bullet">
    /// <item>a submitted case is <see cref="AppealState.Pending"/>;</item>
    /// <item>a staff answer makes it <see cref="AppealState.Answered"/> and issues a new secret applicant link
    /// (replacing the previous one);</item>
    /// <item>an applicant reply, only with a valid unexpired link, makes it Pending again.</item>
    /// </list>
    /// </summary>
    public class DismissalCase : BaseEntity
    {
        private readonly List<Comment> _comments = [];
        private readonly List<DismissalCaseTag> _tags = [];
        private readonly List<DismissalCaseSupportedBy> _supporters = [];

        private DismissalCase() { } // for EF Core

        /// <summary>Identifies one submission of the public form (a double-clicked submit reuses it).</summary>
        public string Guid { get; private set; }
        public int ApplicantId { get; private set; }
        public Applicant Applicant { get; private set; }

        // What the applicant reported. Set once, on submission.
        public int CityID { get; private set; }
        public DateTime? EmploymentTerminationDate { get; private set; }
        public int? CompanySectorID { get; private set; }
        public int? DeclaredTerminationReasonID { get; private set; }
        public string OtherDeclaredTerminationReason { get; private set; }
        public int? CategoryID { get; private set; }
        public EmploymentDurationSince EmploymentDurationSince { get; private set; }
        public EmployeeCount EmployeeCount { get; private set; }
        public string CompanySectorName { get; private set; }
        public string CompanyDescription { get; private set; }
        public bool HasSignedAnyDocument { get; private set; }
        public string SignedDocuments { get; private set; }
        public DismissalState DismissalState { get; private set; }
        public bool WantShareHisExperience { get; private set; }
        public string DetailedExperience { get; private set; }
        public string AdditionalInfo { get; private set; }
        public int? SGKTerminationReasonID { get; private set; }

        public City City { get; private set; }
        public CompanySector CompanySector { get; private set; }
        public DeclaredTerminationReason DeclaredTerminationReason { get; private set; }
        public Category Category { get; private set; }
        public SGKTerminationReason SGKTerminationReason { get; private set; }

        // Workflow state.
        public AppealState AppealState { get; private set; }
        /// <summary>Staff-only notes; never shown to the applicant.</summary>
        public string Notes { get; private set; }
        /// <summary>SHA-256 of the applicant's secret link token (the token itself is never stored).</summary>
        public byte[] AccessTokenHash { get; private set; }
        public DateTime? AccessTokenExpiresAt { get; private set; }

        public IReadOnlyCollection<Comment> Comments => _comments;
        public IReadOnlyCollection<DismissalCaseTag> Tags => _tags;
        public IReadOnlyCollection<DismissalCaseSupportedBy> Supporters => _supporters;

        // ---- Submission ------------------------------------------------------------------------------

        public static DismissalCase Submit(string guid, Applicant applicant, CaseDetails details, IEnumerable<int> supportedByIds)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(guid);
            ArgumentNullException.ThrowIfNull(applicant);
            ArgumentNullException.ThrowIfNull(details);

            var dismissalCase = new DismissalCase
            {
                Guid = guid,
                Applicant = applicant,
                AppealState = AppealState.Pending,   // whatever the form claims
                CityID = details.CityId,
                EmploymentTerminationDate = details.EmploymentTerminationDate,
                DeclaredTerminationReasonID = details.DeclaredTerminationReasonId,
                OtherDeclaredTerminationReason = details.OtherDeclaredTerminationReason,
                SGKTerminationReasonID = details.SgkTerminationReasonId,
                EmploymentDurationSince = details.EmploymentDurationSince,
                EmployeeCount = details.EmployeeCount,
                CompanySectorName = details.CompanySectorName,
                CompanyDescription = details.CompanyDescription,
                HasSignedAnyDocument = details.HasSignedAnyDocument,
                SignedDocuments = details.SignedDocuments,
                DismissalState = details.DismissalState,
                WantShareHisExperience = details.WantShareHisExperience,
                DetailedExperience = details.DetailedExperience,
                AdditionalInfo = details.AdditionalInfo,
            };
            foreach (int id in (supportedByIds ?? []).Distinct())
            {
                dismissalCase._supporters.Add(new DismissalCaseSupportedBy(id));
            }
            return dismissalCase;
        }

        // ---- Staff -----------------------------------------------------------------------------------

        /// <summary>
        /// Records a staff answer, marks the case answered and issues a new applicant link, invalidating the
        /// previous one. Returns the comment and the link token to email; only the token's hash is kept.
        /// </summary>
        public StaffAnswer AnswerByStaff(string text, string staffUserName, DateTime utcNow, TimeSpan linkLifetime)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(staffUserName);
            if (linkLifetime <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(linkLifetime));

            var comment = new Comment(text, staffUserName);
            _comments.Add(comment);
            AppealState = AppealState.Answered;

            var (token, hash) = ApplicantAccessToken.Create();
            AccessTokenHash = hash;
            AccessTokenExpiresAt = utcNow.Add(linkLifetime);
            return new StaffAnswer(comment, token);
        }

        /// <summary>Staff mark a pending case as handled without answering.</summary>
        public void MarkAnswered() => AppealState = AppealState.Answered;

        public void UpdateNotes(string notes) => Notes = notes;

        /// <summary>Makes the case's tags exactly <paramref name="tagIds"/>.</summary>
        public void Evaluate(IEnumerable<int> tagIds)
        {
            var wanted = (tagIds ?? []).ToHashSet();
            _tags.RemoveAll(t => !wanted.Contains(t.TagID));
            foreach (int id in wanted.Where(id => _tags.All(t => t.TagID != id)))
            {
                _tags.Add(new DismissalCaseTag(id));
            }
        }

        /// <summary>Adds a tag; a tag is on a case at most once.</summary>
        public void AddTag(int tagId)
        {
            if (_tags.All(t => t.TagID != tagId)) _tags.Add(new DismissalCaseTag(tagId));
        }

        public void RemoveTag(int tagId) => _tags.RemoveAll(t => t.TagID == tagId);

        // ---- Applicant -------------------------------------------------------------------------------

        /// <summary>
        /// The applicant's reply through their emailed link: recorded under the applicant's name and puts the case
        /// back in the staff queue. Requires <see cref="Applicant"/> to be loaded.
        /// </summary>
        public Comment ReplyAsApplicant(string token, string text, DateTime utcNow)
        {
            if (!IsAccessibleWith(token, utcNow))
                throw new BusinessRuleException("Bağlantı geçersiz veya süresi dolmuş.");
            if (Applicant == null)
                throw new InvalidOperationException("The applicant must be loaded to record their reply.");

            var comment = new Comment(text, Applicant.FullName);
            _comments.Add(comment);
            AppealState = AppealState.Pending;
            return comment;
        }

        /// <summary>Whether <paramref name="token"/> is this case's current, unexpired applicant link.</summary>
        public bool IsAccessibleWith(string token, DateTime utcNow)
        {
            byte[] hash = ApplicantAccessToken.TryHash(token);
            return hash != null
                   && AccessTokenHash != null
                   && CryptographicOperations.FixedTimeEquals(hash, AccessTokenHash)
                   && AccessTokenExpiresAt > utcNow;
        }

        /// <summary>
        /// The same rule as <see cref="IsAccessibleWith"/>, as a query filter (the hash is indexed), so finding a
        /// case by its link and checking the link can never disagree.
        /// </summary>
        /// <remarks>SequenceEqual, not ==: in memory == would compare array references; EF Core translates both to a
        /// varbinary equality in SQL.</remarks>
        public static Expression<Func<DismissalCase, bool>> AccessibleWith(byte[] tokenHash, DateTime utcNow) =>
            d => d.AccessTokenHash != null && d.AccessTokenHash.SequenceEqual(tokenHash) && d.AccessTokenExpiresAt > utcNow;
    }

    /// <summary>The comment a staff answer created and the applicant link token to email with it.</summary>
    public sealed record StaffAnswer(Comment Comment, string LinkToken);

    /// <summary>What the applicant reports about their dismissal on the public form.</summary>
    public sealed record CaseDetails
    {
        public int CityId { get; init; }
        public DateTime? EmploymentTerminationDate { get; init; }
        public int? DeclaredTerminationReasonId { get; init; }
        public string OtherDeclaredTerminationReason { get; init; }
        public int? SgkTerminationReasonId { get; init; }
        public EmploymentDurationSince EmploymentDurationSince { get; init; }
        public EmployeeCount EmployeeCount { get; init; }
        public string CompanySectorName { get; init; }
        public string CompanyDescription { get; init; }
        public bool HasSignedAnyDocument { get; init; }
        public string SignedDocuments { get; init; }
        public DismissalState DismissalState { get; init; }
        public bool WantShareHisExperience { get; init; }
        public string DetailedExperience { get; init; }
        public string AdditionalInfo { get; init; }
    }
}
