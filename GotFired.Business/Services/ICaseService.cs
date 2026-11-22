using GotFired.Model.Entities.Enums;
using GotFired.Model.ViewModels;

namespace GotFired.Business.Services
{
    /// <summary>Dismissal cases: the public application, the staff queue, and the applicant's conversation.</summary>
    public interface ICaseService
    {
        // Public application form
        string NewApplicationGuid();
        ApplicationLookups GetApplicationLookups();
        /// <summary>Stores the application; a repeated Guid (double-clicked submit) is ignored.</summary>
        void SubmitApplication(DismissalCaseApplyModel model);

        // Staff
        (IReadOnlyList<DismissalCaseListModel> Cases, int Total) ListCases(int page, int pageSize, AppealState? state = null);
        DismissalCaseViewModel GetCase(int id);
        void SaveNotes(NoteModel model);
        void MarkAnswered(int caseId);
        void Evaluate(EvaluationViewModel model);
        void AddTag(TagLinkModel model);
        void RemoveTag(TagLinkModel model);
        /// <summary>Stores a staff answer and issues a new applicant link; the caller emails it.</summary>
        ApplicantNotification Answer(int caseId, string text, string staffUserName);
        IReadOnlyList<LookupIDNamePair> GetCategories();
        IReadOnlyList<LookupIDNamePair> GetTags(string search = null);

        // Applicant (secret link token)
        DismissalCaseViewModel GetApplicantView(string token);
        CommentDto AddApplicantReply(string token, string text);
    }

    public record ApplicationLookups(
        IReadOnlyList<LookupIDNamePair> Cities,
        IReadOnlyList<LookupIDNamePair> DeclaredTerminationReasons,
        IReadOnlyList<LookupIDNamePair> SupportedBys,
        IReadOnlyList<CodedLookup> SgkTerminationReasons);

    /// <summary>Applicant email address and the new link token to send after a staff answer.</summary>
    public record ApplicantNotification(string Email, string Token);
}
