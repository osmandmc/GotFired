using GotFired.Model.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace GotFired.DAL
{
    public class GenericRepository<T> : BaseRepository<T> where T : class
    {
        internal GotFiredDbContext Context;
        internal DbSet<T> DbSet;
        //public ObservableCollection<T> Local;

        public GenericRepository(GotFiredDbContext context):base(context)
        {
            Context = context;
            DbSet = context.Set<T>();
            //Local = context.Set<T>().Local;
        }



        #region Public member methods
        //public virtual IQueryable<T> GetQueryable()
        //{
        //    return DbSet.AsQueryable<T>();
        //}

        //public virtual IQueryable<T> GetQueryableAsNoTracking()
        //{
        //    return DbSet.AsQueryable<T>().AsNoTracking();
        //}
        ///// <summary>
        ///// generic Get method for Entities.
        ///// Query.ToList()
        ///// </summary>
        ///// <returns></returns>
        //public virtual IEnumerable<T> Get()
        //{
        //    IQueryable<T> query = DbSet;
        //    return query.ToList();
        //}



        /// <summary>
        /// Generic get method on the basis of id for Entities.
        /// DbSet.Find(id)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //public virtual T GetByID(object id)
        //{
        //    return DbSet.Find(id);
        //}

        /// <summary>
        /// generic Insert method for the entities
        /// </summary>
        /// <param name="entity"></param>
        //public virtual void Insert(T model)
        //{
        //    //DbSet.Add(entity);
        //    if (model == null)
        //    {
        //        throw new ArgumentNullException("model", "Hata Kodu:C101");
        //    }

        //    //db.Logging.Add(new Logging
        //    //{
        //    //    CreatedDate = DateTime.Now,
        //    //    Creator = ((IBaseEntity)model).Creator,
        //    //    FullName = model.GetType().FullName,
        //    //    LoggingType = LoggingType.Info,
        //    //    ManagedThread = System.Threading.Thread.CurrentThread.ManagedThreadId,
        //    //    Message1 = "Create Process",
        //    //});
        //    DbSet.Add(model);
        //}

        ///// <summary>
        ///// Generic Delete method for the entities
        ///// </summary>
        ///// <param name="id"></param>
        //public virtual void Delete(object id)
        //{
        //    T entityToDelete = DbSet.Find(id);
        //    Delete(entityToDelete);
        //}

        ///// <summary>
        ///// Generic Delete method for the entities
        ///// </summary>
        ///// <param name="entityToDelete"></param>
        //public virtual void Delete(T entityToDelete)
        //{
        //    if (Context.Entry(entityToDelete).State == EntityState.Detached)
        //    {
        //        DbSet.Attach(entityToDelete);
        //    }
        //    DbSet.Remove(entityToDelete);
        //}
        ///// <summary>
        ///// generic delete method , deletes data for the entities on the basis of condition.
        ///// </summary>
        ///// <param name="where"></param>
        ///// <returns></returns>
        //public void Delete(Func<T, Boolean> where)
        //{
        //    IQueryable<T> objects = DbSet.Where<T>(where).AsQueryable();
        //    foreach (T obj in objects)
        //        DbSet.Remove(obj);
        //}

        ///// <summary>
        ///// Context.Entry<T>(stateToModel).State
        ///// </summary>
        ///// <param name="stateToModel"></param>
        ///// <param name="state"></param>
        //public virtual void SetState(T stateToModel, EntityState state)
        //{
        //    Context.Entry<T>(stateToModel).State = state;
        //}

        ///// <summary>
        ///// Generic update method for the entities
        ///// </summary>
        ///// <param name="entityToUpdate"></param>
        //public virtual void Update(T model)
        //{
        //    //DbSet.Attach(entityToUpdate);
        //    //Context.Entry(entityToUpdate).State = EntityState.Modified;
        //    if (model == null)
        //    {
        //        throw new ArgumentNullException("model", "Hata Kodu:C102");
        //    }
        //    Context.Entry(model).State = System.Data.Entity.EntityState.Modified;
        //    string rowCheck1 = Context.Entry(model).GetDatabaseValues().GetValue<string>("RowVersion");
        //    string rowCheck2 = ((IRowVersion)model).RowVersion;
        //    if (rowCheck1 != rowCheck2)
        //    {
        //        throw new DbUpdateConcurrencyException("Başka bir kullanıcı güncelleme yapmakta,lütfen daha sonra tekrar deneyin");
        //    }
        //    ((IRowVersion)model).RowVersion = rowCheck2.Replace(rowCheck2.Substring(8, 8), (Convert.ToInt32(rowCheck2.Substring(8, 8)) + 1).ToString());
        //    ((IBaseEntity)model).EditedDate = DateTime.Now;
        //    //db.Logging.Add(new Logging
        //    //{
        //    //    CreatedDate = DateTime.Now,
        //    //    Creator = ((IBaseEntity)model).Creator,
        //    //    FullName = model.GetType().FullName,
        //    //    LoggingType = LoggingType.Info,
        //    //    ManagedThread = System.Threading.Thread.CurrentThread.ManagedThreadId,
        //    //    Message1 = "Update Process",
        //    //});
        //}

        ///// <summary>
        ///// generic method to get many record on the basis of a condition.
        ///// DbSet.Where(where).ToList()
        ///// </summary>
        ///// <param name="where"></param>
        ///// <returns></returns>
        //public virtual IEnumerable<T> GetMany(Func<T, bool> where)
        //{
        //    return DbSet.Where(where).ToList();
        //}

        ///// <summary>
        ///// generic method to get many record on the basis of a condition but query able.
        ///// DbSet.Where(where).AsQueryable()
        ///// </summary>
        ///// <param name="where"></param>
        ///// <returns></returns>
        //public virtual IQueryable<T> GetManyQueryable(Func<T, bool> where)
        //{
        //    return DbSet.Where(where).AsQueryable();
        //}

        ///// <summary>
        ///// generic get method , fetches data for the entities on the basis of condition.
        ///// DbSet.Where(where).FirstOrDefault<T>()
        ///// </summary>
        ///// <param name="where"></param>
        ///// <returns></returns>
        //public T Get(Func<T, Boolean> where)
        //{
        //    return DbSet.Where(where).FirstOrDefault<T>();
        //}


        ///// <summary>
        ///// generic method to fetch all the records from db
        ///// DbSet
        ///// </summary>
        ///// <returns></returns>
        //public virtual IEnumerable<T> GetAll()
        //{
        //    return DbSet;
        //}

        /// <summary>
        /// Inclue multiple
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        //public IQueryable<T> GetQueryableWithInclude(System.Linq.Expressions.Expression<Func<T, bool>> predicate, params string[] include)
        //{
        //    IQueryable<T> query = this.DbSet;
        //    query = include.Aggregate(query, (current, inc) => current.Include(inc));
        //    return query.Where(predicate);
        //}
        //public IEnumerable<T> GetWithInclude(System.Linq.Expressions.Expression<Func<T, bool>> predicate, int page, int pageSize, params string[] include)
        //{
        //    IQueryable<T> query = this.DbSet;
        //    query = include.Aggregate(query, (current, inc) => current.Include(inc));
        //    return query.Where(predicate).AsEnumerable().Skip((page - 1) * pageSize).Take(pageSize);
        //}
        //public IEnumerable<T> GetWithIncludeConditionalOrderBy<TKey>(System.Linq.Expressions.Expression<Func<T, bool>> predicate, System.Linq.Expressions.Expression<Func<T, TKey>> orderbyDesc, int page, int pageSize, params string[] include)
        //{
        //    IQueryable<T> query = this.DbSet;
        //    query = include.Aggregate(query, (current, inc) => current.Include(inc));
            
        //    return query.Where(predicate).OrderByDescending(orderbyDesc).AsEnumerable().Skip((page - 1) * pageSize).Take(pageSize);
        //}
        //public IEnumerable<T> GetWithIncludeOrderBy<TKey>(System.Linq.Expressions.Expression<Func<T, TKey>> orderbyDesc, int page, int pageSize, params string[] include)
        //{
        //    IQueryable<T> query = this.DbSet;
        //    query = include.Aggregate(query, (current, inc) => current.Include(inc));

        //    return query.OrderByDescending(orderbyDesc).AsEnumerable().Skip((page - 1) * pageSize).Take(pageSize);
        //}
        
        //public IEnumerable<T> GetAllWithInclude(int page, int pageSize, params string[] include)
        //{
        //    IQueryable<T> query = this.DbSet;
        //    query = include.Aggregate(query, (current, inc) => current.Include(inc));
        //    return query.AsEnumerable().Skip((page - 1) * pageSize).Take(pageSize);
        //}
        //public T GetByIdWithInclude(System.Linq.Expressions.Expression<Func<T, bool>> predicate, params string[] include)
        //{
        //    IQueryable<T> query = this.DbSet;
        //    query = include.Aggregate(query, (current, inc) => current.Include(inc));
        //    return query.Where(predicate).FirstOrDefault();
        //}

        //public IQueryable<T> GetWithIncludeAsNoTracking(System.Linq.Expressions.Expression<Func<T, bool>> predicate, params string[] include)
        //{
        //    IQueryable<T> query = this.DbSet;
        //    query = include.Aggregate(query, (current, inc) => current.Include(inc));
        //    return query.Where(predicate).AsNoTracking();
        //}
        //public int GetCount()
        //{
        //    return DbSet.Count();
        //}
        ////public void LoadReference(System.Linq.Expressions.Expression<Func<T, bool>> predicate, params string[] include)
        ////{
        ////    IQueryable<T> query = this.DbSet;
        ////    query = include.Aggregate(query, (current, inc) => current.Reference(inc));
        ////    query.Where(predicate).Load();
        ////}


        ////}
        ///// <summary>
        ///// Generic method to check if entity exists
        ///// DbSet.Find(primaryKey) != null
        ///// </summary>
        ///// <param name="primaryKey"></param>
        ///// <returns></returns>
        //public bool Exists(object primaryKey)
        //{
        //    return DbSet.Find(primaryKey) != null;
        //}

        ///// <summary>
        ///// Gets a single record by the specified criteria (usually the unique identifier)
        ///// DbSet.Single<T>(predicate)
        ///// </summary>
        ///// <param name="predicate">Criteria to match on</param>
        ///// <returns>A single record that matches the specified criteria</returns>
        //public T GetSingle(Func<T, bool> predicate)
        //{
        //    return DbSet.Single<T>(predicate);
        //}

        ///// <summary>
        ///// The first record matching the specified criteria
        ///// DbSet.First<T>(predicate)
        ///// </summary>
        ///// <param name="predicate">Criteria to match on</param>
        ///// <returns>A single record containing the first record matching the specified criteria</returns>
        //public T GetFirst(Func<T, bool> predicate)
        //{
        //    return DbSet.First<T>(predicate);
        //}


        #endregion

    }
}
