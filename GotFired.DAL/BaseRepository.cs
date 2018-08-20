using GotFired.Model.Entities;
using GotFired.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.DAL
{
    public abstract class BaseRepository<T> : IRepository<T> where T:class
    {
        protected readonly GotFiredDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(GotFiredDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public void Add(T model)
        {
            _dbSet.Add(model);
        }
       
        public void Delete(object id)
        {
            T entityToDelete = _dbSet.Find(id);
            Delete(entityToDelete);
        }
        public void Delete(T entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }
            _dbSet.Remove(entityToDelete);
        }
        public IEnumerable<T> GetAll()
        {
            return _dbSet;
        }
        public T GetByID(object id)
        {
            return _dbSet.Find(id);
        }
        public T GetFirst(Func<T, bool> predicate)
        {
            return _dbSet.First<T>(predicate);
        }
        public void SetState(T stateToModel, EntityState state)
        {
            _context.Entry<T>(stateToModel).State = state;
        }
        public void Update(T model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model", "Hata Kodu:C102");
            }
            _context.Entry(model).State = System.Data.Entity.EntityState.Modified;
            string rowCheck1 = _context.Entry(model).GetDatabaseValues().GetValue<string>("RowVersion");
            string rowCheck2 = ((IRowVersion)model).RowVersion;
            if (rowCheck1 != rowCheck2)
            {
                throw new DbUpdateConcurrencyException("Başka bir kullanıcı güncelleme yapmakta,lütfen daha sonra tekrar deneyin");
            }
            ((IRowVersion)model).RowVersion = rowCheck2.Replace(rowCheck2.Substring(8, 8), (Convert.ToInt32(rowCheck2.Substring(8, 8)) + 1).ToString());
            ((IBaseEntity)model).EditedDate = DateTime.Now;
            //db.Logging.Add(new Logging
            //{
            //    CreatedDate = DateTime.Now,
            //    Creator = ((IBaseEntity)model).Creator,
            //    FullName = model.GetType().FullName,
            //    LoggingType = LoggingType.Info,
            //    ManagedThread = System.Threading.Thread.CurrentThread.ManagedThreadId,
            //    Message1 = "Update Process",
            //});
        }
        public int GetCount()
        {
            return _dbSet.Count();
        }
    }
}
