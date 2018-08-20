using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model.Entities.DismissalCase
{
    public class DismissalCaseTag: BaseEntity
    {
        public int DismissalCaseID { get; set; }
        public int TagID { get; set; }

        public DismissalCase DismissalCase { get; set; }
        public Tag Tag { get; set; }
    }
}
