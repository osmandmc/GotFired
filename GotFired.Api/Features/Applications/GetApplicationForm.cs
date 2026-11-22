using GotFired.Api.Features.Shared;
using GotFired.Api.Infrastructure;
using GotFired.DAL;
using GotFired.Model.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace GotFired.Api.Features.Applications
{
    /// <summary>
    /// GET /application-form: what apply.html needs to render the form, plus a new submission id (guid) that makes a
    /// double-clicked submit harmless.
    /// </summary>
    public static class GetApplicationForm
    {
        public sealed record Query;

        public sealed record SgkReason(int ID, string Code, string Name);

        // Lower-case names: the keys apply.html reads.
        public sealed record Response(
            string guid,
            IReadOnlyList<Lookup> cities,
            IReadOnlyList<Lookup> declaredTerminationReasons,
            IReadOnlyList<Lookup> supportedBys,
            IReadOnlyList<SgkReason> sgkTerminationReasons,
            IReadOnlyList<Lookup> ageIntervals,
            IReadOnlyList<Lookup> educationStates,
            IReadOnlyList<Lookup> dismissalStates,
            IReadOnlyList<Lookup> genders,
            IReadOnlyList<Lookup> employmentDurations,
            IReadOnlyList<Lookup> employeeCounts);

        public sealed class Handler(GotFiredDbContext db) : IQueryHandler<Query, Response>
        {
            public async Task<Response> Handle(Query query, CancellationToken ct) => new(
                Guid.NewGuid().ToString(),
                await db.City.AsNoTracking().OrderBy(c => c.ID).Select(c => new Lookup(c.ID, c.Name)).ToListAsync(ct),
                await db.DeclaredTerminationReason.AsNoTracking().OrderBy(r => r.ID).Select(r => new Lookup(r.ID, r.Name)).ToListAsync(ct),
                await db.SupportedBy.AsNoTracking().OrderBy(s => s.ID).Select(s => new Lookup(s.ID, s.Name)).ToListAsync(ct),
                await db.SGKTerminationReason.AsNoTracking().OrderBy(r => r.ID).Select(r => new SgkReason(r.ID, r.Code, r.Name)).ToListAsync(ct),
                EnumLookups.Of<AgeInterval>(),
                EnumLookups.Of<EducationState>(),
                EnumLookups.Of<DismissalState>(),
                EnumLookups.Of<Gender>(),
                EnumLookups.Of<EmploymentDurationSince>(),
                EnumLookups.Of<EmployeeCount>());
        }

        public static void Map(RouteGroupBuilder api) =>
            api.MapGet("/application-form", async (Handler handler, CancellationToken ct) => Results.Ok(await handler.Handle(new Query(), ct)))
               .AllowAnonymous();
    }
}
