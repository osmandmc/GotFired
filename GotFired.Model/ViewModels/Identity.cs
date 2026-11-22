using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model.ViewModels
{
    public class UserModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string FriendlyName { get; set; }
    }
    public class UserListItemModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public IEnumerable<string> RoleIds { get; set; }
        public IEnumerable<string> RoleNames { get; set; }

    }
    public class UserRoleModel
    {
        //public string UserId { get; set; }
        public string UserName { get; set; }
        public string FriendlyName { get; set; }
        public IList<string> UserRoles { get; set; }
    }
    public class Role
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class UserTokenModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public Dictionary<string, string> Claims { get; set; }
        public ICollection<string> Roles { get; set; }
    }
}
