using Microsoft.AspNetCore.Identity;

namespace GotFired.Model.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FriendlyName { get; set; }
    }
}
