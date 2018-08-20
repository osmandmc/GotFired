using GotFired.DAL;
using GotFired.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GotFired.Business
{
    public class AuthBusiness : IDisposable
    {
        //private AuthContext _ctx;

        //private UserManager<IdentityUser> _userManager;

        //public AuthRepository()
        //{
        //    _ctx = new AuthContext();
        //    //_userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(_ctx));
        //}
        readonly UnitOfWork _unitOfWork;

        public AuthBusiness()
        {
            _unitOfWork = new UnitOfWork();
        }
        public int GetUsersCount()
        {
            return _unitOfWork.AuthRepository.GetUsersCount();
        }
        public IEnumerable<UserListItemModel> GetUsers(int page, int pageSize)
        {
            return _unitOfWork.AuthRepository.GetUsers(page, pageSize);
        }
        public UserRoleModel GetUserRoles(string id)
        {
            return _unitOfWork.AuthRepository.GetUserRoles(id);
        }
        public IList<string> GetAllRoles()
        {
            return _unitOfWork.AuthRepository.GetAllRoles();
        }
        public void UpdateUserRoleModel(UserRoleModel userRoleModel)
        {
            _unitOfWork.AuthRepository.UpdateUserRoleModel(userRoleModel);
        }

        public UserModel GetUserById(string id)
        {
            return _unitOfWork.AuthRepository.GetUserById(id);
        }


        public void ChangePassword(UserModel userModel)
        {
            _unitOfWork.AuthRepository.ChangePassword(userModel);
            
        }

        public void RegisterUser(UserModel userModel)
        {
            _unitOfWork.AuthRepository.Register(userModel);
        }

        public UserTokenModel FindUser(string userName, string password)
        {
            var user = _unitOfWork.AuthRepository.FindUser(userName, password);
            //if(user!=null)
            //{

            //}
            return _unitOfWork.AuthRepository.FindUser(userName,password);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Debug.WriteLine("UnitOfWork is being disposed");
                    _unitOfWork.Dispose();
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AuthBusiness() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        //public async Task<IdentityUser> FindUser(string userName, string password)
        //{
        //    IdentityUser user = await _userManager.FindAsync(userName, password);

        //    return user;
        //}

        //public void Dispose()
        //{
        //    _ctx.Dispose();
        //    _userManager.Dispose();

        //}
    }
}
