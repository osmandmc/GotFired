using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Entities.Enums;
using GotFired.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model
{
    public interface IUnitOfWork
    {
        void Save();
    }


    public interface IDismissalCaseBusiness
    {
        //DismissalCase GetDismissalCase(int id);
        int GetAllDismissalCasesCount();

        IEnumerable<DismissalCaseListModel> GetDismissalCaseListModel(int page, int pageSize);
        IEnumerable<DismissalCaseListModel> GetDismissalCaseListModelByStatus(AppealState status, int page, int pageSize);
        DismissalCaseViewModel GetDismissalCaseById(int id);
        DismissalCaseViewModel GetDismissalCaseByGuid(string guid);

        void AddDismissalCase(DismissalCaseApplyModel model);
        string GetInitializer();
        IEnumerable<City> GetCities();
        IEnumerable<CompanySector> GetCompanySectors();
        IEnumerable<DeclaredTerminationReason> GetDeclaredTerminationReasons();
        IEnumerable<Category> GetCategories();
        IEnumerable<Tag> GetTags(string searchText);
        void UpdateDismissalCaseByAppealState(int dismissalCaseId);
        void UpdateDismissalCaseByUserAnswer(CommentViewModel commentViewModel);

        void Evaluate(EvaluationViewModel model);
        void AddDismissalCaseTag(DismissalCaseTag model);
        void RemoveDismissalCaseTag(DismissalCaseTag model);

        IEnumerable<LookupIDNamePair> GetAnswerTemplatesLookup();
        AnswerTemplate GetAnswerTemplateContent(int id);
    }

    public interface IParameterBusiness
    {
        IEnumerable<LookupIDNamePair> GetAnswerTemplateListModel(int page, int pageSize);
        void CreateAnswerTemplate(AnswerTemplate answerTemplate);
        AnswerTemplate GetAnswerTemplate(int id);
        void UpdateAnswerTemplate(AnswerTemplate answerTemplate);
    }
}
