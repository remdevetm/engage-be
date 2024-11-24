using MongoDB.Driver;
using UserAuthService.Data.Interfaces;
using UserAuthService.Models.Model;
using UserAuthService.Models.RequestModel;
using UserAuthService.Models.ResponseModel;
using UserAuthService.Repositories.Interfaces;
using UserAuthService.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace UserAuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoDBContext _context;
        private readonly IMongoCollection<User> _users;
        private readonly IHashingService _hashingService;
        private readonly OtpSettings _otpSettings;

        public UserRepository(
            IMongoDBContext context, 
            IHashingService hashingService, 
            IOptions<OtpSettings> otpSettings)
        {
            _hashingService = hashingService ?? throw new ArgumentNullException(nameof(hashingService));
            _users = context.Users;
            _otpSettings = otpSettings.Value ?? throw new ArgumentNullException(nameof(otpSettings));
        }

        public async Task<User> GetUserByEmail(string email)
        {
            try
            {

                var filterBuilder = Builders<User>.Filter;
                var filter = filterBuilder.And(
                    filterBuilder.Eq(u => u.Email, email.ToLower()),
                    filterBuilder.Ne(u => u.Status, UserStatus.Deleted)
                );

                return await _users.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<UserResponseModel> UpdateUserOTP(User user)
        {
            try
            {
                user.OtpExpiry = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiryMinutes);
                user.OtpAttempts = 0;
                user.OtpLockoutEnd = null;

                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                var update = Builders<User>.Update
                    .Set(u => u.Otp, user.Otp)
                    .Set(u => u.OtpExpiry, user.OtpExpiry)
                    .Set(u => u.OtpAttempts, user.OtpAttempts)
                    .Set(u => u.OtpLockoutEnd, user.OtpLockoutEnd);

                var result = await _users.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    return new UserResponseModel(null, "User OTP not updated.", true);
                }

                return new UserResponseModel(user, "User OTP updated successfully");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, ex.Message, true);
            }
        }

        public async Task<UserResponseModel> UpdateUserPassword(string newPassword, User user)
        {
            try
            {
                string salt;
                user.PasswordHash = _hashingService.Hash(newPassword, out salt);

                await _users.ReplaceOneAsync(x => x.Id == user.Id, user);

                return new UserResponseModel(user, "Password updated succefully");
            }
            catch (Exception e)
            {
                return new UserResponseModel(null, e.Message, true);
            }
        }



        public async Task<UserResponseModel> CreateUserAsync(User user)
        {
            try
            {
                
                var filter = Builders<User>.Filter.Eq(u => u.Email, user.Email);
                var existingUser = await _users.Find(filter).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    return new UserResponseModel(null, "User with this email already exists", true);
                }

                string salt;
                user.PasswordHash = _hashingService.Hash(user.PasswordHash, out salt);

                await _users.InsertOneAsync(user);
                return new UserResponseModel(user, "User created successfully");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error creating user: {ex.Message}", true);
            }
        }


        public async Task<UserResponseModel> Login(LoginRequestModel request)
        {
            try
            {

                var user = await GetUserByEmail(request.Email);
                if (user == null)
                {
                    return new UserResponseModel(null, $"User with email {request.Email} not found.", true);
                }
                if (!_hashingService.Verify(request.Password, user.PasswordHash))
                {
                    return new UserResponseModel(null, "Enter correct correctCredentials", true);
                }
                return new UserResponseModel(user, "Login successful");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error during login: {ex.Message}", true);
            }
        }



        public async Task<UserResponseModel> ResetPassword(ChangePasswordRequestModel request)
        {
            try
            {
                var user = await _users.Find(
                    Builders<User>.Filter.And(
                        Builders<User>.Filter.Eq(u => u.Id, request.UserId),
                        Builders<User>.Filter.Eq(u => u.Status, UserStatus.Verified)
                    )).FirstOrDefaultAsync();

                if (user == null) return new UserResponseModel(null, "User with not found.", true);

                if (!_hashingService.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return new UserResponseModel(null, "Invalid current password.", true);
                }

                string salt;
                string newPasswordHash = _hashingService.Hash(request.NewPassword, out salt);
                
                if (_hashingService.Verify(newPasswordHash, user.PasswordHash))
                {
                    return new UserResponseModel(null, "New password same as old password.", true);
                }

                var filter = Builders<User>.Filter.Eq(u => u.Id, request.UserId);
                var update = Builders<User>.Update
                    .Set(u => u.PasswordHash, newPasswordHash)
                    .Set(u => u.MustChangePassword, false);

                var result = await _users.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    return new UserResponseModel(null, "Password not updated.", true);
                }

                return new UserResponseModel(null, "Password updated successfully");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error resetting password: {ex.Message}", true);
            }
        }

        public async Task<UserResponseModel> AgentChangePassword(ChangePasswordRequestModel request)
        {
            try
            {

                var user = await _users.Find(
                    Builders<User>.Filter.And(
                        Builders<User>.Filter.Eq(u => u.Id, request.UserId),
                        Builders<User>.Filter.Eq(u => u.Status, UserStatus.New),
                        Builders<User>.Filter.Ne(u => u.Status, UserStatus.Deleted),
                        Builders<User>.Filter.Eq(u => u.UserType, UserType.Agent)
                    )).FirstOrDefaultAsync();

                if (user == null)
                {
                    return new UserResponseModel(null, "User not found.", true);
                }

                if (!_hashingService.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return new UserResponseModel(null, "Invalid current password.", true);
                }

                string salt;
                string newPasswordHash = _hashingService.Hash(request.NewPassword, out salt);


                var filter = Builders<User>.Filter.Eq(u => u.Id, request.UserId);
                var update = Builders<User>.Update
                    .Set(u => u.PasswordHash, newPasswordHash)
                    .Set(u => u.Status, UserStatus.Verified)
                    .Set(u => u.MustChangePassword, false);


                var result = await _users.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    return new UserResponseModel(null, "There was a problem updating password.", true);
                }

                return new UserResponseModel(null, "Password updated successfully");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error updating password: {ex.Message}", true);
            }
        }

        public async Task<bool> UpdateLastLogin(string userId)
        {
            try
            {
                var filterBuilder = Builders<User>.Filter;
                var filter = filterBuilder.And(
                    filterBuilder.Eq(u => u.Id, userId),
                    filterBuilder.Ne(u => u.Status, UserStatus.Deleted)
                );
                var update = Builders<User>.Update.Set(u => u.LastLogin, DateTime.UtcNow);
                var result = await _users.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<User> GetUserById(string userId)
        {
            try
            {
                // var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
                // return await _users.Find(filter).FirstOrDefaultAsync();
                var filterBuilder = Builders<User>.Filter;
                var filter = filterBuilder.And(
                    filterBuilder.Eq(u => u.Id, userId),
                    filterBuilder.Ne(u => u.Status, UserStatus.Deleted)
                );
                return await _users.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<UserResponseModel> UpdateUserStatus(User user)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                var update = Builders<User>.Update
                    .Set(u => u.Status, user.Status);

                var result = await _users.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    return new UserResponseModel(null, "User status not updated.", true);
                }

                return new UserResponseModel(user, "User status updated successfully");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error updating user status: {ex.Message}", true);
            }
        }

        public async Task<UserResponseModel> UpdateUserProfile(User user)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                var update = Builders<User>.Update
                    .Set(u => u.Name, user.Name)
                    .Set(u => u.Surname, user.Surname)
                    .Set(u => u.WorkingHours, user.WorkingHours)
                    .Set(u => u.Position, user.Position);

                var result = await _users.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    return new UserResponseModel(null, "User profile not updated.", true);
                }

                return new UserResponseModel(user, "User profile updated successfully");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error updating user profile: {ex.Message}", true);
            }
        }

        public async Task<(bool isValid, string message)> ValidateOtp(User user, string otp)
        {
            try 
            {
                // Check if OTP exists
                if (string.IsNullOrEmpty(user.Otp))
                {
                    return (false, "No OTP found. Please request a new one.");
                }

                // Check if OTP has expired
                if (!user.OtpExpiry.HasValue || user.OtpExpiry < DateTime.UtcNow)
                {
                    return (false, "OTP has expired. Please request a new one.");
                }

                // Check if user is locked out
                if (user.OtpLockoutEnd.HasValue && user.OtpLockoutEnd > DateTime.UtcNow)
                {
                    var remainingMinutes = Math.Ceiling((user.OtpLockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                    return (false, $"Too many attempts. Please try again in {remainingMinutes} minutes.");
                }

                // Validate OTP
                if (user.Otp != otp)
                {
                    user.OtpAttempts++;
                    
                    // Check if max attempts reached
                    if (user.OtpAttempts >= _otpSettings.MaxAttempts)
                    {
                        user.OtpLockoutEnd = DateTime.UtcNow.AddMinutes(_otpSettings.LockoutMinutes);
                        await UpdateUserOTP(user);
                        return (false, $"Maximum attempts reached. Account locked for {_otpSettings.LockoutMinutes} minutes.");
                    }

                    await UpdateUserOTP(user);
                    return (false, $"Invalid OTP. {_otpSettings.MaxAttempts - user.OtpAttempts} attempts remaining.");
                }

                // Reset attempts on successful validation
                user.OtpAttempts = 0;
                user.OtpLockoutEnd = null;
                await UpdateUserOTP(user);

                //// Reset attempts and clear OTP after successful validation
                //user.OtpAttempts = 0;
                //user.OtpLockoutEnd = null;
                //user.Otp = null; // Invalidate OTP after successful use
                //user.OtpExpiry = null;
                //await UpdateUserOTP(user);

                return (true, "OTP validated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error validating OTP: {ex.Message}");
            }
        }
    }
}
