using GotFired.DAL;
using GotFired.Model.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GotFired.DAL
{
    public class AuthRepository
    {
        internal GotFiredDbContext _context;
        internal UserManager<ApplicationUser> _userManager;
        internal RoleManager<IdentityRole> _roleManager;

        public AuthRepository(GotFiredDbContext context)
        {
            _context = context;
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
        }
        public int GetUsersCount()
        {
            return _userManager.Users.Count();
        }
        public IEnumerable<UserListItemModel> GetUsers(int page, int pageSize)
        {
            var users = _userManager.Users.Select(u => new UserListItemModel { UserId = u.Id, UserName = u.UserName,  RoleIds = u.Roles.Select(r => r.RoleId).ToList() }).OrderBy(u => u.UserName).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            foreach (var item in users)
            {
                item.RoleNames = _roleManager.Roles.Where(r => item.RoleIds.Contains(r.Id)).Select(r => r.Name).ToList();
            }
            return users;

        }
        public void Register(UserModel userModel)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = userModel.UserName,
                FriendlyName = userModel.FriendlyName

            };
            IdentityResult result = _userManager.Create(user, userModel.Password);
            if (!result.Succeeded) throw new Exception("Kaydınız oluşturulamadı.");
        }

        public UserTokenModel FindUser(string userName, string password)
        {
            IdentityUser result = _userManager.Find(userName, password);
            
            return new UserTokenModel
            {
                Email = result.Email,
                EmailConfirmed = result.EmailConfirmed,
                PhoneNumber = result.PhoneNumber,
                Roles = result.Roles.Select(s => _roleManager.Roles.First(f=> f.Id == s.RoleId).Name).ToList(),
                Claims = result.Claims.ToDictionary(d => d.ClaimType, d => d.ClaimValue)
            };
        }

        public UserModel GetUserById(string id)
        {
            ApplicationUser result = _userManager.FindById(id);
            return new UserModel { UserName = result.UserName };
        }

        public void ChangePassword(UserModel model)
        {
            var user = _userManager.FindByName(model.UserName);
            _userManager.RemovePassword(user.Id);
            var AddPassword = _userManager.AddPassword(user.Id, model.Password);
            if (!AddPassword.Succeeded)
            {
                throw new Exception("Kullanıcı parolası 6 karaketerden az olamaz.");
            }

        }

        public UserRoleModel GetUserRoles(string id)
        {
            //var user = _context.ApplicationUser.Where(s=>s.Id == id).FirstOrDefault();
            var user = _userManager.Users.Where(s => s.Id == id).FirstOrDefault();
            var userRoles = _userManager.GetRoles(id);
            return new UserRoleModel
            {
                UserName = user.UserName,
                FriendlyName = user.FriendlyName,
                UserRoles = userRoles
            };
        }
        public void UpdateUserRoleModel(UserRoleModel userRoleModel)
        {
            var applicationUser = _userManager.FindByName(userRoleModel.UserName);
            applicationUser.FriendlyName = userRoleModel.FriendlyName;
            _userManager.Update(applicationUser);
            _userManager.RemoveFromRoles(applicationUser.Id, _userManager.GetRoles(applicationUser.Id).ToArray());
            foreach (var item in userRoleModel.UserRoles)
            {
                _userManager.AddToRole(applicationUser.Id, item);
            }
        }
        public IList<string> GetAllRoles()
        {
            return _roleManager.Roles.Select(r=>r.Name).ToList();
        }

    }
}
