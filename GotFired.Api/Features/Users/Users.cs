using GotFired.Api.Features.Shared;
using GotFired.Api.Infrastructure;
using GotFired.DAL;
using GotFired.DAL.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GotFired.Api.Features.Users
{
    public sealed record UserResponse(string Id, string UserName, string FriendlyName, IReadOnlyList<string> UserRoles);

    /// <summary>GET /users?page=: staff accounts with their role names.</summary>
    public static class ListUsers
    {
        public sealed record Query(int Page, int PageSize);
        public sealed record Item(string UserId, string UserName, IReadOnlyList<string> RoleNames);

        public sealed class Handler(GotFiredDbContext db) : IQueryHandler<Query, Page<Item>>
        {
            public async Task<Page<Item>> Handle(Query query, CancellationToken ct)
            {
                var items = await db.Users.AsNoTracking().OrderBy(u => u.UserName)
                    .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
                    .Select(u => new Item(u.Id, u.UserName,
                        db.UserRoles.Where(ur => ur.UserId == u.Id).Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name).ToList()))
                    .ToListAsync(ct);
                return new Page<Item>(items, new Paging(await db.Users.CountAsync(ct), query.PageSize, query.Page));
            }
        }

        public static void Map(RouteGroupBuilder users) =>
            users.MapGet("/", async (int? page, int? pageSize, Handler handler, CancellationToken ct) =>
            {
                var (p, size) = PagingDefaults.Normalize(page, pageSize);
                return Results.Ok(await handler.Handle(new Query(p, size), ct));
            });
    }

    /// <summary>GET /users/{id}.</summary>
    public static class GetUser
    {
        public sealed record Query(string Id);

        public sealed class Handler(UserManager<ApplicationUser> users) : IQueryHandler<Query, UserResponse>
        {
            public async Task<UserResponse> Handle(Query query, CancellationToken ct)
            {
                var user = await users.FindByIdAsync(query.Id);
                return user == null ? null : new UserResponse(user.Id, user.UserName, user.FriendlyName, [.. await users.GetRolesAsync(user)]);
            }
        }

        public static void Map(RouteGroupBuilder users) =>
            users.MapGet("/{id}", async (string id, Handler handler, CancellationToken ct) =>
                await handler.Handle(new Query(id), ct) is { } user ? Results.Ok(user) : Problems.NotFound());
    }

    /// <summary>POST /users { UserName, Password, FriendlyName }: 201 Created.</summary>
    public static class CreateUser
    {
        public sealed record Command(string UserName, string Password, string FriendlyName);

        public sealed class Handler(UserManager<ApplicationUser> users) : ICommandHandler<Command, UserResponse>
        {
            public async Task<UserResponse> Handle(Command command, CancellationToken ct)
            {
                var user = new ApplicationUser { UserName = command.UserName, FriendlyName = command.FriendlyName };
                IdentityErrors.ThrowIfFailed(await users.CreateAsync(user, command.Password), "Kullanıcı oluşturulamadı");
                return new UserResponse(user.Id, user.UserName, user.FriendlyName, []);
            }
        }

        public static void Map(RouteGroupBuilder users) =>
            users.MapPost("/", async (Command command, Handler handler, CancellationToken ct) =>
            {
                var user = await handler.Handle(command, ct);
                return Results.Created($"/api/v1/users/{user.Id}", user);
            });
    }

    /// <summary>PUT /users/{id} { FriendlyName, UserRoles }: display name and roles (the user name does not change).</summary>
    public static class UpdateUser
    {
        public sealed record Request(string FriendlyName, string[] UserRoles);
        public sealed record Command(string Id, string FriendlyName, string[] Roles);

        public sealed class Handler(UserManager<ApplicationUser> users) : ICommandHandler<Command, bool>
        {
            public async Task<bool> Handle(Command command, CancellationToken ct)
            {
                var user = await users.FindByIdAsync(command.Id);
                if (user == null) return false;

                user.FriendlyName = command.FriendlyName;
                IdentityErrors.ThrowIfFailed(await users.UpdateAsync(user), "Kullanıcı güncellenemedi");
                IdentityErrors.ThrowIfFailed(await users.RemoveFromRolesAsync(user, await users.GetRolesAsync(user)), "Roller güncellenemedi");
                if (command.Roles is { Length: > 0 })
                    IdentityErrors.ThrowIfFailed(await users.AddToRolesAsync(user, command.Roles), "Roller güncellenemedi");
                return true;
            }
        }

        public static void Map(RouteGroupBuilder users) =>
            users.MapPut("/{id}", async (string id, Request request, Handler handler, CancellationToken ct) =>
                await handler.Handle(new Command(id, request.FriendlyName, request.UserRoles ?? []), ct) ? Results.NoContent() : Problems.NotFound());
    }

    /// <summary>PUT /users/{id}/password { Password }: an admin sets a new password.</summary>
    public static class SetPassword
    {
        public sealed record Request(string Password);
        public sealed record Command(string Id, string Password);

        public sealed class Handler(UserManager<ApplicationUser> users) : ICommandHandler<Command, bool>
        {
            public async Task<bool> Handle(Command command, CancellationToken ct)
            {
                var user = await users.FindByIdAsync(command.Id);
                if (user == null) return false;

                // Validate first, so a rejected password does not leave the user without one.
                foreach (var validator in users.PasswordValidators)
                    IdentityErrors.ThrowIfFailed(await validator.ValidateAsync(users, user, command.Password), "Parola kabul edilmedi");
                IdentityErrors.ThrowIfFailed(await users.RemovePasswordAsync(user), "Parola değiştirilemedi");
                IdentityErrors.ThrowIfFailed(await users.AddPasswordAsync(user, command.Password), "Parola değiştirilemedi");
                return true;
            }
        }

        public static void Map(RouteGroupBuilder users) =>
            users.MapPut("/{id}/password", async (string id, Request request, Handler handler, CancellationToken ct) =>
                await handler.Handle(new Command(id, request.Password), ct) ? Results.NoContent() : Problems.NotFound());
    }

    /// <summary>GET /roles.</summary>
    public static class ListRoles
    {
        public sealed record Query;

        public sealed class Handler(RoleManager<IdentityRole> roles) : IQueryHandler<Query, IReadOnlyList<string>>
        {
            public async Task<IReadOnlyList<string>> Handle(Query query, CancellationToken ct) =>
                await roles.Roles.OrderBy(r => r.Name).Select(r => r.Name).ToListAsync(ct);
        }

        public static void Map(RouteGroupBuilder api) =>
            api.MapGet("/roles", async (Handler handler, CancellationToken ct) => Results.Ok(await handler.Handle(new Query(), ct)));
    }

    internal static class IdentityErrors
    {
        /// <summary>Identity rule failures are business-rule violations: 422 with the reason.</summary>
        public static void ThrowIfFailed(IdentityResult result, string message)
        {
            if (result.Succeeded) return;
            bool tooShort = result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.PasswordTooShort));
            throw new GotFired.Model.BusinessRuleException(tooShort
                ? "Kullanıcı parolası 6 karakterden az olamaz."
                : $"{message}: {string.Join(" ", result.Errors.Select(e => e.Description))}");
        }
    }
}
