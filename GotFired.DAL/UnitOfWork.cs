using GotFired.DAL;
using GotFired.Model;
using GotFired.Model.Entities;
using GotFired.Model.Entities.DismissalCase;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Identity.EntityFramework;
using GotFired.Model.Interfaces;
using GotFired.DAL.Repositories;

namespace GotFired.DAL
{
    /// <summary>
    /// Unit of Work class responsible for DB transactions
    /// </summary>
    //public abstract class UnitOfWork : IDisposable, IUnitOfWork
    public partial class UnitOfWork : IDisposable, IUnitOfWork
    {
        #region Private member variables
        GotFiredDbContext _context = null;
        //GenericRepository<DismissalCase> _dismissalCaseModel;
        GenericRepository<City> _cityModel;
        GenericRepository<SGKTerminationReason> _sgkTerminationReasonModel;
        GenericRepository<CompanySector> _companySectorModel;
        GenericRepository<DeclaredTerminationReason> _declaredTerminationReasonModel;
        GenericRepository<SupportedBy> _supportedByModel;
        GenericRepository<DismissalCaseSupprtedBy> _dismissalCaseSupprtedByModel;
        AnswerTemplateRepository _answerTemplateRepository;
        GenericRepository<Category> _categoryModel;
        BaseRepository<Comment> _commentModel;
        ITagRepository _tagModel;
        IDismissalCaseTagRepository _dismissalCaseTagModel;
         //_userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
         AuthRepository _auth;

        IDismissalCaseRepository _dismissalCaseRepository;

        #endregion

        public UnitOfWork()
        {
            _context = new GotFiredDbContext();
        }

        #region Public Repository Creation properties
        public AuthRepository AuthRepository
        {
            get
            {
                if (_auth == null)
                    //_auth = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_context));
                    _auth = new AuthRepository(_context);
                return _auth;
            }
        }
        public IDismissalCaseRepository dismissalCaseRepository
        {
            get
            {
                if (_dismissalCaseRepository == null)
                    //_auth = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_context));
                    _dismissalCaseRepository = new DismissalCaseRepository(_context);
                return _dismissalCaseRepository;
            }
        }
       

        //public GenericRepository<DismissalCase> DismissalCaseRepository
        //{
        //    get
        //    {
        //        if (_dismissalCaseModel == null)
        //            _dismissalCaseModel = new GenericRepository<DismissalCase>(_context);
        //        return _dismissalCaseModel;
        //    }
        //}
        public BaseRepository<Comment> CommentRepository
        {
            get
            {
                if (_commentModel == null)
                    _commentModel = new GenericRepository<Comment>(_context);
                return _commentModel;
            }
        }
        public GenericRepository<DismissalCaseSupprtedBy> DismissalCaseSupprtedByRepository
        {
            get
            {
                if (_dismissalCaseSupprtedByModel == null)
                    _dismissalCaseSupprtedByModel = new GenericRepository<DismissalCaseSupprtedBy>(_context);
                return _dismissalCaseSupprtedByModel;
            }
        }
        public GenericRepository<City> CityRepository
        {
            get
            {
                if (_cityModel == null)
                    _cityModel = new GenericRepository<City>(_context);
                return _cityModel;
            }
        }
        public GenericRepository<SGKTerminationReason> SGKTerminationReasonRepository
        {
            get
            {
                if (_sgkTerminationReasonModel == null)
                    _sgkTerminationReasonModel = new GenericRepository<SGKTerminationReason>(_context);
                return _sgkTerminationReasonModel;
            }
        }
        public GenericRepository<CompanySector> CompanySectorRepository
        {
            get
            {
                if (_companySectorModel == null)
                    _companySectorModel = new GenericRepository<CompanySector>(_context);
                return _companySectorModel;
            }
        }
        public GenericRepository<DeclaredTerminationReason> DeclaredTerminationReasonRepository
        {
            get
            {
                if (_declaredTerminationReasonModel == null)
                    _declaredTerminationReasonModel = new GenericRepository<DeclaredTerminationReason>(_context);
                return _declaredTerminationReasonModel;
            }
        }
        public GenericRepository<SupportedBy> SupportedByRepository
        {
            get
            {
                if (_supportedByModel == null)
                    _supportedByModel = new GenericRepository<SupportedBy>(_context);
                return _supportedByModel;
            }
        }
        
        public IAnswerTemplateRepository AnswerTemplateRepository
        {
            get
            {
                if (_answerTemplateRepository == null)
                    _answerTemplateRepository = new AnswerTemplateRepository(_context);
                return _answerTemplateRepository;
            }
        }
        public GenericRepository<Category> CategoryRepository
        {
            get
            {
                if (_categoryModel == null)
                    _categoryModel = new GenericRepository<Category>(_context);
                return _categoryModel;
            }
        }
        public ITagRepository TagRepository
        {
            get
            {
                if (_tagModel == null)
                    _tagModel = new TagRepository(_context);
                return _tagModel;
            }
        }
        public IDismissalCaseTagRepository DismissalCaseTagRepository
        {
            get
            {
                if (_dismissalCaseTagModel == null)
                    _dismissalCaseTagModel = new DismissalCaseTagRepository(_context);
                return _dismissalCaseTagModel;
            }
        }
        #endregion

        private static void SaveChanges(DbContext context)
        {
            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var sb = new StringBuilder();
                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }
                throw new DbEntityValidationException(
                    "Entity Validation Failed - errors follow:\n" +
                    sb.ToString(), ex
                );
            }
        }

        #region Public member methods
        /// <summary>
        /// Save method.
        /// </summary>
        public void Save()
        {
            foreach (var entry in _context.ChangeTracker.Entries().Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified)))
            {
                if (entry.Entity is BaseEntity)
                {
                    BaseEntity baseEntity = entry.Entity as BaseEntity;

                    baseEntity.IsActive = true;
                    baseEntity.IsDeleted = false;


                    //if (entry.Entity is DismissalCase)
                    //{
                    //    //baseEntity.Creator = baseEntity
                    //}

                    if (entry.State == EntityState.Added)
                    {
                        baseEntity.CreatedDate = DateTime.Now;
                        //baseEntity.Creator = 0; // ?
                    }
                    else
                    {
                        _context.Entry(baseEntity).Property(x => x.CreatedDate).IsModified = false;
                        _context.Entry(baseEntity).Property(x => x.Creator).IsModified = false;
                    }
                    baseEntity.RowVersion = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper() + new Random().Next(10000000, 20000000);
                    baseEntity.EditedDate = DateTime.Now;
                    //baseEntity.Editor = 0; // ?
                }
            }


            try
            {
                //_context.SaveChanges();
                SaveChanges(_context);
            }
            catch (DbEntityValidationException e)
            {

                var outputLines = new List<string>();
                foreach (var eve in e.EntityValidationErrors)
                {
                    outputLines.Add(string.Format("{0}: Entity of type \"{1}\" in state \"{2}\" has the following validation errors:", DateTime.Now, eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        outputLines.Add(string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage));
                    }
                }
                //System.IO.File.AppendAllLines(@"C:\errors.txt", outputLines);

                throw e;
            }

        }

        #endregion

        #region Implementing IDisposable...

        private bool disposed = false;

        /// <summary>
        /// Protected Virtual Dispose method
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Debug.WriteLine("GotFiredDbContext is being disposed");
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            //GC.SuppressFinalize(this);
        }
        #endregion
    }
}
