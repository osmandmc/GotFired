using GotFired.DAL;
using GotFired.DAL.Identity;
using GotFired.Model.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GotFired.Business.Services
{
    public interface IUserService
    {
        /// <summary>The user and roles for a valid user name and password; null otherwise.</summary>
        Task<UserTokenModel> FindUserAsync(string userName, string password);
        Task<(IReadOnlyList<UserListItemModel> Users, int Total)> ListAsync(int page, int pageSize);
        Task<UserModel> GetAsync(string id);
        Task<UserRoleModel> GetRolesAsync(string id);
        Task<IReadOnlyList<string>> GetAllRolesAsync();
        Task CreateAsync(UserModel model);
        Task ChangePasswordAsync(UserModel model);
        Task UpdateRolesAsync(UserRoleModel model);
    }

    public class UserService(GotFiredDbContext db, UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles) : IUserService
    {
        public async Task<UserTokenModel> FindUserAsync(string userName, string password)
        {
            var user = await users.FindByNameAsync(userName ?? string.Empty);
            if (user == null || !await users.CheckPasswordAsync(user, password ?? string.Empty))
            {
                return null;
            }
            return new UserTokenModel
            {
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                Roles = await users.GetRolesAsync(user),
                Claims = (await users.GetClaimsAsync(user)).GroupBy(c => c.Type).ToDictionary(g => g.Key, g => g.First().Value)
            };
        }

        public async Task<(IReadOnlyList<UserListItemModel> Users, int Total)> ListAsync(int page, int pageSize)
        {
            var list = await db.Users
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(u => new UserListItemModel
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    RoleIds = db.UserRoles.Where(r => r.UserId == u.Id).Select(r => r.RoleId).ToList(),
                    RoleNames = db.UserRoles.Where(r => r.UserId == u.Id)
                        .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name).ToList()
                })
                .ToListAsync();
            return (list, await db.Users.CountAsync());
        }

        public async Task<UserModel> GetAsync(string id) =>
            new() { UserName = (await FindAsync(id)).UserName };

        public async Task<UserRoleModel> GetRolesAsync(string id)
        {
            var user = await FindAsync(id);
            return new UserRoleModel { UserName = user.UserName, FriendlyName = user.FriendlyName, UserRoles = await users.GetRolesAsync(user) };
        }

        public async Task<IReadOnlyList<string>> GetAllRolesAsync() =>
            await roles.Roles.OrderBy(r => r.Name).Select(r => r.Name).ToListAsync();

        public async Task CreateAsync(UserModel model)
        {
            var result = await users.CreateAsync(new ApplicationUser { UserName = model.UserName, FriendlyName = model.FriendlyName }, model.Password);
            ThrowIfFailed(result, "Kullanıcı oluşturulamadı");
        }

        public async Task ChangePasswordAsync(UserModel model)
        {
            var user = await users.FindByNameAsync(model.UserName ?? string.Empty)
                       ?? throw new BusinessRuleException("Kullanıcı bulunamadı.");
            // Validate first so a rejected password does not leave the user without one.
            foreach (var validator in users.PasswordValidators)
            {
                ThrowIfFailed(await validator.ValidateAsync(users, user, model.Password), "Parola kabul edilmedi");
            }
            ThrowIfFailed(await users.RemovePasswordAsync(user), "Parola değiştirilemedi");
            ThrowIfFailed(await users.AddPasswordAsync(user, model.Password), "Parola değiştirilemedi");
        }

        public async Task UpdateRolesAsync(UserRoleModel model)
        {
            var user = await users.FindByNameAsync(model.UserName ?? string.Empty)
                       ?? throw new BusinessRuleException("Kullanıcı bulunamadı.");
            user.FriendlyName = model.FriendlyName;
            ThrowIfFailed(await users.UpdateAsync(user), "Kullanıcı güncellenemedi");
            ThrowIfFailed(await users.RemoveFromRolesAsync(user, await users.GetRolesAsync(user)), "Roller güncellenemedi");
            if (model.UserRoles is { Count: > 0 })
            {
                ThrowIfFailed(await users.AddToRolesAsync(user, model.UserRoles), "Roller güncellenemedi");
            }
        }

        private async Task<ApplicationUser> FindAsync(string id) =>
            await users.FindByIdAsync(id ?? string.Empty) ?? throw new BusinessRuleException("Kullanıcı bulunamadı.");

        private static void ThrowIfFailed(IdentityResult result, string message)
        {
            if (result.Succeeded) return;
            bool tooShort = result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.PasswordTooShort));
            throw new BusinessRuleException(tooShort
                ? "Kullanıcı parolası 6 karakterden az olamaz."
                : $"{message}: {string.Join(" ", result.Errors.Select(e => e.Description))}");
        }
    }
}
