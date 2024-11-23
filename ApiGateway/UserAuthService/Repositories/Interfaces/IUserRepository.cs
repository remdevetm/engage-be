using UserAuthService.Models.Model;
using UserAuthService.Models.RequestModel;
using UserAuthService.Models.ResponseModel;

namespace UserAuthService.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserResponseModel> CreateUserAsync(User user);
        Task<UserResponseModel> Login(LoginRequestModel request);
        Task<bool> UpdateLastLogin(string userId);
        Task<UserResponseModel> AgentChangePassword(ChangePasswordRequestModel request);
        Task<UserResponseModel> ResetPassword(ChangePasswordRequestModel request);
        Task<User> GetUserByEmail(string email);
        Task<UserResponseModel> UpdateUserOTP(User user);
        Task<UserResponseModel> UpdateUserPassword(string newPassword, User user);
        Task<User> GetUserById(string id);
        Task<UserResponseModel> UpdateUserStatus(User user);
        Task<UserResponseModel> UpdateUserProfile(User user);
    }
}
