using GotFired.Model.Entities;
using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Entities.Enums;
using GotFired.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GotFired.DAL
{
    /// <summary>
    /// Development-only sample data (replaces the EF6 "ForSeedOnly" helper). Never run against production:
    /// Program.cs only calls it when the Seed:Enabled setting is true.
    /// </summary>
    public static class DevDataSeeder
    {
        public static async Task SeedAsync(GotFiredDbContext context, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, string adminUserName, string adminPassword)
        {
            foreach (var role in new[] { "admin", "editor", "user" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            if (await userManager.FindByNameAsync(adminUserName) == null)
            {
                var admin = new ApplicationUser { UserName = adminUserName, FriendlyName = adminUserName };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
                await userManager.AddToRoleAsync(admin, "admin");
            }

            if (await context.City.AnyAsync()) return;

            context.City.AddRange(Stamp(new City { Name = "İstanbul" }), Stamp(new City { Name = "Ankara" }), Stamp(new City { Name = "İzmir" }));
            context.Category.Add(Stamp(new Category { Name = "Mobbing" }));
            context.CompanySector.Add(Stamp(new CompanySector { Name = "İnsan Kaynakları" }));
            context.DeclaredTerminationReason.Add(Stamp(new DeclaredTerminationReason { Name = "Performans Düşüklüğü" }));
            context.SGKTerminationReason.Add(Stamp(new SGKTerminationReason { Code = "04", Name = "Belirsiz süreli iş sözleşmesinin işveren tarafından haklı sebep bildirilmeden feshi" }));
            context.SupportedBy.AddRange(Stamp(new SupportedBy { Name = "Sendika" }), Stamp(new SupportedBy { Name = "Avukat" }));
            context.AnswerTemplate.Add(Stamp(new AnswerTemplate { Name = "Karşılama", Content = "<p>Merhaba, başvurunuz için teşekkürler.</p>" }));
            context.Tag.Add(Stamp(new Tag { Name = "tazminat" }));
            await context.SaveChangesAsync();

            var city = await context.City.OrderBy(c => c.ID).FirstAsync();
            context.DismissalCase.Add(Stamp(new DismissalCase
            {
                Guid = "0f8fad5b-d9cb-469f-a165-70867728950e",
                CityID = city.ID,
                Applicant = Stamp(new Applicant
                {
                    FullName = "Örnek Başvurucu",
                    AgeInterval = AgeInterval.ThirtyFiveToFortyFour,
                    EducationalState = EducationState.HighSchool,
                    Email = "applicant@example.com",
                    Gender = Gender.Female
                }),
                DismissalState = DismissalState.AboutToBeDismissed,
                EmployeeCount = EmployeeCount.MoreThenThirty,
                EmploymentDurationSince = EmploymentDurationSince.FromOneAndHalfYearToThreeYears,
                EmploymentTerminationDate = DateTime.Today.AddDays(-10),
                AppealState = AppealState.Pending,
                CompanyDescription = "Örnek şirket",
                DetailedExperience = "Örnek deneyim",
                WantShareHisExperience = true
            }));
            await context.SaveChangesAsync();
        }

        private static T Stamp<T>(T entity) where T : BaseEntity
        {
            entity.CreatedDate = entity.EditedDate = DateTime.Now;
            entity.IsActive = true;
            entity.RowVersion = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper() + Random.Shared.Next(10000000, 20000000);
            return entity;
        }
    }
}
