using GotFired.Model.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GotFired.Model.Entities
{
    public class DismissalCase : BaseEntity
    {
        public string Guid { get; set; }
        public int ApplicantId { get; set; }
        //public string DismissalCaseGuid { get; set; }
        public int CityID { get; set; }
       
        public DateTime? EmploymentTerminationDate { get; set; }
        public int? CompanySectorID { get; set; }
        public int? DeclaredTerminationReasonID { get; set; }
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
        public string Notes { get; set; }
        public ICollection<DismissalCaseSupportedBy> DismissalCaseSupportedBys { get; set; }
        public ICollection<DismissalCaseTag> DismissalCaseTags { get; set; }
        [NotMapped]
        public int[] SupportedByIds { get; set; }
        public int? SGKTerminationReasonID { get; set; }
        /// <summary>SHA-256 of the applicant's secret link token (the token itself is never stored).</summary>
        public byte[] AccessTokenHash { get; set; }
        public DateTime? AccessTokenExpiresAt { get; set; }

        //navigation properties
        public virtual Applicant Applicant { get; set; }
        public virtual City City { get; set; }
        public virtual CompanySector CompanySector { get; set; }
        public virtual DeclaredTerminationReason DeclaredTerminationReason { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public SGKTerminationReason SGKTerminationReason { get; set; }
    }
}
