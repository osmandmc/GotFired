using GotFired.DAL;
using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GotFired.Business.Services
{
    public interface IAnswerTemplateService
    {
        IReadOnlyList<LookupIDNamePair> List(int page, int pageSize);
        IReadOnlyList<LookupIDNamePair> Lookup();
        AnswerTemplateDto Get(int id);
        void Create(AnswerTemplateCreateModel model);
        /// <summary>Saves an edit; rejected when someone else saved the template since <c>RowVersion</c> was read.</summary>
        void Update(AnswerTemplateDto model);
    }

    public class AnswerTemplateService(GotFiredDbContext db) : IAnswerTemplateService
    {
        public IReadOnlyList<LookupIDNamePair> List(int page, int pageSize) =>
            db.AnswerTemplate.OrderBy(t => t.ID).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(t => new LookupIDNamePair { ID = t.ID, Name = t.Name }).ToList();

        public IReadOnlyList<LookupIDNamePair> Lookup() =>
            db.AnswerTemplate.OrderBy(t => t.ID).Select(t => new LookupIDNamePair { ID = t.ID, Name = t.Name }).ToList();

        public AnswerTemplateDto Get(int id) =>
            db.AnswerTemplate.Where(t => t.ID == id)
                .Select(t => new AnswerTemplateDto { ID = t.ID, Name = t.Name, Content = t.Content, RowVersion = t.RowVersion })
                .SingleOrDefault();

        public void Create(AnswerTemplateCreateModel model)
        {
            db.AnswerTemplate.Add(new AnswerTemplate { Name = model.Name, Content = model.Content });
            db.SaveChanges();
        }

        public void Update(AnswerTemplateDto model)
        {
            var template = db.AnswerTemplate.Find(model.ID)
                           ?? throw new BusinessRuleException("Şablon bulunamadı.");
            // Optimistic concurrency: the UPDATE only matches if RowVersion is still what the editor loaded.
            db.Entry(template).Property(t => t.RowVersion).OriginalValue = model.RowVersion;
            template.Name = model.Name;
            template.Content = model.Content;
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessRuleException("Başka bir kullanıcı güncelleme yapmakta, lütfen daha sonra tekrar deneyin.");
            }
        }
    }
}
