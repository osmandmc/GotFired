using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.DAL.Repositories
{
    public class DismissalCaseTagRepository : BaseRepository<DismissalCaseTag>, IDismissalCaseTagRepository
    {
        public DismissalCaseTagRepository(GotFiredDbContext context) : base(context)
        {
        }
        public void RemoveAll(int dismissalCaseId)
        {
            var removeTags = _context.DismissalCaseTag.Where(s => s.DismissalCaseID == dismissalCaseId).ToList();
            _context.DismissalCaseTag.RemoveRange(removeTags);
            //_context.SaveChanges();
        }
    }
}
