using GotFired.Api.Features.Shared;
using GotFired.Api.Infrastructure;
using GotFired.DAL;
using GotFired.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace GotFired.Api.Features.AnswerTemplates
{
    public sealed record TemplateResponse(int ID, string Name, string Content);

    public sealed record TemplateRequest(string Name, string Content);

    /// <summary>GET /answer-templates?page=&amp;pageSize=: the template list and the answer editor's drop-down.</summary>
    public static class ListTemplates
    {
        public sealed record Query(int Page, int PageSize);

        public sealed class Handler(GotFiredDbContext db) : IQueryHandler<Query, Page<Lookup>>
        {
            public async Task<Page<Lookup>> Handle(Query query, CancellationToken ct)
            {
                var items = await db.AnswerTemplate.AsNoTracking().OrderBy(t => t.ID)
                    .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
                    .Select(t => new Lookup(t.ID, t.Name)).ToListAsync(ct);
                return new Page<Lookup>(items, new Paging(await db.AnswerTemplate.CountAsync(ct), query.PageSize, query.Page));
            }
        }

        public static void Map(RouteGroupBuilder templates) =>
            templates.MapGet("/", async (int? page, int? pageSize, Handler handler, CancellationToken ct) =>
            {
                var (p, size) = PagingDefaults.Normalize(page, pageSize);
                return Results.Ok(await handler.Handle(new Query(p, size), ct));
            });
    }

    /// <summary>GET /answer-templates/{id}: the template, with its version as the ETag (needed to update it).</summary>
    public static class GetTemplate
    {
        public sealed record Query(int Id);
        public sealed record Result(TemplateResponse Template, string Version);

        public sealed class Handler(GotFiredDbContext db) : IQueryHandler<Query, Result>
        {
            public async Task<Result> Handle(Query query, CancellationToken ct) =>
                await db.AnswerTemplate.AsNoTracking().Where(t => t.ID == query.Id)
                    .Select(t => new Result(new TemplateResponse(t.ID, t.Name, t.Content), t.RowVersion))
                    .SingleOrDefaultAsync(ct);
        }

        public static void Map(RouteGroupBuilder templates) =>
            templates.MapGet("/{id:int}", async (int id, HttpContext http, Handler handler, CancellationToken ct) =>
            {
                var result = await handler.Handle(new Query(id), ct);
                if (result == null) return Problems.NotFound();
                http.Response.Headers.ETag = ETag.From(result.Version);
                return Results.Ok(result.Template);
            });
    }

    /// <summary>POST /answer-templates: 201 Created with Location.</summary>
    public static class CreateTemplate
    {
        public sealed record Command(string Name, string Content);

        public sealed class Handler(GotFiredDbContext db) : ICommandHandler<Command, TemplateResponse>
        {
            public async Task<TemplateResponse> Handle(Command command, CancellationToken ct)
            {
                var template = AnswerTemplate.Create(command.Name, command.Content);
                db.AnswerTemplate.Add(template);
                await db.SaveChangesAsync(ct);
                return new TemplateResponse(template.ID, template.Name, template.Content);
            }
        }

        public static void Map(RouteGroupBuilder templates) =>
            templates.MapPost("/", async (TemplateRequest request, Handler handler, CancellationToken ct) =>
            {
                var created = await handler.Handle(new Command(request.Name, request.Content), ct);
                return Results.Created($"/api/v1/answer-templates/{created.ID}", created);
            });
    }

    /// <summary>
    /// PUT /answer-templates/{id} with If-Match: the version from GET. 204 when saved, 412 Precondition Failed when
    /// someone else saved it meanwhile, 428 Precondition Required without If-Match.
    /// </summary>
    public static class UpdateTemplate
    {
        public sealed record Command(int Id, string Name, string Content, string ExpectedVersion);
        public enum Outcome { Updated, NotFound, Stale }

        public sealed class Handler(GotFiredDbContext db) : ICommandHandler<Command, Outcome>
        {
            public async Task<Outcome> Handle(Command command, CancellationToken ct)
            {
                var template = await db.AnswerTemplate.SingleOrDefaultAsync(t => t.ID == command.Id, ct);
                if (template == null) return Outcome.NotFound;

                // Optimistic concurrency: the UPDATE only matches the version the client loaded.
                db.Entry(template).Property(t => t.RowVersion).OriginalValue = command.ExpectedVersion;
                template.Edit(command.Name, command.Content);
                try
                {
                    await db.SaveChangesAsync(ct);
                    return Outcome.Updated;
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Outcome.Stale;
                }
            }
        }

        public static void Map(RouteGroupBuilder templates) =>
            templates.MapPut("/{id:int}", async (int id, TemplateRequest request, HttpContext http, Handler handler, CancellationToken ct) =>
            {
                string version = ETag.Parse(http.Request.Headers.IfMatch);
                if (version == null)
                    return Results.Problem(statusCode: StatusCodes.Status428PreconditionRequired, title: "If-Match başlığı gerekli.");

                return await handler.Handle(new Command(id, request.Name, request.Content, version), ct) switch
                {
                    Outcome.Updated => Results.NoContent(),
                    Outcome.NotFound => Problems.NotFound(),
                    _ => Results.Problem(statusCode: StatusCodes.Status412PreconditionFailed,
                        title: "Başka bir kullanıcı güncelleme yapmakta, lütfen sayfayı yenileyip tekrar deneyin."),
                };
            });
    }

    internal static class ETag
    {
        public static string From(string version) => $"\"{version}\"";

        public static string Parse(string ifMatch) =>
            string.IsNullOrWhiteSpace(ifMatch) ? null : ifMatch.Trim().TrimStart('W', '/').Trim('"');
    }
}
