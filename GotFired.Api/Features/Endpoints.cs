using GotFired.Api.Features.AnswerTemplates;
using GotFired.Api.Features.ApplicantAccess;
using GotFired.Api.Features.Applications;
using GotFired.Api.Features.Auth;
using GotFired.Api.Features.Cases;
using GotFired.Api.Features.Lookups;
using GotFired.Api.Features.Users;

namespace GotFired.Api.Features
{
    /// <summary>The whole HTTP API: one line per use case. Each slice owns its route, request, handler and response.</summary>
    public static class Endpoints
    {
        public const string Staff = "Staff";            // admin, editor, user
        public const string Admin = "Admin";            // admin
        public const string AdminOrUser = "AdminOrUser"; // admin, user (user management screens, as before)

        public static IServiceCollection AddEndpointPolicies(this IServiceCollection services) =>
            services.AddAuthorizationBuilder()
                .AddPolicy(Staff, p => p.RequireRole("admin", "editor", "user"))
                .AddPolicy(Admin, p => p.RequireRole("admin"))
                .AddPolicy(AdminOrUser, p => p.RequireRole("admin", "user"))
                .Services;

        public static void MapFeatures(this IEndpointRouteBuilder app)
        {
            RequestToken.MapTokenEndpoint(app);   // POST /token (OAuth2 password grant, outside /api)

            var api = app.MapGroup("/api/v1");

            // Public: the application form and the applicant's own case (secret link token).
            GetApplicationForm.Map(api);
            SubmitApplication.Map(api);
            GetApplicantCase.Map(api);
            ReplyToCase.Map(api);

            // Staff: cases and their lookups.
            var staff = api.MapGroup("").RequireAuthorization(Staff);
            var cases = staff.MapGroup("/cases");
            ListCases.Map(cases);
            GetCase.Map(cases);
            AnswerCase.Map(cases);
            UpdateNotes.Map(cases);
            SetAppealState.Map(cases);
            SetTags.Map(cases);
            CaseTag.Map(cases);
            ListCategories.Map(staff);
            ListTags.Map(staff);

            // Answer templates: every staff member reads them while answering; only admins change them.
            var templates = staff.MapGroup("/answer-templates");
            ListTemplates.Map(templates);
            GetTemplate.Map(templates);
            var templateAdmin = api.MapGroup("/answer-templates").RequireAuthorization(Admin);
            CreateTemplate.Map(templateAdmin);
            UpdateTemplate.Map(templateAdmin);

            // Users: admins and "user" role can view; only admins change.
            var userReaders = api.MapGroup("").RequireAuthorization(AdminOrUser);
            ListUsers.Map(userReaders.MapGroup("/users"));
            GetUser.Map(userReaders.MapGroup("/users"));
            ListRoles.Map(userReaders);
            var userAdmin = api.MapGroup("/users").RequireAuthorization(Admin);
            CreateUser.Map(userAdmin);
            UpdateUser.Map(userAdmin);
            SetPassword.Map(userAdmin);
        }
    }
}
