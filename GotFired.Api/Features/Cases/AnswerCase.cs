using System.Security.Claims;
using GotFired.Api.Features.ApplicantAccess;
using GotFired.Api.Features.Shared;
using GotFired.Api.Infrastructure;
using GotFired.Api.Infrastructure.Email;
using GotFired.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GotFired.Api.Features.Cases
{
    /// <summary>
    /// POST /cases/{id}/comments: a staff answer. The case becomes Answered and the applicant is emailed a new
    /// secret link (replacing the previous one). 201 with the stored comment.
    /// </summary>
    public static class AnswerCase
    {
        public sealed record Request(string Text);

        public sealed record Command(int CaseId, string Text, string StaffUserName);

        public sealed class Handler(GotFiredDbContext db, IEmailSender email, IOptions<ApplicantLinkOptions> links, TimeProvider time)
            : ICommandHandler<Command, CommentResponse>
        {
            public async Task<CommentResponse> Handle(Command command, CancellationToken ct)
            {
                var saved = await ConcurrencyRetry.RunAsync(db, async () =>
                {
                    var dismissalCase = await db.DismissalCase.Include(d => d.Applicant).SingleOrDefaultAsync(d => d.ID == command.CaseId, ct);
                    if (dismissalCase == null) return default;

                    var answer = dismissalCase.AnswerByStaff(command.Text, command.StaffUserName, time.GetUtcNow().UtcDateTime, links.Value.Lifetime);
                    await db.SaveChangesAsync(ct);
                    return (dismissalCase.Applicant.Email, answer);
                });
                if (saved.answer == null) return null;
                var (applicantEmail, answer) = saved;

                // After saving: the applicant is only told about an answer that exists.
                if (!string.IsNullOrWhiteSpace(applicantEmail))
                {
                    email.SendApplicantLink(applicantEmail, answer.LinkToken);
                }
                var c = answer.Comment;
                return new CommentResponse(c.ID, c.Text, c.UserName, c.CreatedDate);
            }
        }

        public static void Map(RouteGroupBuilder cases) =>
            cases.MapPost("/{id:int}/comments", async (int id, Request request, ClaimsPrincipal user, Handler handler, CancellationToken ct) =>
                await handler.Handle(new Command(id, request.Text, user.Identity!.Name), ct) is { } comment
                    ? Results.Created($"/api/v1/cases/{id}", comment)
                    : Problems.NotFound());
    }
}
