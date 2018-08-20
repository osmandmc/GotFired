using GotFired.DAL;
using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Entities.Enums;
using GotFired.Model.Interfaces;
using GotFired.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace GotFired.DAL.Repositories
{
    public class DismissalCaseRepository : BaseRepository<DismissalCase>, IDismissalCaseRepository
    {
        public DismissalCaseRepository(GotFiredDbContext context) : base(context)
        {
        }

        public IEnumerable<DismissalCaseListModel> GetDismissalCaseListModel(int page, int pageSize)
        {
            var model = _dbSet.Include(d=>d.Applicant).Include(d=>d.Comments).OrderByDescending(d => d.EditedDate).AsEnumerable().Skip((page - 1) * pageSize).Take(pageSize).
                Select(d => new DismissalCaseListModel
                {
                    Id = d.ID,
                    AppealState = d.AppealState,
                    ApplicantEmail = d.Applicant.Email,
                    ApplicantFullName = d.Applicant.FullName,
                    CreatedDate = d.CreatedDate,
                    CommentDate = d.EditedDate,
                    CommentedBy = d.Comments.OrderByDescending(c=>c.CreatedDate).FirstOrDefault() != null ? d.Comments.OrderByDescending(c => c.CreatedDate).FirstOrDefault().UserName : string.Empty
                }).ToList();
            return model;
        }

        public IEnumerable<DismissalCaseListModel> GetDismissalCasesByStatus(AppealState appealState, int page, int pageSize)
        {
            var model = _dbSet.Include(d => d.Applicant).Include(d => d.Comments).OrderByDescending(d => d.EditedDate)
               .Where(d=>d.AppealState == appealState)
               .AsEnumerable().Skip((page - 1) * pageSize).Take(pageSize)
               .Select(d => new DismissalCaseListModel
               {
                   Id = d.ID,
                   AppealState = d.AppealState,
                   ApplicantEmail = d.Applicant.Email,
                   ApplicantFullName = d.Applicant.FullName,
                   CreatedDate = d.CreatedDate,
                   CommentDate = d.EditedDate,
                   CommentedBy = d.Comments.OrderByDescending(c => c.CreatedDate).FirstOrDefault() != null ? d.Comments.OrderByDescending(c => c.CreatedDate).FirstOrDefault().UserName : string.Empty
               })
               .OrderByDescending(d=>d.CommentDate)
               .ToList();
            return model;
        }
        public DismissalCaseViewModel GetDismissalCaseById(int id)
        {
            var model = _dbSet
                .Include(d => d.Applicant)
                .Include(d => d.Comments)
                .Include(d => d.City)
                .Include(d => d.DismissalCaseSupprtedBys.Select(s => s.SupportedBy))
                .Include(d => d.DeclaredTerminationReason)
                .Include(d => d.SGKTerminationReason)
                .Include(d => d.CompanySector)
                .Include(d => d.DismissalCaseTags.Select(s => s.Tag))
                .Where(d => d.ID == id)
                .Select(d => new DismissalCaseViewModel
                {
                    Guid = d.Guid.ToString(),
                    AdditionalInfo = d.AdditionalInfo,
                    AgeInterval = d.Applicant.AgeInterval,
                    CityId = d.CityID,
                    CityName = d.City.Name,
                    CompanyDescription = d.CompanyDescription,
                    CompanySectorName = d.CompanySectorName,
                    EmployeeCount = d.EmployeeCount,
                    DeclaredTerminationReasonId = d.DeclaredTerminationReasonID,
                    DeclaredTerminationReasonName = d.DeclaredTerminationReason != null ? d.DeclaredTerminationReason.Name : "",
                    SGKTerminationReasonId = d.DeclaredTerminationReasonID,
                    SGKTerminationReasonName = d.DeclaredTerminationReason != null ? d.SGKTerminationReason.Name : "",
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
                    SupportedByNames = d.DismissalCaseSupprtedBys.Select(s => s.SupportedBy.Name),
                    Notes = d.Notes,
                    CategoryId = d.CategoryID,
                    DismissalCaseTags = d.DismissalCaseTags.Select(l=> new LookupIDNamePair { ID = l.TagID, Name = l.Tag.Name}),
                    Comments = d.Comments.Select(c=> new CommentViewModel {
                        DismissalCaseId = c.DismissalCaseID,
                        Text = c.Text,
                        UserName = c.UserName,
                        CreatedDate = c.CreatedDate
                    })
                }).SingleOrDefault();
            return model;
        }
        public DismissalCaseViewModel GetDismissalCaseByGuid(string guid)
        {
            var model = _dbSet
                .Include(d => d.Applicant)
                .Include(d => d.Comments)
                .Include(d => d.City)
                .Include(d => d.DismissalCaseSupprtedBys.Select(s => s.SupportedBy))
                .Include(d => d.DeclaredTerminationReason)
                .Include(d => d.SGKTerminationReason)
                .Include(d => d.CompanySector)
                .Where(d => d.Guid == guid)
                .Select(d => new DismissalCaseViewModel
                {
                    Id = d.ID,
                    Guid = d.Guid.ToString(),
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
                    SGKTerminationReasonName = d.SGKTerminationReason != null ? d.SGKTerminationReason.Code + " - " +d.SGKTerminationReason.Name : "",
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
                    SupportedByNames = d.DismissalCaseSupprtedBys.Select(s => s.SupportedBy.Name),
                    Notes = d.Notes,
                    Comments = d.Comments.Select(c => new CommentViewModel
                    {
                        DismissalCaseId = c.DismissalCaseID,
                        Text = c.Text,
                        UserName = c.UserName,
                        CreatedDate = c.CreatedDate
                    })
                }).SingleOrDefault();
            return model;
        }
        public int GetCountByAppealState(AppealState appealState)
        {
            return _dbSet.Where(d => d.AppealState == appealState).Count();
        }
      
    }
}
