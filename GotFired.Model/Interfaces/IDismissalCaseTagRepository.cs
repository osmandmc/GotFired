using GotFired.Model.Entities.DismissalCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model.Interfaces
{
    public interface IDismissalCaseTagRepository: IRepository<DismissalCaseTag>
    {
        void RemoveAll(int dismissalCaseId);
    }
}
