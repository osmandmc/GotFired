using GotFired.Api.Features.Cases;
using GotFired.Api.Features.Shared;
using GotFired.Api.Infrastructure;
using GotFired.DAL;
using GotFired.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GotFired.Api.Features.ApplicantAccess
{
    /// <summary>
    /// The applicant's own case, reached only with the secret link token from their email, sent in the
    /// X-Applicant-Token header (userView.html reads it from the URL fragment). An invalid or expired token is a 404,
    /// so the API does not reveal whether a case exists.
    /// </summary>
    public static class ApplicantCase
    {
        public const string TokenHeader = "X-Applicant-Token";

        internal static async Task<int?> FindCaseIdAsync(GotFiredDbContext db, string token, DateTime utcNow, CancellationToken ct)
        {
            byte[] hash = ApplicantAccessToken.TryHash(token);
            if (hash == null) return null;
            return await db.DismissalCase.AsNoTracking().Where(DismissalCase.AccessibleWith(hash, utcNow))
                .Select(d => (int?)d.ID).SingleOrDefaultAsync(ct);
        }

        internal static void NoStore(HttpContext http) => http.Response.Headers.CacheControl = "no-store";
    }

    /// <summary>GET /applicant/case: what the applicant reported and the conversation, without staff notes.</summary>
    public static class GetApplicantCase
    {
        public sealed record Query(string Token);

        public sealed record Response : CaseFacts
        {
            public IReadOnlyList<CommentResponse> Comments { get; init; }
        }

        public sealed class Handler(GotFiredDbContext db, TimeProvider time) : IQueryHandler<Query, Response>
        {
            public async Task<Response> Handle(Query query, CancellationToken ct)
            {
                int? caseId = await ApplicantCase.FindCaseIdAsync(db, query.Token, time.GetUtcNow().UtcDateTime, ct);
                if (caseId == null) return null;

                var cases = db.DismissalCase.AsNoTracking().Where(d => d.ID == caseId);
                var row = await cases.Select(CaseFacts.Select).SingleAsync(ct);
                string guid = await cases.Select(d => d.Guid).SingleAsync(ct);

                // The conversation spans every case sharing this Guid: double-clicked submissions used to store
                // several cases with one Guid (74 in production).
                var comments = await db.Comment.AsNoTracking()
                    .Where(c => c.DismissalCaseID == caseId || db.DismissalCase.Any(d => d.ID == c.DismissalCaseID && guid != null && d.Guid == guid))
                    .OrderBy(c => c.CreatedDate).ThenBy(c => c.ID)
                    .Select(c => new CommentResponse(c.ID, c.Text, c.UserName, c.CreatedDate))
                    .ToListAsync(ct);
                return CaseFacts.ToFacts<Response>(row) with { Comments = comments };
            }
        }

        public static void Map(RouteGroupBuilder api) =>
            api.MapGet("/applicant/case", async ([FromHeader(Name = ApplicantCase.TokenHeader)] string token, HttpContext http, Handler handler, CancellationToken ct) =>
            {
                ApplicantCase.NoStore(http);
                return await handler.Handle(new Query(token), ct) is { } response ? Results.Ok(response) : Problems.NotFound();
            })
            .AllowAnonymous();
    }

    /// <summary>POST /applicant/case/comments { Text }: the applicant's reply; the case goes back to the staff queue.</summary>
    public static class ReplyToCase
    {
        public sealed record Request(string Text);
        public sealed record Command(string Token, string Text);

        public sealed class Handler(GotFiredDbContext db, TimeProvider time) : ICommandHandler<Command, CommentResponse>
        {
            public async Task<CommentResponse> Handle(Command command, CancellationToken ct)
            {
                DateTime now = time.GetUtcNow().UtcDateTime;
                int? caseId = await ApplicantCase.FindCaseIdAsync(db, command.Token, now, ct);
                if (caseId == null) return null;

                var comment = await ConcurrencyRetry.RunAsync(db, async () =>
                {
                    var dismissalCase = await db.DismissalCase.Include(d => d.Applicant).SingleAsync(d => d.ID == caseId, ct);
                    var reply = dismissalCase.ReplyAsApplicant(command.Token, command.Text, now);
                    await db.SaveChangesAsync(ct);
                    return reply;
                });
                return new CommentResponse(comment.ID, comment.Text, comment.UserName, comment.CreatedDate);
            }
        }

        public static void Map(RouteGroupBuilder api) =>
            api.MapPost("/applicant/case/comments", async ([FromHeader(Name = ApplicantCase.TokenHeader)] string token, Request request, HttpContext http, Handler handler, CancellationToken ct) =>
            {
                ApplicantCase.NoStore(http);
                return await handler.Handle(new Command(token, request.Text), ct) is { } comment
                    ? Results.Created("/api/v1/applicant/case", comment)
                    : Problems.NotFound();
            })
            .AllowAnonymous();
    }
}
