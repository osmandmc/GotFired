using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model.Entities
{
    public class SupportedBy : BaseEntity
    {
        public string Name { get; set; }
       // public List<DismissalCase> DismissalCases { get; set; }
        public ICollection<DismissalCaseSupportedBy> DismissalCaseSupportedBys { get; set; }


    }
}
