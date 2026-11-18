using GotFired.DAL;
using GotFired.Model.ViewModels;

namespace GotFired.Business
{
    public class AuthBusiness
    {
        private readonly AuthRepository _authRepository;

        public AuthBusiness(AuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public Task<int> GetUsersCountAsync() => _authRepository.GetUsersCountAsync();

        public Task<IEnumerable<UserListItemModel>> GetUsersAsync(int page, int pageSize) => _authRepository.GetUsersAsync(page, pageSize);

        public Task<UserRoleModel> GetUserRolesAsync(string id) => _authRepository.GetUserRolesAsync(id);

        public Task<IList<string>> GetAllRolesAsync() => _authRepository.GetAllRolesAsync();

        public Task UpdateUserRoleModelAsync(UserRoleModel userRoleModel) => _authRepository.UpdateUserRoleModelAsync(userRoleModel);

        public Task<UserModel> GetUserByIdAsync(string id) => _authRepository.GetUserByIdAsync(id);

        public Task ChangePasswordAsync(UserModel userModel) => _authRepository.ChangePasswordAsync(userModel);

        public Task RegisterUserAsync(UserModel userModel) => _authRepository.RegisterAsync(userModel);

        /// <summary>Returns null when the credentials are invalid.</summary>
        public Task<UserTokenModel> FindUserAsync(string userName, string password) => _authRepository.FindUserAsync(userName, password);
    }
}
