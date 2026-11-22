using GotFired.Api.Features.Shared;
using GotFired.Api.Infrastructure;
using GotFired.DAL;
using GotFired.Model.Entities;
using GotFired.Model.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace GotFired.Api.Features.Cases
{
    /// <summary>GET /cases?state=pending|answered&amp;page=: the staff queue, most recently active first.</summary>
    public static class ListCases
    {
        public sealed record Query(AppealState? State, int Page, int PageSize);

        public sealed record Item(int Id, DateTime CreatedDate, DateTime CommentDate, string ApplicantFullName, string ApplicantEmail, AppealState AppealState, string CommentedBy);

        public sealed class Handler(GotFiredDbContext db) : IQueryHandler<Query, Page<Item>>
        {
            public async Task<Page<Item>> Handle(Query query, CancellationToken ct)
            {
                IQueryable<DismissalCase> cases = db.DismissalCase.AsNoTracking();
                if (query.State != null) cases = cases.Where(d => d.AppealState == query.State);

                var items = await cases
                    .OrderByDescending(d => d.EditedDate).ThenByDescending(d => d.ID)
                    .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
                    .Select(d => new Item(d.ID, d.CreatedDate, d.EditedDate, d.Applicant.FullName, d.Applicant.Email, d.AppealState,
                        d.Comments.OrderByDescending(c => c.CreatedDate).Select(c => c.UserName).FirstOrDefault() ?? ""))
                    .ToListAsync(ct);
                return new Page<Item>(items, new Paging(await cases.CountAsync(ct), query.PageSize, query.Page));
            }
        }

        public static void Map(RouteGroupBuilder cases) =>
            cases.MapGet("/", async (string state, int? page, int? pageSize, Handler handler, CancellationToken ct) =>
            {
                AppealState? filter = null;
                if (!string.IsNullOrEmpty(state))
                {
                    if (!Enum.TryParse(state, ignoreCase: true, out AppealState parsed) || !Enum.IsDefined(parsed))
                        return Results.ValidationProblem(new Dictionary<string, string[]> { ["state"] = ["pending veya answered olmalı."] });
                    filter = parsed;
                }
                var (p, size) = PagingDefaults.Normalize(page, pageSize);
                return Results.Ok(await handler.Handle(new Query(filter, p, size), ct));
            });
    }
}
