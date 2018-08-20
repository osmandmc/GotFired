using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model.Entities.DismissalCase
{
    public class SupportedBy : BaseEntity
    {
        public string Name { get; set; }
       // public List<DismissalCase> DismissalCases { get; set; }
        public ICollection<DismissalCaseSupprtedBy> DismissalCaseSupprtedBys { get; set; }

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
