using GotFired.Model.Identity;
using GotFired.Model.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GotFired.DAL
{
    public class AuthRepository
    {
        private readonly GotFiredDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthRepository(GotFiredDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public Task<int> GetUsersCountAsync()
        {
            return _userManager.Users.CountAsync();
        }

        public async Task<IEnumerable<UserListItemModel>> GetUsersAsync(int page, int pageSize)
        {
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListItemModel
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    RoleIds = _context.UserRoles.Where(r => r.UserId == u.Id).Select(r => r.RoleId).ToList()
                })
                .ToListAsync();

            var roleNames = await _roleManager.Roles.ToDictionaryAsync(r => r.Id, r => r.Name);
            foreach (var item in users)
            {
                item.RoleNames = item.RoleIds.Where(roleNames.ContainsKey).Select(id => roleNames[id]).ToList();
            }
            return users;
        }

        public async Task RegisterAsync(UserModel userModel)
        {
            var user = new ApplicationUser
            {
                UserName = userModel.UserName,
                FriendlyName = userModel.FriendlyName
            };
            IdentityResult result = await _userManager.CreateAsync(user, userModel.Password);
            if (!result.Succeeded) throw new Exception("Kaydınız oluşturulamadı.");
        }

        /// <summary>Returns null when the user does not exist or the password is wrong.</summary>
        public async Task<UserTokenModel> FindUserAsync(string userName, string password)
        {
            var user = await _userManager.FindByNameAsync(userName ?? string.Empty);
            if (user == null || !await _userManager.CheckPasswordAsync(user, password ?? string.Empty))
            {
                return null;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            return new UserTokenModel
            {
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                Roles = await _userManager.GetRolesAsync(user),
                Claims = claims.GroupBy(c => c.Type).ToDictionary(g => g.Key, g => g.First().Value)
            };
        }

        public async Task<UserModel> GetUserByIdAsync(string id)
        {
            ApplicationUser result = await _userManager.FindByIdAsync(id);
            return new UserModel { UserName = result.UserName };
        }

        public async Task ChangePasswordAsync(UserModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            await _userManager.RemovePasswordAsync(user);
            var addPassword = await _userManager.AddPasswordAsync(user, model.Password);
            if (!addPassword.Succeeded)
            {
                throw new Exception("Kullanıcı parolası 6 karaketerden az olamaz.");
            }
        }

        public async Task<UserRoleModel> GetUserRolesAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return new UserRoleModel
            {
                UserName = user.UserName,
                FriendlyName = user.FriendlyName,
                UserRoles = await _userManager.GetRolesAsync(user)
            };
        }

        public async Task UpdateUserRoleModelAsync(UserRoleModel userRoleModel)
        {
            var applicationUser = await _userManager.FindByNameAsync(userRoleModel.UserName);
            applicationUser.FriendlyName = userRoleModel.FriendlyName;
            await _userManager.UpdateAsync(applicationUser);
            await _userManager.RemoveFromRolesAsync(applicationUser, await _userManager.GetRolesAsync(applicationUser));
            if (userRoleModel.UserRoles != null)
            {
                await _userManager.AddToRolesAsync(applicationUser, userRoleModel.UserRoles);
            }
        }

        public async Task<IList<string>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        }
    }
}
