using GotFired.DAL;
using GotFired.DAL.Repositories;
using GotFired.Model;
using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Business
{
    public class ParameterBusiness : IParameterBusiness
    {
        readonly UnitOfWork _unitOfWork;

        public ParameterBusiness()
        {
            _unitOfWork = new UnitOfWork();
        }

        public void CreateAnswerTemplate(AnswerTemplate answerTemplate)
        {
            _unitOfWork.AnswerTemplateRepository.Add(answerTemplate);
            _unitOfWork.Save();
        }

        public AnswerTemplate GetAnswerTemplate(int id)
        {
            return _unitOfWork.AnswerTemplateRepository.GetByID(id);
        }

        public IEnumerable<LookupIDNamePair> GetAnswerTemplateListModel(int page, int pageSize)
        {
            return _unitOfWork.AnswerTemplateRepository.GetListModel(page, pageSize);
        }

        public void UpdateAnswerTemplate(AnswerTemplate answerTemplate)
        {
            _unitOfWork.AnswerTemplateRepository.Update(answerTemplate);
            _unitOfWork.Save();
        }
    }
}
