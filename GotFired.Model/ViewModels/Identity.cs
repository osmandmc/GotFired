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
        //[Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        //[DataType(DataType.Password)]
        //[Display(Name = "Password")]
        public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm password")]
        //[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
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
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public Dictionary<string, string> Claims { get; set; }
        public ICollection<string> Roles { get; set; }
    }
}
