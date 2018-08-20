using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model.Entities.DismissalCase
{
    public class DismissalCaseSupprtedBy: BaseEntity
    {
        public int DismissalCaseId { get; set; }
        public int SupportedById { get; set; }

        public SupportedBy SupportedBy { get; set; }
        public DismissalCase DismissalCase { get; set; }
    }
}
