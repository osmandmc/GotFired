using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using GotFired.Model.Entities;
using GotFired.Model.Entities.Enums;

namespace GotFired.Api.Features.Cases
{
    /// <summary>
    /// What the applicant reported, as shown to staff and to the applicant: the base of both responses (the pages
    /// read these fields at the top level). Read-side projection.
    /// </summary>
    public record CaseFacts
    {
        public string FullName { get; init; }
        public string EmailAddress { get; init; }
        public string PhoneNumber { get; init; }
        public string AgeIntervalName { get; init; }
        public string GenderName { get; init; }
        public string EducationStateName { get; init; }
        public string CityName { get; init; }
        public string CompanySectorName { get; init; }
        public string CompanyDescription { get; init; }
        public string DeclaredTerminationReasonName { get; init; }
        public string SGKTerminationReasonName { get; init; }
        public string EmploymentDurationSinceName { get; init; }
        public string EmployeeCountName { get; init; }
        public DateTime? EmploymentTerminationDate { get; init; }
        public string DismissalStateName { get; init; }
        public string SignedDocuments { get; init; }
        public string WantShareHisExperienceString { get; init; }
        public string DetailedExperience { get; init; }
        public string AdditionalInfo { get; init; }
        public IReadOnlyList<string> SupportedByNames { get; init; }

        /// <summary>Row read from SQL; enum display names are resolved in memory by <see cref="ToFacts{T}"/>.</summary>
        internal sealed record Row(
            string FullName, string Email, string PhoneNumber, AgeInterval? AgeInterval, Gender Gender, EducationState? EducationState,
            string CityName, string CompanySectorName, string CompanyDescription, string DeclaredReason, string SgkCode, string SgkName,
            EmploymentDurationSince EmploymentDurationSince, EmployeeCount EmployeeCount, DateTime? EmploymentTerminationDate,
            DismissalState DismissalState, string SignedDocuments, bool WantShareHisExperience, string DetailedExperience,
            string AdditionalInfo, List<string> SupportedByNames);

        internal static readonly Expression<Func<DismissalCase, Row>> Select = d => new Row(
            d.Applicant.FullName, d.Applicant.Email, d.Applicant.PhoneNumber, d.Applicant.AgeInterval, d.Applicant.Gender, d.Applicant.EducationalState,
            d.City.Name, d.CompanySectorName, d.CompanyDescription,
            d.DeclaredTerminationReason != null ? d.DeclaredTerminationReason.Name : null,
            d.SGKTerminationReason != null ? d.SGKTerminationReason.Code : null,
            d.SGKTerminationReason != null ? d.SGKTerminationReason.Name : null,
            d.EmploymentDurationSince, d.EmployeeCount, d.EmploymentTerminationDate, d.DismissalState, d.SignedDocuments,
            d.WantShareHisExperience, d.DetailedExperience, d.AdditionalInfo,
            d.Supporters.Select(s => s.SupportedBy.Name).ToList());

        internal static T ToFacts<T>(Row r) where T : CaseFacts, new() => new()
        {
            FullName = r.FullName,
            EmailAddress = r.Email,
            PhoneNumber = r.PhoneNumber,
            AgeIntervalName = Display(r.AgeInterval),
            GenderName = Display(r.Gender),
            EducationStateName = Display(r.EducationState),
            CityName = r.CityName,
            CompanySectorName = r.CompanySectorName,
            CompanyDescription = r.CompanyDescription,
            DeclaredTerminationReasonName = r.DeclaredReason ?? "",
            SGKTerminationReasonName = r.SgkName != null ? $"{r.SgkCode} - {r.SgkName}" : "",
            EmploymentDurationSinceName = Display(r.EmploymentDurationSince),
            EmployeeCountName = Display(r.EmployeeCount),
            EmploymentTerminationDate = r.EmploymentTerminationDate,
            DismissalStateName = Display(r.DismissalState),
            SignedDocuments = r.SignedDocuments,
            WantShareHisExperienceString = r.WantShareHisExperience ? "Evet" : "Hayır",
            DetailedExperience = r.DetailedExperience,
            AdditionalInfo = r.AdditionalInfo,
            SupportedByNames = r.SupportedByNames,
        };

        private static string Display(Enum value) =>
            value == null ? "" : value.GetType().GetMember(value.ToString())[0].GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();
    }
}
