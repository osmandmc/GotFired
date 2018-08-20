using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model.Interfaces
{
    public interface IRepository<T> where T : class
    {
        void Add(T model);
        T GetByID(object id);
        T GetFirst(Func<T, bool> predicate);
        void Update(T model);
        void Delete(T model);
        IEnumerable<T> GetAll();
        int GetCount();
    }
}
