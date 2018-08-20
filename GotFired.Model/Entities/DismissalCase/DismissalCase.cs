using GotFired.Model.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GotFired.Model.Entities.DismissalCase
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
        public ICollection<DismissalCaseSupprtedBy> DismissalCaseSupprtedBys { get; set; }
        public ICollection<DismissalCaseTag> DismissalCaseTags { get; set; }
        [NotMapped]
        public int[] SupportedByIds { get; set; }
        public int? SGKTerminationReasonID { get; set; }
        public override int Creator
        {
            get
            {
                return base.Creator;
            }

            set
            {
                base.Creator = this.ApplicantId;
            }
        }
        public override int Editor
        {
            get
            {
                return base.Editor;
            }

            set
            {
                base.Editor = this.ApplicantId;
            }
        }

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
