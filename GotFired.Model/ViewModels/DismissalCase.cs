using GotFired.Model.Entities;
using GotFired.Model.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GotFired.Model.ViewModels
{
    public class DismissalCaseApplyModel
    {
        public string Guid { get; set; }
        public int CityID { get; set; }
        [DataType(DataType.Date)]
        public DateTime? EmploymentTerminationDate { get; set; }
        public int? CompanySectorID { get; set; }
        public int? DeclaredTerminationReasonID { get; set; }
        public int? SGKTerminationReasonID { get; set; }
        public string OtherDeclaredTerminationReason { get; set; }
        public int? CategoryID { get; set; }
        public EmploymentDurationSince EmploymentDurationSince { get; set; }
        public EmployeeCount EmployeeCount { get; set; }
        public string CompanySectorName { get; set; }
        public string CompanyDescription { get; set; }
        public bool HasSignedAnyDocument { get; set; }
        public string SignedDocuments { get; set; }
        public DismissalState DismissalState { get; set; }
        public bool WantShareHisExperience { get; set; }
        public string DetailedExperience { get; set; }
        public string AdditionalInfo { get; set; }
        public AppealState AppealState { get; set; }
        public int[] SupportedByIds { get; set; }

        public AgeInterval? ApplicantAgeInterval { get; set; }
        public EducationState? ApplicantEducationalState { get; set; }
        public Gender ApplicantGender { get; set; }
        [EmailAddress]
        [Required]
        [Display(Name ="Email")]
        public string ApplicantEmail { get; set; }

        public string ApplicantPhoneNumber { get; set; }
        public string ApplicantFullName { get; set; }
        public int? ApplicantPositionId { get; set; }
    }
    public class DismissalCaseListModel
    {
        public int Id { get; set; }
        public DateTime CreatedDate{ get; set; }
        public DateTime CommentDate { get; set; }
        //public DismissalState DismissalState { get; set; }
        public string ApplicantFullName { get; set; }
        public string ApplicantEmail { get; set; }
        public AppealState AppealState { get; set; }
        public string CommentedBy { get; set; }
        //public string DismissalStateVal { get { return DismissalState.GetDisplayName(); } }
        //public string AppealStateVal { get { return AppealState.GetDisplayName(); } }
    }
    public class DismissalCaseViewModel
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public string FullName { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string AgeIntervalName { get { return AgeInterval.GetDisplayName(); } }
        public string GenderName { get { return Gender.GetDisplayName(); } }
        public string EducationStateName { get { return EducationState.GetDisplayName(); } }
        public int? PositionId { get; set; }
        public DateTime? EmploymentTerminationDate { get; set; }
        public int CityId { get; set; }
        public string EmploymentDurationSinceName { get { return EmploymentDurationSince.GetDisplayName(); } }
        public string EmployeeCountName { get { return EmployeeCount.GetDisplayName(); } }
        public int? CompanySectorId { get; set; }
        public string CompanyDescription { get; set; }
        public int? DeclaredTerminationReasonId { get; set; }
        public string DeclaredTerminationReasonName { get; set; }
        public int? SGKTerminationReasonId { get; set; }
        public string SGKTerminationReasonName { get; set; }
        public bool HasSignedAnyDocument { get; set; }
        public string SignedDocuments { get; set; }
        public string DismissalStateName { get { return DismissalState.GetDisplayName(); } }
        public IEnumerable<string> SupportedByNames { get; set; }
        public string SupportedByName { get; set; }
        public bool WantShareHisExperience { get; set; }
        public string WantShareHisExperienceString { get; set; }
        public string DetailedExperience { get; set; }
        public string AdditionalInfo { get; set; }
        public int? CategoryId { get; set; }
        public string Description { get; set; }
        public bool IsConsulted { get; set; }
        public int ApplicantId { get; set; }
        public string CityName { get; set; }
        public string CompanySectorName { get; set; }
        public string Notes { get; set; }
        public IEnumerable<LookupIDNamePair> DismissalCaseTags { get; set; }

        public AgeInterval? AgeInterval { get; set; }
        public Gender Gender { get; set; }
        public EducationState? EducationState { get; set; }
        public EmploymentDurationSince EmploymentDurationSince { get; set; }
        public EmployeeCount EmployeeCount { get; set; }
        public DismissalState DismissalState { get; set; }
        

        public ICollection<LookupIDNamePair> Cities { get; set; }
        public ICollection<LookupIDNamePair> CompanySectors { get; set; }
        public ICollection<LookupIDNamePair> DeclaredTerminationReasons { get; set; }
        public IEnumerable<CommentViewModel> Comments { get; set; }
    }
    public class DismissalCaseId
    {
        public int Id { get; set; }
    }
    public class NoteModel
    {
        public int DismissalCaseId { get; set; }
        public string Notes { get; set; }
    }
    public class CommentViewModel
    {
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }
        public bool IsConsultant { get; set; }
        //public DateTime SentAt { get; set; }

        public int DismissalCaseId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Guid { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class LookupIDNamePair
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class EvaluationViewModel
    {
        public int DismissalCaseId { get; set; }
        public int CategoryId { get; set; }
        public IEnumerable<LookupIDNamePair> Tags { get; set; }
    }
}

