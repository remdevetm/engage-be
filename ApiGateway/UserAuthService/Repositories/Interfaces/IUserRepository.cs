using UserAuthService.Models;
using UserAuthService.Models.RequestModel;
using UserAuthService.Models.ResponseModel;

namespace UserAuthService.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserResponseModel> CreateUserAsync(User user);
        //Task<UserResponseModel> GetUserByEmail(string email);
        //Task<UserResponseModel> GetByIdAsync(string userId);
        Task<UserResponseModel> Login(LoginRequestModel request);
        Task<bool> UpdateLastLogin(string userId);
        Task<bool> LogLoginActivity(string userId);
        //Task<UserResponseModel> UpdateUserStatus(string userId, UserStatus status);
        //Task<UserResponseModel> UpdateProfile(string userId, ProfileUpdateRequestModel request);
        Task<UserResponseModel> ChangePassword(ChangePasswordRequestModel request);
        //Task<UserResponseModel> UpdatePassword(string userId, string newPasswordHash);
        //Task<bool> UpdateLastLogin(string userId);
        //Task<bool> LogLoginActivity(string userId, LoginActivityType type);
        Task<UserResponseModel> SetMustChangePassword(string userId, bool mustChangePassword);
        //Task<UserResponseModel> UpdateAsync(string userId, ProfileUpdateRequestModel updateModel);
    }
}
