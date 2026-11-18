using GotFired.Business;
using GotFired.Model.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GotFired.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthBusiness _authBusiness;
        private const int pageSize = 20;

        public AuthController(AuthBusiness authBusiness)
        {
            _authBusiness = authBusiness;
        }

        [Authorize(Roles = "admin, user")]
        [HttpGet("index/{page}")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var users = await _authBusiness.GetUsersAsync(page, pageSize);
            int count = await _authBusiness.GetUsersCountAsync();
            return Ok(new
            {
                data = users,
                paging = new Paging
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = count
                }
            });
        }

        [Authorize(Roles = "admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(UserModel userModel)
        {
            await _authBusiness.RegisterUserAsync(userModel);
            return Ok("Üyelik bilgileriniz alınmıştır.");
        }

        [Authorize(Roles = "admin")]
        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword(UserModel userModel)
        {
            await _authBusiness.ChangePasswordAsync(userModel);
            return Ok("Üyelik bilgileriniz alınmıştır.");
        }

        [Authorize(Roles = "admin, user")]
        [HttpGet("getUserById/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            return Ok(await _authBusiness.GetUserByIdAsync(id));
        }

        [Authorize(Roles = "admin, user")]
        [HttpGet("getUserRoles/{id}")]
        public async Task<IActionResult> GetUserRoles(string id)
        {
            return Ok(await _authBusiness.GetUserRolesAsync(id));
        }

        [Authorize(Roles = "admin, user")]
        [HttpGet("getAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            return Ok(await _authBusiness.GetAllRolesAsync());
        }

        [Authorize(Roles = "admin")]
        [HttpPost("updateUserRoles")]
        public async Task<IActionResult> UpdateUserRoles(UserRoleModel userRoleModel)
        {
            await _authBusiness.UpdateUserRoleModelAsync(userRoleModel);
            return Ok("Üyelik bilgileriniz alınmıştır.");
        }

        // Anonymous self-registration (a POST by Web API 2 convention). Not used by the SPA; kept for parity.
        // Users created here have no role and therefore cannot access any endpoint.
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserModel userModel)
        {
            await _authBusiness.RegisterUserAsync(userModel);
            return Ok("Üyelik bilgileriniz alınmıştır.");
        }
    }
}
