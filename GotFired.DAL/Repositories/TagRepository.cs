using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.DAL.Repositories
{
    public class TagRepository : BaseRepository<Tag>, ITagRepository
    {
        public TagRepository(GotFiredDbContext context) : base(context)
        {
        }

        public IEnumerable<Tag> GetTagBySearchText(string searchText)
        {
            var result = _context.Tag.Where(t => searchText.Contains(t.Name) || t.Name.Contains(searchText)).ToList();
            return result;
        }

        public int[] GetTagIds(string[] tagsName)
        {
            return _context.Tag.Where(t => tagsName.Contains(t.Name)).Select(s => s.ID).ToArray();
        }

        
    }
}
