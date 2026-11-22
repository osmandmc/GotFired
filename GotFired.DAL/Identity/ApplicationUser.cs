using Microsoft.AspNetCore.Identity;

namespace GotFired.DAL.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FriendlyName { get; set; }
    }
}
