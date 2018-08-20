using GotFired.DAL;
using GotFired.Model.Entities.DismissalCase;
using GotFired.Model.Entities.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GotFired.DAL
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    //public class ApplicationUser : IdentityUser
    //{
    //    public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
    //    {
    //        // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
    //        var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
    //        // Add custom user claims here
    //        return userIdentity;
    //    }
    //}

    //public class GotFiredDbContext : IdentityDbContext<ApplicationUser>
    //{
    //    public GotFiredDbContext()
    //        : base("Data Source=FAIK-PC; Initial Catalog=Structure4DB; Integrated Security=True;", throwIfV1Schema: false)
    //    {
    //        Configuration.LazyLoadingEnabled = false;
    //        Database.Log = s => Debug.WriteLine(s);
    //    }

    //    public static GotFiredDbContext Create()
    //    {
    //        return new GotFiredDbContext();
    //    }
    //}

    public static class ForSeedOnly
    {
        public static void AddUser(GotFiredDbContext context)
        {
            IdentityResult identityResult;
            UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            roleManager.Create(new IdentityRole { Name = "admin" });
            roleManager.Create(new IdentityRole { Name = "editor" });
            roleManager.Create(new IdentityRole { Name = "user" });
            var user = new ApplicationUser()
            {
                UserName = "osman",
            };
            var temp = userManager.FindByName(user.UserName);
            if (temp == null)
            {
                identityResult = userManager.Create(user, "password");
                userManager.AddToRole(user.Id, "admin");
                //temp = userManager.FindByName(user.UserName);
            }
        }


        public static void AddBulkData(GotFiredDbContext context)
        {
            context.City.AddOrUpdate(
                    c => c.ID,
                    new City
                    {
                        ID = 1,
                        Name = "Istanbul",
                        Creator = 0,
                        Editor = 0,
                        RowVersion = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper() + new Random().Next(10000000, 20000000),
                        CreatedDate = DateTime.Now,
                        EditedDate = DateTime.Now
                    },
                    new City
                    {
                        ID = 2,
                        Name = "Ankara",
                        Creator = 0,
                        Editor = 0,
                        RowVersion = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper() + new Random().Next(10000000, 20000000),
                        CreatedDate = DateTime.Now,
                        EditedDate = DateTime.Now
                    },
                    new City { ID = 3, Name = "İzmir", Creator = 0, Editor = 0, RowVersion = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper() + new Random().Next(10000000, 20000000), CreatedDate = DateTime.Now, EditedDate = DateTime.Now }
                    );

            context.Category.AddOrUpdate(
                c => c.ID,
                new Category { ID = 1, Name = "Mobbing", CreatedDate = DateTime.Now, EditedDate = DateTime.Now, Creator = 0, Editor = 0, RowVersion = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper() + new Random().Next(10000000, 20000000) }
                );

            context.CompanySector.AddOrUpdate(
                c => c.ID,
                new CompanySector { ID = 1, Name = "İnsan Kaynakları", CreatedDate = DateTime.Now, EditedDate = DateTime.Now, Creator = 0, Editor = 0, RowVersion = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper() + new Random().Next(10000000, 20000000) }
                );
            context.DeclaredTerminationReason.AddOrUpdate(
                c => c.ID,
                new DeclaredTerminationReason
                {
                    ID = 1,
                    Name = "Performans Düşüklüğü",
                    Creator = 0,
                    Editor = 0,
                    RowVersion = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper() + new Random().Next(10000000, 20000000),
                    CreatedDate = DateTime.Now,
                    EditedDate = DateTime.Now
                }
                );
            context.Applicant.AddOrUpdate(
                c => c.ID,
                new Applicant
                {
                    ID = 1,
                    FullName = "Ahmet",
                    AgeInterval = AgeInterval.ThirtyFiveToFortyFour,
                    EducationalState = EducationState.HighSchool,
                    Email = "ahmet@yardimedin.com",
                    Gender = Gender.Male,
                    IsActive = true,
                    PhoneNumber = "5390032020",
                    IsDeleted = false,
                    CreatedDate = DateTime.Now,
                    EditedDate = DateTime.Now,
                    Creator = 0,
                    Editor = 0,
                    RowVersion = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper() + new Random().Next(10000000, 20000000)
                });

            context.SupportedBy.AddOrUpdate(
                c => c.ID,
                new SupportedBy
                {
                    ID = 1,
                    Name = "Ayşe",
                    Creator = 0,
                    Editor = 0,
                    RowVersion = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper() + new Random().Next(10000000, 20000000),
                    CreatedDate = DateTime.Now,
                    EditedDate = DateTime.Now
                },
                new SupportedBy
                {
                    ID = 2,
                    Name = "Fatma",
                    Creator = 0,
                    Editor = 0,
                    RowVersion = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper() + new Random().Next(10000000, 20000000),
                    CreatedDate = DateTime.Now,
                    EditedDate = DateTime.Now
                }
                    );
            context.DismissalCase.AddOrUpdate(
                c => c.ID,
                new DismissalCase
                {
                    ID = 1,
                    Guid = "0f8fad5b-d9cb-469f-a165-70867728950e",
                    AdditionalInfo = "-",
                    ApplicantId = 1,
                    CategoryID = 1,
                    CityID = 1,
                    CompanyDescription = "Açıklama",
                    CompanySectorID = 1,
                    DeclaredTerminationReasonID = 1,
                    DetailedExperience = "Yok",
                    DismissalState = DismissalState.AboutToBeDismissed,
                    EmployeeCount = EmployeeCount.MoreThenThirty,
                    WantShareHisExperience = true,
                    HasSignedAnyDocument = false,
                    EmploymentDurationSince = EmploymentDurationSince.FromOneAndHalfYearToThreeYears,
                    EmploymentTerminationDate = DateTime.Now.AddDays(-10),
                    Creator = 0,
                    Editor = 0,
                    RowVersion = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper() + new Random().Next(10000000, 20000000),
                    CreatedDate = DateTime.Now,
                    EditedDate = DateTime.Now
                });
            context.Comment.AddOrUpdate(
                c => c.ID,
                new Comment
                {
                    ID = 1,
                    DismissalCaseID = 1,
                    Text = "test",
                    UserName = "Ahmet",
                    Creator = 0,
                    Editor = 0,
                    RowVersion = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8).ToUpper() + new Random().Next(10000000, 20000000),
                    CreatedDate = DateTime.Now,
                    EditedDate = DateTime.Now
                });

        }


    }
}