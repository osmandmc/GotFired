using GotFired.Business.Services;
using GotFired.Model.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GotFired.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController(IUserService users) : ControllerBase
    {
        private const int PageSize = 20;
        // The SPA shows this text after every successful change (Web API 2 returned it as a JSON string).
        private const string Saved = "Üyelik bilgileriniz alınmıştır.";

        [Authorize(Roles = "admin, user")]
        [HttpGet("index/{page}")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var (list, total) = await users.ListAsync(page, PageSize);
            return Ok(new { data = list, paging = new Paging { CurrentPage = page, ItemsPerPage = PageSize, TotalItems = total } });
        }

        [Authorize(Roles = "admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(UserModel userModel)
        {
            await users.CreateAsync(userModel);
            return Ok(Saved);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword(UserModel userModel)
        {
            await users.ChangePasswordAsync(userModel);
            return Ok(Saved);
        }

        [Authorize(Roles = "admin, user")]
        [HttpGet("getUserById/{id}")]
        public async Task<IActionResult> GetUserById(string id) => Ok(await users.GetAsync(id));

        [Authorize(Roles = "admin, user")]
        [HttpGet("getUserRoles/{id}")]
        public async Task<IActionResult> GetUserRoles(string id) => Ok(await users.GetRolesAsync(id));

        [Authorize(Roles = "admin, user")]
        [HttpGet("getAllRoles")]
        public async Task<IActionResult> GetAllRoles() => Ok(await users.GetAllRolesAsync());

        [Authorize(Roles = "admin")]
        [HttpPost("updateUserRoles")]
        public async Task<IActionResult> UpdateUserRoles(UserRoleModel userRoleModel)
        {
            await users.UpdateRolesAsync(userRoleModel);
            return Ok(Saved);
        }

        // Anonymous self-registration (a POST by Web API 2 convention). Not used by the SPA; kept for parity.
        // Users created here have no role and therefore cannot access any endpoint.
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserModel userModel)
        {
            await users.CreateAsync(userModel);
            return Ok(Saved);
        }
    }
}
