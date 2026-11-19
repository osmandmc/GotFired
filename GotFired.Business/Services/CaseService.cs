using GotFired.DAL;
using GotFired.Model;
using GotFired.Model.Entities;
using GotFired.Model.Entities.Enums;
using GotFired.Model.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GotFired.Business.Services
{
    public class CaseService(GotFiredDbContext db, IOptions<ApplicantLinkOptions> linkOptions, TimeProvider time) : ICaseService
    {
        private readonly ApplicantLinkOptions _linkOptions = linkOptions.Value;

        // ---- Public application form ----------------------------------------------------------------

        public string NewApplicationGuid() => Guid.NewGuid().ToString();

        public ApplicationLookups GetApplicationLookups() => new(
            db.City.OrderBy(c => c.ID).Select(c => new LookupIDNamePair { ID = c.ID, Name = c.Name }).ToList(),
            db.DeclaredTerminationReason.OrderBy(r => r.ID).Select(r => new LookupIDNamePair { ID = r.ID, Name = r.Name }).ToList(),
            db.SupportedBy.OrderBy(s => s.ID).Select(s => new LookupIDNamePair { ID = s.ID, Name = s.Name }).ToList(),
            db.SGKTerminationReason.OrderBy(r => r.ID).Select(r => new CodedLookup { ID = r.ID, Code = r.Code, Name = r.Name }).ToList());

        public void SubmitApplication(DismissalCaseApplyModel model)
        {
            // The Guid is issued by GET apply when the form loads; a second POST with it is a double-clicked submit.
            if (!string.IsNullOrEmpty(model.Guid) && db.DismissalCase.Any(d => d.Guid == model.Guid))
            {
                return;
            }

            var applicant = Applicant.Create(model.ApplicantFullName, model.ApplicantEmail, model.ApplicantPhoneNumber,
                model.ApplicantGender, model.ApplicantAgeInterval, model.ApplicantEducationalState, model.ApplicantPositionId);
            var details = new CaseDetails
            {
                CityId = model.CityID,
                EmploymentTerminationDate = model.EmploymentTerminationDate,
                DeclaredTerminationReasonId = model.DeclaredTerminationReasonID,
                OtherDeclaredTerminationReason = model.OtherDeclaredTerminationReason,
                SgkTerminationReasonId = model.SGKTerminationReasonID,
                EmploymentDurationSince = model.EmploymentDurationSince,
                EmployeeCount = model.EmployeeCount,
                CompanySectorName = model.CompanySectorName,
                CompanyDescription = model.CompanyDescription,
                HasSignedAnyDocument = model.HasSignedAnyDocument,
                SignedDocuments = model.SignedDocuments,
                DismissalState = model.DismissalState,
                WantShareHisExperience = model.WantShareHisExperience,
                DetailedExperience = model.DetailedExperience,
                AdditionalInfo = model.AdditionalInfo,
            };
            string guid = string.IsNullOrEmpty(model.Guid) ? NewApplicationGuid() : model.Guid;

            db.DismissalCase.Add(DismissalCase.Submit(guid, applicant, details, model.SupportedByIds));
            db.SaveChanges();
        }

        // ---- Staff --------------------------------------------------------------------------------

        public (IReadOnlyList<DismissalCaseListModel> Cases, int Total) ListCases(int page, int pageSize, AppealState? state = null)
        {
            IQueryable<DismissalCase> cases = db.DismissalCase;
            if (state != null) cases = cases.Where(d => d.AppealState == state);

            var pageOfCases = cases
                .OrderByDescending(d => d.EditedDate).ThenByDescending(d => d.ID)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(d => new DismissalCaseListModel
                {
                    Id = d.ID,
                    AppealState = d.AppealState,
                    ApplicantEmail = d.Applicant.Email,
                    ApplicantFullName = d.Applicant.FullName,
                    CreatedDate = d.CreatedDate,
                    CommentDate = d.EditedDate,
                    CommentedBy = d.Comments.OrderByDescending(c => c.CreatedDate).Select(c => c.UserName).FirstOrDefault() ?? ""
                })
                .ToList();
            return (pageOfCases, cases.Count());
        }

        public DismissalCaseViewModel GetCase(int id)
        {
            var model = db.DismissalCase
                .Where(d => d.ID == id)
                .Select(d => new DismissalCaseViewModel
                {
                    Id = d.ID,
                    Guid = d.Guid,
                    AdditionalInfo = d.AdditionalInfo,
                    AgeInterval = d.Applicant.AgeInterval,
                    CityId = d.CityID,
                    CityName = d.City.Name,
                    CompanyDescription = d.CompanyDescription,
                    CompanySectorName = d.CompanySectorName,
                    EmployeeCount = d.EmployeeCount,
                    DeclaredTerminationReasonId = d.DeclaredTerminationReasonID,
                    DeclaredTerminationReasonName = d.DeclaredTerminationReason != null ? d.DeclaredTerminationReason.Name : "",
                    SGKTerminationReasonId = d.SGKTerminationReasonID,
                    SGKTerminationReasonName = d.SGKTerminationReason != null ? d.SGKTerminationReason.Code + " - " + d.SGKTerminationReason.Name : "",
                    Description = d.CompanyDescription,
                    DetailedExperience = d.DetailedExperience,
                    EmailAddress = d.Applicant.Email,
                    DismissalState = d.DismissalState,
                    EducationState = d.Applicant.EducationalState,
                    FullName = d.Applicant.FullName,
                    EmploymentDurationSince = d.EmploymentDurationSince,
                    EmploymentTerminationDate = d.EmploymentTerminationDate,
                    Gender = d.Applicant.Gender,
                    PhoneNumber = d.Applicant.PhoneNumber,
                    SignedDocuments = d.SignedDocuments,
                    WantShareHisExperienceString = d.WantShareHisExperience ? "Evet" : "Hayır",
                    SupportedByNames = d.Supporters.Select(s => s.SupportedBy.Name).ToList(),
                    Notes = d.Notes,
                    CategoryId = d.CategoryID,
                    DismissalCaseTags = d.Tags.Select(t => new LookupIDNamePair { ID = t.TagID, Name = t.Tag.Name }).ToList(),
                    Comments = d.Comments.OrderBy(c => c.CreatedDate).ThenBy(c => c.ID).Select(c => new CommentViewModel
                    {
                        DismissalCaseId = c.DismissalCaseID,
                        Text = c.Text,
                        UserName = c.UserName,
                        CreatedDate = c.CreatedDate
                    }).ToList()
                })
                .SingleOrDefault();
            return model;
        }

        public void SaveNotes(NoteModel model)
        {
            Load(model.DismissalCaseId).UpdateNotes(model.Notes);
            db.SaveChanges();
        }

        public void MarkAnswered(int caseId)
        {
            Load(caseId).MarkAnswered();
            db.SaveChanges();
        }

        public void Evaluate(EvaluationViewModel model)
        {
            Load(model.DismissalCaseId, withTags: true).Evaluate((model.Tags ?? []).Select(t => t.ID));
            db.SaveChanges();
        }

        public void AddTag(TagLinkModel model)
        {
            Load(model.DismissalCaseID, withTags: true).AddTag(model.TagID);
            db.SaveChanges();
        }

        public void RemoveTag(TagLinkModel model)
        {
            Load(model.DismissalCaseID, withTags: true).RemoveTag(model.TagID);
            db.SaveChanges();
        }

        public ApplicantNotification Answer(int caseId, string text, string staffUserName)
        {
            var dismissalCase = Load(caseId, withApplicant: true);
            string token = dismissalCase.AnswerByStaff(text, staffUserName, time.GetUtcNow().UtcDateTime, _linkOptions.Lifetime);
            db.SaveChanges();
            return new ApplicantNotification(dismissalCase.Applicant.Email, token);
        }

        public IReadOnlyList<LookupIDNamePair> GetCategories() =>
            db.Category.OrderBy(c => c.ID).Select(c => new LookupIDNamePair { ID = c.ID, Name = c.Name }).ToList();

        public IReadOnlyList<LookupIDNamePair> GetTags(string search = null)
        {
            IQueryable<Tag> tags = db.Tag;
            if (!string.IsNullOrEmpty(search)) tags = tags.Where(t => search.Contains(t.Name) || t.Name.Contains(search));
            return tags.OrderBy(t => t.ID).Select(t => new LookupIDNamePair { ID = t.ID, Name = t.Name }).ToList();
        }

        // ---- Applicant (secret link token) --------------------------------------------------------

        public DismissalCaseViewModel GetApplicantView(string token)
        {
            byte[] hash = ApplicantAccessToken.TryHash(token);
            if (hash == null) return null;
            int? caseId = db.DismissalCase.Where(DismissalCase.AccessibleWith(hash, time.GetUtcNow().UtcDateTime))
                .Select(d => (int?)d.ID).SingleOrDefault();
            if (caseId == null) return null;

            var model = db.DismissalCase
                .Where(d => d.ID == caseId)
                .Select(d => new DismissalCaseViewModel
                {
                    Id = d.ID,
                    AdditionalInfo = d.AdditionalInfo,
                    AgeInterval = d.Applicant.AgeInterval,
                    CityId = d.CityID,
                    CityName = d.City.Name,
                    CompanyDescription = d.CompanyDescription,
                    CompanySectorName = d.CompanySectorName,
                    EmployeeCount = d.EmployeeCount,
                    DeclaredTerminationReasonId = d.DeclaredTerminationReasonID,
                    DeclaredTerminationReasonName = d.DeclaredTerminationReason != null ? d.DeclaredTerminationReason.Name : "",
                    SGKTerminationReasonId = d.SGKTerminationReasonID,
                    SGKTerminationReasonName = d.SGKTerminationReason != null ? d.SGKTerminationReason.Code + " - " + d.SGKTerminationReason.Name : "",
                    Description = d.CompanyDescription,
                    DetailedExperience = d.DetailedExperience,
                    EmailAddress = d.Applicant.Email,
                    DismissalState = d.DismissalState,
                    EducationState = d.Applicant.EducationalState,
                    FullName = d.Applicant.FullName,
                    EmploymentDurationSince = d.EmploymentDurationSince,
                    EmploymentTerminationDate = d.EmploymentTerminationDate,
                    Gender = d.Applicant.Gender,
                    PhoneNumber = d.Applicant.PhoneNumber,
                    SignedDocuments = d.SignedDocuments,
                    WantShareHisExperienceString = d.WantShareHisExperience ? "Evet" : "Hayır",
                    SupportedByNames = d.Supporters.Select(s => s.SupportedBy.Name).ToList(),
                    // Notes are staff-only and are deliberately not exposed to the applicant.
                })
                .Single();

            // userView.html posts the token back with replies, in the Guid field.
            model.Guid = token;
            // The conversation spans every case sharing this case's Guid: a double-clicked apply form used to store
            // several cases with one Guid (74 in production).
            string caseGuid = db.DismissalCase.Where(d => d.ID == caseId).Select(d => d.Guid).Single();
            model.Comments = db.Comment
                .Where(c => c.DismissalCaseID == caseId || (caseGuid != null && db.DismissalCase.Any(d => d.ID == c.DismissalCaseID && d.Guid == caseGuid)))
                .OrderBy(c => c.CreatedDate).ThenBy(c => c.ID)
                .Select(c => new CommentViewModel { DismissalCaseId = c.DismissalCaseID, Text = c.Text, UserName = c.UserName, CreatedDate = c.CreatedDate })
                .ToList();
            return model;
        }

        public CommentDto AddApplicantReply(string token, string text)
        {
            DateTime now = time.GetUtcNow().UtcDateTime;
            var dismissalCase = FindByToken(token, now, withApplicant: true);
            if (dismissalCase == null) return null;

            var comment = dismissalCase.ReplyAsApplicant(token, text, now);
            db.SaveChanges();
            return new CommentDto { ID = comment.ID, DismissalCaseID = comment.DismissalCaseID, Text = comment.Text, UserName = comment.UserName, CreatedDate = comment.CreatedDate };
        }

        // ---- helpers ------------------------------------------------------------------------------

        private DismissalCase Load(int id, bool withTags = false, bool withApplicant = false)
        {
            IQueryable<DismissalCase> query = db.DismissalCase;
            if (withTags) query = query.Include(d => d.Tags);
            if (withApplicant) query = query.Include(d => d.Applicant);
            return query.SingleOrDefault(d => d.ID == id) ?? throw new InvalidOperationException($"Dismissal case {id} not found.");
        }

        private DismissalCase FindByToken(string token, DateTime utcNow, bool withApplicant)
        {
            byte[] hash = ApplicantAccessToken.TryHash(token);
            if (hash == null) return null;
            IQueryable<DismissalCase> query = db.DismissalCase;
            if (withApplicant) query = query.Include(d => d.Applicant);
            return query.SingleOrDefault(DismissalCase.AccessibleWith(hash, utcNow));
        }
    }
}
