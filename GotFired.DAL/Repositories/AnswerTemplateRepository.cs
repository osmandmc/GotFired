using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Interfaces;
using GotFired.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.DAL.Repositories
{
    public class AnswerTemplateRepository : BaseRepository<AnswerTemplate>, IAnswerTemplateRepository
    {
        public AnswerTemplateRepository(GotFiredDbContext context) : base(context)
        {
        }
        public IEnumerable<LookupIDNamePair> GetLookup()
        {
            return _context.AnswerTemplate.Select(a => new LookupIDNamePair { ID = a.ID, Name = a.Name }).ToList();
        }
        public IEnumerable<LookupIDNamePair> GetListModel(int page, int pageSize)
        {
            var model = _dbSet.AsEnumerable().Skip((page - 1) * pageSize).Take(pageSize).
               Select(d => new LookupIDNamePair
               {
                   ID = d.ID,
                   Name = d.Name
               }).ToList();
            return model;
        }
        
        
    }
}
