using GotFired.Api.Features.Shared;
using GotFired.Api.Infrastructure;
using GotFired.DAL;
using GotFired.Model.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace GotFired.Api.Features.Cases
{
    /// <summary>GET /cases/{id}: everything staff see about a case.</summary>
    public static class GetCase
    {
        public sealed record Query(int Id);

        public sealed record Response : CaseFacts
        {
            public int Id { get; init; }
            public AppealState AppealState { get; init; }
            public string Notes { get; init; }
            public int? CategoryId { get; init; }
            public IReadOnlyList<Lookup> DismissalCaseTags { get; init; }
            public IReadOnlyList<CommentResponse> Comments { get; init; }
        }

        public sealed class Handler(GotFiredDbContext db) : IQueryHandler<Query, Response>
        {
            public async Task<Response> Handle(Query query, CancellationToken ct)
            {
                var cases = db.DismissalCase.AsNoTracking().Where(d => d.ID == query.Id);
                var row = await cases.Select(CaseFacts.Select).SingleOrDefaultAsync(ct);
                if (row == null) return null;

                var state = await cases.Select(d => new { d.AppealState, d.Notes, d.CategoryID }).SingleAsync(ct);
                return CaseFacts.ToFacts<Response>(row) with
                {
                    Id = query.Id,
                    AppealState = state.AppealState,
                    Notes = state.Notes,
                    CategoryId = state.CategoryID,
                    DismissalCaseTags = await db.DismissalCaseTag.AsNoTracking().Where(t => t.DismissalCaseID == query.Id)
                        .Select(t => new Lookup(t.TagID, t.Tag.Name)).ToListAsync(ct),
                    Comments = await db.Comment.AsNoTracking().Where(c => c.DismissalCaseID == query.Id)
                        .OrderBy(c => c.CreatedDate).ThenBy(c => c.ID)
                        .Select(c => new CommentResponse(c.ID, c.Text, c.UserName, c.CreatedDate)).ToListAsync(ct),
                };
            }
        }

        public static void Map(RouteGroupBuilder cases) =>
            cases.MapGet("/{id:int}", async (int id, Handler handler, CancellationToken ct) =>
                await handler.Handle(new Query(id), ct) is { } response ? Results.Ok(response) : Problems.NotFound());
    }
}
