using UserAuthService.Models;
using UserAuthService.Models.RequestModel;
using UserAuthService.Models.ResponseModel;

namespace UserAuthService.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserResponseModel> CreateUserAsync(User user);
        Task<UserResponseModel> Login(LoginRequestModel request);
        Task<bool> UpdateLastLogin(string userId);
        Task<bool> LogLoginActivity(string userId);
        Task<UserResponseModel> ChangePassword(ChangePasswordRequestModel request);
        Task<UserResponseModel> ResetPassword(ChangePasswordRequestModel request);
        Task<UserResponseModel> ForgotPassword(ChangePasswordRequestModel request);
        Task<User> GetUserByEmail(string email);
        Task<UserResponseModel> UpdateUserOTP(User user);
        Task<UserResponseModel> UpdateUserPassword(User user);
        Task<User> GetUserById(string id);
        Task<UserResponseModel> UpdateUserStatus(User user);
    }
}
