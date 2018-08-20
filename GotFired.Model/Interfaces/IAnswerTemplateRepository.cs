using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model.Interfaces
{
    public interface IAnswerTemplateRepository : IRepository<AnswerTemplate>
    {
        IEnumerable<LookupIDNamePair> GetLookup();
        IEnumerable<LookupIDNamePair> GetListModel(int page, int pageSize);


    }
}
