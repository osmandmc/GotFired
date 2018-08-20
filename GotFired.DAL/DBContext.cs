using GotFired.Model.Entities.DismissalCase;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Diagnostics;

namespace GotFired.DAL
{
    class DbContextConfiguration : DbConfiguration
    {
        DbContextConfiguration()
        {
            //this.SetDatabaseInitializer(new DropCreateDatabaseAlways<GotFiredDbContext>());
            SetProviderServices(SqlProviderServices.ProviderInvariantName, SqlProviderServices.Instance);
        }
    }
    [DbConfigurationType(typeof(DbContextConfiguration))]
    public partial class GotFiredDbContext : IdentityDbContext<ApplicationUser> //:ApplicationDbContext DbContext 
    {
        [DebuggerHidden]
        public GotFiredDbContext()
        :base("Data Source=(localdb)\\mssqllocaldb; Initial Catalog=gotfired; Integrated Security=True")
        //: base("Data Source=localhost; Initial Catalog=gotfired; User ID= gfUser; Password=mg8~Cu79")
        {
            Configuration.LazyLoadingEnabled = false;
            Database.Log = s => Debug.WriteLine(s);
        }
        public DbSet<Applicant> Applicant { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<CompanySector> CompanySector { get; set; }
        public DbSet<DeclaredTerminationReason> DeclaredTerminationReason { get; set; }
        public DbSet<DismissalCase> DismissalCase { get; set; }
        public DbSet<SupportedBy> SupportedBy { get; set; }
        public DbSet<AnswerTemplate> AnswerTemplate { get; set; }
        public DbSet<DismissalCaseSupprtedBy> DismissalCaseSupprtedBy { get; set; }
        public DbSet<Tag> Tag { get; set; }
        public DbSet<DismissalCaseTag> DismissalCaseTag { get; set; }

    }
    public class ApplicationUser : IdentityUser
    {
        public string FriendlyName { get; set; }
    }
}
