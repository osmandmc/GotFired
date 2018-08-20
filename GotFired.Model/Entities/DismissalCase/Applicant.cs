using GotFired.Model.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model.Entities.DismissalCase
{
    public class Applicant : BaseEntity
    {
        public AgeInterval? AgeInterval { get; set; }
        public EducationState? EducationalState { get; set; }
        public Gender Gender { get; set; }
        [EmailAddress]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public int? PositionId { get; set; }

        public override int Creator
        {
            get
            {
                return base.Creator;
            }

            set
            {
                base.Creator = this.ID;
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
                base.Editor = this.ID;
            }
        }
    }
}
