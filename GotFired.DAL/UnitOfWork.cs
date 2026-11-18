using GotFired.DAL.Repositories;
using GotFired.Model;
using GotFired.Model.Entities;
using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GotFired.DAL
{
    /// <summary>
    /// Unit of Work: lazily creates repositories over one shared (request-scoped) context.
    /// Call <see cref="Save"/> to commit. The context lifetime is owned by DI.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GotFiredDbContext _context;

        private GenericRepository<City> _cityModel;
        private GenericRepository<SGKTerminationReason> _sgkTerminationReasonModel;
        private GenericRepository<CompanySector> _companySectorModel;
        private GenericRepository<DeclaredTerminationReason> _declaredTerminationReasonModel;
        private GenericRepository<SupportedBy> _supportedByModel;
        private GenericRepository<DismissalCaseSupprtedBy> _dismissalCaseSupprtedByModel;
        private AnswerTemplateRepository _answerTemplateRepository;
        private GenericRepository<Category> _categoryModel;
        private BaseRepository<Comment> _commentModel;
        private ITagRepository _tagModel;
        private IDismissalCaseTagRepository _dismissalCaseTagModel;
        private IDismissalCaseRepository _dismissalCaseRepository;

        public UnitOfWork(GotFiredDbContext context)
        {
            _context = context;
        }

        public IDismissalCaseRepository dismissalCaseRepository => _dismissalCaseRepository ??= new DismissalCaseRepository(_context);
        public BaseRepository<Comment> CommentRepository => _commentModel ??= new GenericRepository<Comment>(_context);
        public GenericRepository<DismissalCaseSupprtedBy> DismissalCaseSupprtedByRepository => _dismissalCaseSupprtedByModel ??= new GenericRepository<DismissalCaseSupprtedBy>(_context);
        public GenericRepository<City> CityRepository => _cityModel ??= new GenericRepository<City>(_context);
        public GenericRepository<SGKTerminationReason> SGKTerminationReasonRepository => _sgkTerminationReasonModel ??= new GenericRepository<SGKTerminationReason>(_context);
        public GenericRepository<CompanySector> CompanySectorRepository => _companySectorModel ??= new GenericRepository<CompanySector>(_context);
        public GenericRepository<DeclaredTerminationReason> DeclaredTerminationReasonRepository => _declaredTerminationReasonModel ??= new GenericRepository<DeclaredTerminationReason>(_context);
        public GenericRepository<SupportedBy> SupportedByRepository => _supportedByModel ??= new GenericRepository<SupportedBy>(_context);
        public IAnswerTemplateRepository AnswerTemplateRepository => _answerTemplateRepository ??= new AnswerTemplateRepository(_context);
        public GenericRepository<Category> CategoryRepository => _categoryModel ??= new GenericRepository<Category>(_context);
        public ITagRepository TagRepository => _tagModel ??= new TagRepository(_context);
        public IDismissalCaseTagRepository DismissalCaseTagRepository => _dismissalCaseTagModel ??= new DismissalCaseTagRepository(_context);

        /// <summary>
        /// Stamps audit fields (dates, active/deleted flags, the string RowVersion) on every added or
        /// modified <see cref="BaseEntity"/>, then saves.
        /// </summary>
        public void Save()
        {
            foreach (var entry in _context.ChangeTracker.Entries<BaseEntity>()
                         .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                BaseEntity baseEntity = entry.Entity;
                baseEntity.IsActive = true;
                baseEntity.IsDeleted = false;
                if (entry.State == EntityState.Added)
                {
                    baseEntity.CreatedDate = DateTime.Now;
                }
                else
                {
                    entry.Property(x => x.CreatedDate).IsModified = false;
                    entry.Property(x => x.Creator).IsModified = false;
                }
                baseEntity.RowVersion = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper() + Random.Shared.Next(10000000, 20000000);
                baseEntity.EditedDate = DateTime.Now;
            }

            _context.SaveChanges();
        }
    }
}
