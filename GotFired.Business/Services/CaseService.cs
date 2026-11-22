using GotFired.DAL;
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

            var dismissalCase = new DismissalCase
            {
                Guid = string.IsNullOrEmpty(model.Guid) ? NewApplicationGuid() : model.Guid,
                AdditionalInfo = model.AdditionalInfo,
                AppealState = model.AppealState,
                Applicant = new Applicant
                {
                    AgeInterval = model.ApplicantAgeInterval,
                    EducationalState = model.ApplicantEducationalState,
                    FullName = model.ApplicantFullName,
                    Email = model.ApplicantEmail,
                    Gender = model.ApplicantGender,
                    PhoneNumber = model.ApplicantPhoneNumber,
                    PositionId = model.ApplicantPositionId
                },
                CityID = model.CityID,
                CompanyDescription = model.CompanyDescription,
                CompanySectorName = model.CompanySectorName,
                DeclaredTerminationReasonID = model.DeclaredTerminationReasonID,
                SGKTerminationReasonID = model.SGKTerminationReasonID,
                DetailedExperience = model.DetailedExperience,
                DismissalState = model.DismissalState,
                EmployeeCount = model.EmployeeCount,
                EmploymentDurationSince = model.EmploymentDurationSince,
                EmploymentTerminationDate = model.EmploymentTerminationDate,
                OtherDeclaredTerminationReason = model.OtherDeclaredTerminationReason,
                SignedDocuments = model.SignedDocuments,
                WantShareHisExperience = model.WantShareHisExperience,
                DismissalCaseSupportedBys = (model.SupportedByIds ?? [])
                    .Select(id => new DismissalCaseSupportedBy { SupportedById = id })
                    .ToList()
            };
            db.DismissalCase.Add(dismissalCase);
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
                    SupportedByNames = d.DismissalCaseSupportedBys.Select(s => s.SupportedBy.Name).ToList(),
                    Notes = d.Notes,
                    CategoryId = d.CategoryID,
                    DismissalCaseTags = d.DismissalCaseTags.Select(t => new LookupIDNamePair { ID = t.TagID, Name = t.Tag.Name }).ToList(),
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
            FindCase(model.DismissalCaseId).Notes = model.Notes;
            db.SaveChanges();
        }

        public void MarkAnswered(int caseId)
        {
            FindCase(caseId).AppealState = AppealState.Answered;
            db.SaveChanges();
        }

        public void Evaluate(EvaluationViewModel model)
        {
            db.DismissalCaseTag.RemoveRange(db.DismissalCaseTag.Where(t => t.DismissalCaseID == model.DismissalCaseId));
            foreach (var tag in model.Tags ?? [])
            {
                db.DismissalCaseTag.Add(new DismissalCaseTag { DismissalCaseID = model.DismissalCaseId, TagID = tag.ID });
            }
            db.SaveChanges();
        }

        public void AddTag(TagLinkModel model)
        {
            db.DismissalCaseTag.Add(new DismissalCaseTag { DismissalCaseID = model.DismissalCaseID, TagID = model.TagID });
            db.SaveChanges();
        }

        public void RemoveTag(TagLinkModel model)
        {
            db.DismissalCaseTag.Remove(db.DismissalCaseTag.First(t => t.DismissalCaseID == model.DismissalCaseID && t.TagID == model.TagID));
            db.SaveChanges();
        }

        public ApplicantNotification Answer(int caseId, string text, string staffUserName)
        {
            var dismissalCase = FindCase(caseId);
            dismissalCase.AppealState = AppealState.Answered;

            var (token, hash) = ApplicantAccessToken.Create();
            dismissalCase.AccessTokenHash = hash;
            dismissalCase.AccessTokenExpiresAt = time.GetUtcNow().UtcDateTime.Add(_linkOptions.Lifetime);

            db.Comment.Add(new Comment { DismissalCaseID = caseId, Text = text, UserName = staffUserName });
            db.SaveChanges();

            string email = db.Applicant.Where(a => a.ID == dismissalCase.ApplicantId).Select(a => a.Email).SingleOrDefault();
            return new ApplicantNotification(email, token);
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
            int? caseId = FindCaseIdByToken(token);
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
                    SupportedByNames = d.DismissalCaseSupportedBys.Select(s => s.SupportedBy.Name).ToList(),
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
            int? caseId = FindCaseIdByToken(token);
            if (caseId == null) return null;

            var dismissalCase = FindCase(caseId.Value);
            dismissalCase.AppealState = AppealState.Pending;
            var comment = new Comment
            {
                DismissalCaseID = dismissalCase.ID,
                Text = text,
                // The author is the applicant on file, never a name sent by the browser.
                UserName = db.Applicant.Where(a => a.ID == dismissalCase.ApplicantId).Select(a => a.FullName).SingleOrDefault(),
            };
            db.Comment.Add(comment);
            db.SaveChanges();

            return new CommentDto { ID = comment.ID, DismissalCaseID = comment.DismissalCaseID, Text = comment.Text, UserName = comment.UserName, CreatedDate = comment.CreatedDate };
        }

        // ---- helpers ------------------------------------------------------------------------------

        private DismissalCase FindCase(int id) =>
            db.DismissalCase.Find(id) ?? throw new InvalidOperationException($"Dismissal case {id} not found.");

        private int? FindCaseIdByToken(string token)
        {
            byte[] hash = ApplicantAccessToken.TryHash(token);
            if (hash == null) return null;
            DateTime now = time.GetUtcNow().UtcDateTime;
            return db.DismissalCase
                .Where(d => d.AccessTokenHash == hash && d.AccessTokenExpiresAt > now)
                .Select(d => (int?)d.ID)
                .SingleOrDefault();
        }
    }
}
