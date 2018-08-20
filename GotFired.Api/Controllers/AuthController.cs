using GotFired.Business;
using GotFired.Model.ViewModels;
using System.Web.Http;

namespace GotFired.Api.Controllers
{
    [RoutePrefix("api/v1/auth")]
  
    public class AuthController : ApiController
    {
        readonly AuthBusiness _authBusiness;
        const int pageSize = 20;
        public AuthController()
        {
            _authBusiness = new AuthBusiness();
        }
        [Authorize(Roles = "admin, user")]
        [HttpGet,Route("index/{page}")]
        public IHttpActionResult Index(int page = 1)
        {
            var users = _authBusiness.GetUsers(page, pageSize);
            int count = _authBusiness.GetUsersCount();
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
        [HttpPost, Route("create")]
        public IHttpActionResult Create([FromBody]UserModel userModel)
        {
            _authBusiness.RegisterUser(userModel);

            return Ok("Üyelik bilgileriniz alınmıştır.");
        }
        [Authorize(Roles = "admin")]
        [HttpPost, Route("changePassword")]
        public IHttpActionResult ChangePassword([FromBody]UserModel userModel)
        {
            _authBusiness.ChangePassword(userModel);

            return Ok("Üyelik bilgileriniz alınmıştır.");
        }
        [Authorize(Roles = "admin, user")]
        [HttpGet, Route("getUserById/{id}")]
        public IHttpActionResult GetUserById(string id)
        {
            var userRole = _authBusiness.GetUserById(id);
            return Ok(userRole);
        }
        [Authorize(Roles = "admin, user")]
        [HttpGet, Route("getUserRoles/{id}")]
        public IHttpActionResult GetUserRoles(string id)
        {
            var userRole = _authBusiness.GetUserRoles(id);
            return Ok(userRole);
        }
        [Authorize(Roles = "admin, user")]
        [HttpGet, Route("getAllRoles")]
        public IHttpActionResult GetAllRoles()
        {
            var userRole = _authBusiness.GetAllRoles();
            return Ok(userRole);
        }
        [Authorize(Roles = "admin")]
        [HttpPost, Route("updateUserRoles")]
        public IHttpActionResult UpdateUserRoles([FromBody]UserRoleModel userRoleModel)
        {
            _authBusiness.UpdateUserRoleModel(userRoleModel);

            return Ok("Üyelik bilgileriniz alınmıştır.");
        }
        // POST api/Account/Register
        [AllowAnonymous]
        [Route("register")]
        public IHttpActionResult Register(UserModel userModel)
        {
            _authBusiness.RegisterUser(userModel);

            return Ok("Üyelik bilgileriniz alınmıştır.");
        }


       

    }

}
