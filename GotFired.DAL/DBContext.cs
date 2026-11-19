using GotFired.Model.Entities;
using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GotFired.DAL
{
    /// <summary>
    /// EF Core context mapped onto the schema that was originally created by EF6 (migration "sgk", 2018)
    /// and ASP.NET Identity 2. The ASP.NET Identity v2 tables must first be upgraded with
    /// db/identity-v2-to-core.sql. Every mapping below exists to keep the EF Core model identical to that
    /// existing database, so an empty "Probe" migration proves parity.
    /// </summary>
    public class GotFiredDbContext : IdentityDbContext<ApplicationUser>
    {
        public GotFiredDbContext(DbContextOptions<GotFiredDbContext> options) : base(options)
        {
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
        public DbSet<SGKTerminationReason> SGKTerminationReason { get; set; }
        public DbSet<Tag> Tag { get; set; }
        public DbSet<DismissalCaseTag> DismissalCaseTag { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // EF6 mapped System.DateTime to SQL Server "datetime"; EF Core defaults to "datetime2".
            configurationBuilder.Properties<DateTime>().HaveColumnType("datetime");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // EF6 pluralized table names by convention.
            builder.Entity<AnswerTemplate>().ToTable("AnswerTemplates");
            builder.Entity<Applicant>().ToTable("Applicants");
            builder.Entity<Category>().ToTable("Categories");
            builder.Entity<City>().ToTable("Cities");
            builder.Entity<Comment>().ToTable("Comments");
            builder.Entity<CompanySector>().ToTable("CompanySectors");
            builder.Entity<DeclaredTerminationReason>().ToTable("DeclaredTerminationReasons");
            builder.Entity<DismissalCase>().ToTable("DismissalCases");
            builder.Entity<DismissalCaseSupprtedBy>().ToTable("DismissalCaseSupprtedBies");
            builder.Entity<DismissalCaseTag>().ToTable("DismissalCaseTags");
            builder.Entity<SGKTerminationReason>().ToTable("SGKTerminationReasons");
            builder.Entity<SupportedBy>().ToTable("SupportedBies");
            builder.Entity<Tag>().ToTable("Tags");

            // Relationships and delete behaviour exactly as in the EF6 migration.
            builder.Entity<Comment>()
                .HasOne<DismissalCase>().WithMany(d => d.Comments)
                .HasForeignKey(c => c.DismissalCaseID).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DismissalCase>(e =>
            {
                e.Ignore(d => d.SupportedByIds);
                e.Property(d => d.AccessTokenHash).HasMaxLength(32);
                e.HasIndex(d => d.AccessTokenHash).IsUnique().HasFilter("[AccessTokenHash] IS NOT NULL");
                e.HasOne(d => d.Applicant).WithMany().HasForeignKey(d => d.ApplicantId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(d => d.City).WithMany().HasForeignKey(d => d.CityID).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(d => d.Category).WithMany().HasForeignKey(d => d.CategoryID).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(d => d.CompanySector).WithMany().HasForeignKey(d => d.CompanySectorID).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(d => d.DeclaredTerminationReason).WithMany().HasForeignKey(d => d.DeclaredTerminationReasonID).OnDelete(DeleteBehavior.NoAction);
                e.HasOne(d => d.SGKTerminationReason).WithMany().HasForeignKey(d => d.SGKTerminationReasonID).OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<DismissalCaseSupprtedBy>(e =>
            {
                e.HasOne(s => s.DismissalCase).WithMany(d => d.DismissalCaseSupprtedBys).HasForeignKey(s => s.DismissalCaseId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(s => s.SupportedBy).WithMany(s => s.DismissalCaseSupprtedBys).HasForeignKey(s => s.SupportedById).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<DismissalCaseTag>(e =>
            {
                e.HasOne(t => t.DismissalCase).WithMany(d => d.DismissalCaseTags).HasForeignKey(t => t.DismissalCaseID).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(t => t.Tag).WithMany().HasForeignKey(t => t.TagID).OnDelete(DeleteBehavior.Cascade);
            });

            // ASP.NET Identity 2 used nvarchar(128) keys; Core defaults to nvarchar(450).
            builder.Entity<ApplicationUser>(e =>
            {
                e.Property(u => u.Id).HasMaxLength(128);
                e.Property(u => u.UserName).IsRequired();
            });
            builder.Entity<IdentityRole>(e =>
            {
                e.Property(r => r.Id).HasMaxLength(128);
                e.Property(r => r.Name).IsRequired();
            });
            builder.Entity<IdentityUserRole<string>>(e =>
            {
                e.Property(r => r.UserId).HasMaxLength(128);
                e.Property(r => r.RoleId).HasMaxLength(128);
            });
            builder.Entity<IdentityUserClaim<string>>().Property(c => c.UserId).HasMaxLength(128);
            builder.Entity<IdentityUserLogin<string>>(e =>
            {
                e.Property(l => l.UserId).HasMaxLength(128);
                e.Property(l => l.LoginProvider).HasMaxLength(128);
                e.Property(l => l.ProviderKey).HasMaxLength(128);
            });
            builder.Entity<IdentityRoleClaim<string>>().Property(c => c.RoleId).HasMaxLength(128);
            builder.Entity<IdentityUserToken<string>>(e =>
            {
                e.Property(t => t.UserId).HasMaxLength(128);
                e.Property(t => t.LoginProvider).HasMaxLength(128);
                e.Property(t => t.Name).HasMaxLength(128);
            });

            ApplyEf6ConstraintNames(builder);
        }

        /// <summary>
        /// EF6 named constraints "PK_dbo.Table", "FK_dbo.Table_dbo.Principal_Column" and indexes "IX_Column".
        /// Matching these names keeps future EF Core migrations able to drop/alter existing constraints.
        /// Applies to tables that came from the EF6 schema (all except the two new Identity tables, which
        /// db/identity-v2-to-core.sql creates with these same names).
        /// </summary>
        private static void ApplyEf6ConstraintNames(ModelBuilder builder)
        {
            foreach (IMutableEntityType entity in builder.Model.GetEntityTypes())
            {
                string table = entity.GetTableName();
                if (table == null) continue;

                entity.FindPrimaryKey()?.SetName($"PK_dbo.{table}");

                foreach (IMutableForeignKey fk in entity.GetForeignKeys())
                {
                    string principal = fk.PrincipalEntityType.GetTableName();
                    string columns = string.Join("_", fk.Properties.Select(p => p.GetColumnName()));
                    fk.SetConstraintName($"FK_dbo.{table}_dbo.{principal}_{columns}");
                }

                foreach (IMutableIndex index in entity.GetIndexes())
                {
                    // Identity's named indexes (UserNameIndex, RoleNameIndex, EmailIndex) keep their names.
                    if (index.GetDatabaseName() is "UserNameIndex" or "RoleNameIndex" or "EmailIndex") continue;
                    index.SetDatabaseName("IX_" + string.Join("_", index.Properties.Select(p => p.GetColumnName())));
                }
            }
        }
    }
}
