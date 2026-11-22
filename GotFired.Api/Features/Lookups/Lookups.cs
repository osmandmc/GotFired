using GotFired.Api.Features.Shared;
using GotFired.Api.Infrastructure;
using GotFired.DAL;
using GotFired.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace GotFired.Api.Features.Lookups
{
    /// <summary>GET /categories.</summary>
    public static class ListCategories
    {
        public sealed record Query;

        public sealed class Handler(GotFiredDbContext db) : IQueryHandler<Query, IReadOnlyList<Lookup>>
        {
            public async Task<IReadOnlyList<Lookup>> Handle(Query query, CancellationToken ct) =>
                await db.Category.AsNoTracking().OrderBy(c => c.ID).Select(c => new Lookup(c.ID, c.Name)).ToListAsync(ct);
        }

        public static void Map(RouteGroupBuilder api) =>
            api.MapGet("/categories", async (Handler handler, CancellationToken ct) => Results.Ok(await handler.Handle(new Query(), ct)));
    }

    /// <summary>GET /tags?search=: all tags, or those matching the text typed in the tag box.</summary>
    public static class ListTags
    {
        public sealed record Query(string Search);

        public sealed class Handler(GotFiredDbContext db) : IQueryHandler<Query, IReadOnlyList<Lookup>>
        {
            public async Task<IReadOnlyList<Lookup>> Handle(Query query, CancellationToken ct)
            {
                IQueryable<Tag> tags = db.Tag.AsNoTracking();
                if (!string.IsNullOrEmpty(query.Search))
                    tags = tags.Where(t => query.Search.Contains(t.Name) || t.Name.Contains(query.Search));
                return await tags.OrderBy(t => t.ID).Select(t => new Lookup(t.ID, t.Name)).ToListAsync(ct);
            }
        }

        public static void Map(RouteGroupBuilder api) =>
            api.MapGet("/tags", async (string search, Handler handler, CancellationToken ct) => Results.Ok(await handler.Handle(new Query(search), ct)));
    }
}
