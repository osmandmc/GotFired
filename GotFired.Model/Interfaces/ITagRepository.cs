using GotFired.Model.Entities.DismissalCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model.Interfaces
{
    public interface ITagRepository : IRepository<Tag>
    {
        IEnumerable<Tag> GetTagBySearchText(string searchText);
        int[] GetTagIds(string[] tagsName);
    }
}
