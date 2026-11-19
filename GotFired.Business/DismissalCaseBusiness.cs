using GotFired.DAL;
using GotFired.Model;
using System;
using System.Collections.Generic;
using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Entities.Enums;
using GotFired.Model.ViewModels;
using System.Linq;
using Microsoft.Extensions.Options;

namespace GotFired.Business
{
    public class DismissalCaseBusiness : IDismissalCaseBusiness
    {
        readonly UnitOfWork _unitOfWork;
        readonly ApplicantLinkOptions _linkOptions;

        public DismissalCaseBusiness(UnitOfWork unitOfWork, IOptions<ApplicantLinkOptions> linkOptions)
        {
            _unitOfWork = unitOfWork;
            _linkOptions = linkOptions.Value;
        }

        public IEnumerable<DismissalCaseListModel> GetDismissalCaseListModel(int page, int pageSize)
        {
            return _unitOfWork.dismissalCaseRepository.GetDismissalCaseListModel(page, pageSize);
        }
        public IEnumerable<DismissalCaseListModel> GetDismissalCaseListModelByStatus(AppealState status, int page, int pageSize)
        {
            return _unitOfWork.dismissalCaseRepository.GetDismissalCasesByStatus(status, page, pageSize);
        }
        public DismissalCaseViewModel GetDismissalCaseById(int id)
        {
            return _unitOfWork.dismissalCaseRepository.GetDismissalCaseById(id);
        }
        /// <summary>
        /// The applicant's own view of their case, for a valid unexpired link token; null otherwise.
        /// The token is echoed back in <c>Guid</c> because userView.html posts that field with replies.
        /// </summary>
        public DismissalCaseViewModel GetApplicantView(string token)
        {
            int? caseId = FindCaseByToken(token);
            if (caseId == null) return null;

            var view = _unitOfWork.dismissalCaseRepository.GetApplicantView(caseId.Value);
            view.Guid = token;
            return view;
        }

        public string GetInitializer()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Stores the applicant's reply on the case their link token belongs to and marks it pending.
        /// The case and the author name come from the token, never from the request. Null for an invalid token.
        /// </summary>
        public Comment AddApplicantReply(string token, string text)
        {
            int? caseId = FindCaseByToken(token);
            if (caseId == null) return null;

            var dismissalCase = _unitOfWork.dismissalCaseRepository.GetByID(caseId.Value);
            dismissalCase.AppealState = AppealState.Pending;
            _unitOfWork.dismissalCaseRepository.Update(dismissalCase);

            var comment = new Comment
            {
                DismissalCaseID = dismissalCase.ID,
                Text = text,
                UserName = _unitOfWork.ApplicantRepository.GetByID(dismissalCase.ApplicantId)?.FullName,
            };
            _unitOfWork.CommentRepository.Add(comment);
            _unitOfWork.Save();
            return comment;
        }

        /// <summary>
        /// Stores a staff answer, marks the case answered and issues a new applicant link token (replacing the
        /// previous one). Returns where to email the link; the caller sends it after this has been saved.
        /// </summary>
        public ApplicantNotification AnswerCase(int caseId, string text, string staffUserName)
        {
            var dismissalCase = _unitOfWork.dismissalCaseRepository.GetByID(caseId)
                                ?? throw new InvalidOperationException($"Dismissal case {caseId} not found.");
            dismissalCase.AppealState = AppealState.Answered;

            var (token, hash) = ApplicantAccessToken.Create();
            dismissalCase.AccessTokenHash = hash;
            dismissalCase.AccessTokenExpiresAt = DateTime.UtcNow.Add(_linkOptions.Lifetime);
            _unitOfWork.dismissalCaseRepository.Update(dismissalCase);

            _unitOfWork.CommentRepository.Add(new Comment
            {
                DismissalCaseID = caseId,
                Text = text,
                UserName = staffUserName,
            });
            _unitOfWork.Save();

            var email = _unitOfWork.ApplicantRepository.GetByID(dismissalCase.ApplicantId)?.Email;
            return new ApplicantNotification(email, token);
        }

        private int? FindCaseByToken(string token)
        {
            byte[] hash = ApplicantAccessToken.TryHash(token);
            return hash == null ? null : _unitOfWork.dismissalCaseRepository.FindIdByAccessToken(hash, DateTime.UtcNow);
        }

        public void UpdateDismissalCaseByNotes(NoteModel noteModel)
        {
            var dismissalCase = _unitOfWork.dismissalCaseRepository.GetByID(noteModel.DismissalCaseId);
            dismissalCase.Notes = noteModel.Notes;
            _unitOfWork.dismissalCaseRepository.Update(dismissalCase);
            _unitOfWork.Save();
        }
        public void UpdateDismissalCaseByAppealState(int dismissalCaseId)
        {
            var dismissalCase = _unitOfWork.dismissalCaseRepository.GetByID(dismissalCaseId);
            dismissalCase.AppealState = AppealState.Answered;
            _unitOfWork.dismissalCaseRepository.Update(dismissalCase);
            _unitOfWork.Save();
        }
        
        public void AddDismissalCase(DismissalCaseApplyModel model)
        {
            // The Guid is issued by GET apply when the form loads; a second POST with the same Guid is a
            // double-clicked submit. Accept it without storing a duplicate case.
            if (!string.IsNullOrEmpty(model.Guid) && _unitOfWork.dismissalCaseRepository.ExistsByGuid(model.Guid))
            {
                return;
            }
            DismissalCase dismissalCase = new DismissalCase
            {
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
                Guid = model.Guid,
                OtherDeclaredTerminationReason = model.OtherDeclaredTerminationReason,
                SignedDocuments = model.SignedDocuments,
                WantShareHisExperience = model.WantShareHisExperience
            };
            if (model.SupportedByIds != null)
            {
                dismissalCase.DismissalCaseSupprtedBys = new List<DismissalCaseSupprtedBy>();
                foreach (var item in model.SupportedByIds)
                {
                    dismissalCase.DismissalCaseSupprtedBys.Add(new DismissalCaseSupprtedBy { SupportedById = item });
                }
            }
            
            _unitOfWork.dismissalCaseRepository.Add(dismissalCase);
            _unitOfWork.Save();
        }

        public int GetAllDismissalCasesCount()
        {
            return _unitOfWork.dismissalCaseRepository.GetCount();
        }
        public int GetAllDismissalCasesCountByStatus(AppealState appealState)
        {
            return _unitOfWork.dismissalCaseRepository.GetCountByAppealState(appealState);
        }


        public void AddComment(Comment model)
        {
            _unitOfWork.CommentRepository.Add(model);
            _unitOfWork.Save();
        }
        public IEnumerable<City> GetCities()
        {
            return _unitOfWork.CityRepository.GetAll();
        }
        public IEnumerable<CompanySector> GetCompanySectors()
        {
            return _unitOfWork.CompanySectorRepository.GetAll();
        }
        public IEnumerable<DeclaredTerminationReason> GetDeclaredTerminationReasons()
        {
            return _unitOfWork.DeclaredTerminationReasonRepository.GetAll();
        }
        public IEnumerable<SGKTerminationReason> GetSGKTerminationReasons()
        {
            return _unitOfWork.SGKTerminationReasonRepository.GetAll();
        }
        public IEnumerable<SupportedBy> GetSupportedBys()
        {
            return _unitOfWork.SupportedByRepository.GetAll();
        }
        
        public IEnumerable<LookupIDNamePair> GetAnswerTemplatesLookup()
        {
            return _unitOfWork.AnswerTemplateRepository.GetLookup();
        }
        public AnswerTemplate GetAnswerTemplateContent(int id)
        {
            return _unitOfWork.AnswerTemplateRepository.GetByID(id);
        }

        public IEnumerable<Category> GetCategories()
        {
            return _unitOfWork.CategoryRepository.GetAll();
        }

        public IEnumerable<Tag> GetTags()
        {
            return _unitOfWork.TagRepository.GetAll();
        }

        public IEnumerable<Tag> GetTags(string searchText)
        {
            return _unitOfWork.TagRepository.GetTagBySearchText(searchText);
        }

        public void Evaluate(EvaluationViewModel model)
        {
            //var dismissalCase = _unitOfWork.dismissalCaseRepository.GetByID(model.DismissalCaseId);
            //dismissalCase.CategoryID = model.CategoryId;

            _unitOfWork.DismissalCaseTagRepository.RemoveAll(model.DismissalCaseId);

            foreach (var tag in model.Tags)
            {
                _unitOfWork.DismissalCaseTagRepository.Add(
                    new DismissalCaseTag
                    {
                        DismissalCaseID = model.DismissalCaseId,
                        TagID = tag.ID
                    });
            }
            //foreach (var tag in model.Tags.Where(t => t.Id == 0))
            //{
            //    _unitOfWork.TagRepository.Add(new Tag { Name = tag.Name });

            //}
            _unitOfWork.Save();

            //var newTagIds = _unitOfWork.TagRepository.GetTagIds(model.Tags.Where(t => t.Id == 0).Select(s => s.Name).ToArray());

            //for (int i=0; i<newTagIds.Length; i++)
            //{
            //    _unitOfWork.DismissalCaseTagRepository.Add(
            //        new DismissalCaseTag
            //        {
            //            DismissalCaseID = model.DismissalCaseId,
            //            TagID = newTagIds[i]
            //        });
            //}
            //_unitOfWork.Save();
        }

        public void AddDismissalCaseTag(DismissalCaseTag model)
        {
            _unitOfWork.DismissalCaseTagRepository.Add(model);
            _unitOfWork.Save();
        }
        public void RemoveDismissalCaseTag(DismissalCaseTag model)
        {
            var dismissalCaseTag = _unitOfWork.DismissalCaseTagRepository.GetFirst(t => t.TagID == model.TagID && t.DismissalCaseID == model.DismissalCaseID);
            _unitOfWork.DismissalCaseTagRepository.Delete(dismissalCaseTag);
            _unitOfWork.Save();
        }
    }

    /// <summary>Applicant email address and the new link token to send after a staff answer.</summary>
    public record ApplicantNotification(string Email, string Token);
}
