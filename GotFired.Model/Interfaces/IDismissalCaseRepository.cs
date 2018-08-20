using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Entities.Enums;
using GotFired.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model.Interfaces
{
    public interface IDismissalCaseRepository : IRepository<DismissalCase>
    {
        IEnumerable<DismissalCaseListModel> GetDismissalCaseListModel(int page, int pageSize);
        IEnumerable<DismissalCaseListModel> GetDismissalCasesByStatus(AppealState appealState, int page, int pageSize);
        DismissalCaseViewModel GetDismissalCaseById(int id);
        DismissalCaseViewModel GetDismissalCaseByGuid(string guid);
        int GetCountByAppealState(AppealState appealState);
    }
}
