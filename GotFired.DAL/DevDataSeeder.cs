using GotFired.Model.Entities;
using GotFired.Model.Entities.Enums;
using GotFired.DAL.Identity;
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

            // Audit columns (dates, RowVersion) are stamped by SaveChanges.
            context.City.AddRange(new City { Name = "İstanbul" }, new City { Name = "Ankara" }, new City { Name = "İzmir" });
            context.Category.Add(new Category { Name = "Mobbing" });
            context.CompanySector.Add(new CompanySector { Name = "İnsan Kaynakları" });
            context.DeclaredTerminationReason.Add(new DeclaredTerminationReason { Name = "Performans Düşüklüğü" });
            context.SGKTerminationReason.Add(new SGKTerminationReason { Code = "04", Name = "Belirsiz süreli iş sözleşmesinin işveren tarafından haklı sebep bildirilmeden feshi" });
            context.SupportedBy.AddRange(new SupportedBy { Name = "Sendika" }, new SupportedBy { Name = "Avukat" });
            context.AnswerTemplate.Add(AnswerTemplate.Create("Karşılama", "<p>Merhaba, başvurunuz için teşekkürler.</p>"));
            context.Tag.Add(new Tag { Name = "tazminat" });
            await context.SaveChangesAsync();

            var city = await context.City.OrderBy(c => c.ID).FirstAsync();
            var applicant = Applicant.Create("Örnek Başvurucu", "applicant@example.com", null, Gender.Female,
                AgeInterval.ThirtyFiveToFortyFour, EducationState.HighSchool, null);
            context.DismissalCase.Add(DismissalCase.Submit("0f8fad5b-d9cb-469f-a165-70867728950e", applicant, new CaseDetails
            {
                CityId = city.ID,
                DismissalState = DismissalState.AboutToBeDismissed,
                EmployeeCount = EmployeeCount.MoreThanThirty,
                EmploymentDurationSince = EmploymentDurationSince.FromOneAndHalfYearToThreeYears,
                EmploymentTerminationDate = DateTime.Today.AddDays(-10),
                CompanyDescription = "Örnek şirket",
                DetailedExperience = "Örnek deneyim",
                WantShareHisExperience = true
            }, supportedByIds: []));
            await context.SaveChangesAsync();
        }
    }
}
