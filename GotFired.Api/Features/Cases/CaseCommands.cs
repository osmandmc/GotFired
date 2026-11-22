using GotFired.Api.Features.Shared;
using GotFired.Api.Infrastructure;
using GotFired.DAL;
using GotFired.Model.Entities;
using GotFired.Model.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace GotFired.Api.Features.Cases
{
    // Small staff commands on a case. Each loads the aggregate, calls one domain method and saves.
    // They return false when the case does not exist (404) and 204 No Content on success.

    /// <summary>PUT /cases/{id}/notes { Notes }: staff-only notes.</summary>
    public static class UpdateNotes
    {
        public sealed record Request(string Notes);
        public sealed record Command(int CaseId, string Notes);

        public sealed class Handler(GotFiredDbContext db) : ICommandHandler<Command, bool>
        {
            public async Task<bool> Handle(Command command, CancellationToken ct) =>
                await CaseCommand.Run(db, command.CaseId, includeTags: false, c => c.UpdateNotes(command.Notes), ct);
        }

        public static void Map(RouteGroupBuilder cases) =>
            cases.MapPut("/{id:int}/notes", async (int id, Request request, Handler handler, CancellationToken ct) =>
                CaseCommand.Result(await handler.Handle(new Command(id, request.Notes), ct)));
    }

    /// <summary>
    /// PUT /cases/{id}/appeal-state { AppealState }: staff mark a case as handled. Only Answered can be set this
    /// way; Pending only happens through an applicant reply.
    /// </summary>
    public static class SetAppealState
    {
        public sealed record Request(AppealState AppealState);
        public sealed record Command(int CaseId);

        public sealed class Handler(GotFiredDbContext db) : ICommandHandler<Command, bool>
        {
            public async Task<bool> Handle(Command command, CancellationToken ct) =>
                await CaseCommand.Run(db, command.CaseId, includeTags: false, c => c.MarkAnswered(), ct);
        }

        public static void Map(RouteGroupBuilder cases) =>
            cases.MapPut("/{id:int}/appeal-state", async (int id, Request request, Handler handler, CancellationToken ct) =>
                request.AppealState != AppealState.Answered
                    ? Results.ValidationProblem(new Dictionary<string, string[]> { ["AppealState"] = ["Sadece 'Answered' (2) atanabilir."] })
                    : CaseCommand.Result(await handler.Handle(new Command(id), ct)));
    }

    /// <summary>PUT /cases/{id}/tags { TagIds }: replaces the case's tags (evaluation).</summary>
    public static class SetTags
    {
        public sealed record Request(int[] TagIds);
        public sealed record Command(int CaseId, int[] TagIds);

        public sealed class Handler(GotFiredDbContext db) : ICommandHandler<Command, bool>
        {
            public async Task<bool> Handle(Command command, CancellationToken ct) =>
                await CaseCommand.Run(db, command.CaseId, includeTags: true, c => c.Evaluate(command.TagIds), ct);
        }

        public static void Map(RouteGroupBuilder cases) =>
            cases.MapPut("/{id:int}/tags", async (int id, Request request, Handler handler, CancellationToken ct) =>
                CaseCommand.Result(await handler.Handle(new Command(id, request.TagIds ?? []), ct)));
    }

    /// <summary>PUT /cases/{id}/tags/{tagId} (idempotent) and DELETE /cases/{id}/tags/{tagId}.</summary>
    public static class CaseTag
    {
        public sealed record Command(int CaseId, int TagId, bool Present);

        public sealed class Handler(GotFiredDbContext db) : ICommandHandler<Command, bool>
        {
            public async Task<bool> Handle(Command command, CancellationToken ct) =>
                await CaseCommand.Run(db, command.CaseId, includeTags: true, c =>
                {
                    if (command.Present) c.AddTag(command.TagId);
                    else c.RemoveTag(command.TagId);
                }, ct);
        }

        public static void Map(RouteGroupBuilder cases)
        {
            cases.MapPut("/{id:int}/tags/{tagId:int}", async (int id, int tagId, Handler handler, CancellationToken ct) =>
                CaseCommand.Result(await handler.Handle(new Command(id, tagId, Present: true), ct)));
            cases.MapDelete("/{id:int}/tags/{tagId:int}", async (int id, int tagId, Handler handler, CancellationToken ct) =>
                CaseCommand.Result(await handler.Handle(new Command(id, tagId, Present: false), ct)));
        }
    }

    internal static class CaseCommand
    {
        public static Task<bool> Run(GotFiredDbContext db, int caseId, bool includeTags, Action<DismissalCase> change, CancellationToken ct) =>
            ConcurrencyRetry.RunAsync(db, async () =>
            {
                IQueryable<DismissalCase> query = db.DismissalCase;
                if (includeTags) query = query.Include(d => d.Tags);
                var dismissalCase = await query.SingleOrDefaultAsync(d => d.ID == caseId, ct);
                if (dismissalCase == null) return false;

                change(dismissalCase);
                await db.SaveChangesAsync(ct);
                return true;
            });

        public static IResult Result(bool found) => found ? Results.NoContent() : Problems.NotFound();
    }
}
