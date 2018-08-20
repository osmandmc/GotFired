using GotFired.DAL;
using GotFired.Model;
using System;
using System.Collections.Generic;
using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Entities.Enums;
using GotFired.Model.ViewModels;
using System.Linq;

namespace GotFired.Business
{
    public class DismissalCaseBusiness : IDismissalCaseBusiness
    {
        readonly UnitOfWork _unitOfWork;

        public DismissalCaseBusiness()
        {
            _unitOfWork = new UnitOfWork();
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
        public DismissalCaseViewModel GetDismissalCaseByGuid(string guid)
        {
            return _unitOfWork.dismissalCaseRepository.GetDismissalCaseByGuid(guid);

        }


        public string GetInitializer()
        {
            return Guid.NewGuid().ToString();
        }

        public void UpdateDismissalCaseByApplicantAnswer(Comment comment, string guid)
        {
            var dismissalCase = _unitOfWork.dismissalCaseRepository.GetByID(comment.DismissalCaseID);
            dismissalCase.AppealState = AppealState.Pending;

            _unitOfWork.dismissalCaseRepository.Update(dismissalCase);
            _unitOfWork.CommentRepository.Add(comment);
            _unitOfWork.Save();
        }
        public void UpdateDismissalCaseByUserAnswer(CommentViewModel commentViewModel)
        {
            
            var dismissalCase = _unitOfWork.dismissalCaseRepository.GetByID(commentViewModel.DismissalCaseId);
            dismissalCase.AppealState = AppealState.Answered;

            _unitOfWork.dismissalCaseRepository.Update(dismissalCase);
            Comment comment = new Comment
            {
                DismissalCaseID = commentViewModel.DismissalCaseId,
                Text = commentViewModel.Text,
                UserName = commentViewModel.UserName,
            };
            _unitOfWork.CommentRepository.Add(comment);
            _unitOfWork.Save();
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
}
