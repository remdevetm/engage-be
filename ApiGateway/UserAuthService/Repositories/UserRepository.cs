using MongoDB.Driver;
using UserAuthService.Data.Interfaces;
using UserAuthService.Models;
using UserAuthService.Models.RequestModel;
using UserAuthService.Models.ResponseModel;
using UserAuthService.Repositories.Interfaces;
using UserAuthService.Services.Interfaces;

namespace UserAuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoDBContext _context;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<LoginActivity> _loginActivities;
        private readonly IHashingService _hashingService;

        public UserRepository(IMongoDBContext context, IHashingService hashingService)
        {
            _hashingService = hashingService;
            _users = context.Users;
            _loginActivities = context.LoginActivities;
        }

        public async Task<User> GetUserByEmail(string email)
        {

            try
            {
                var filterBuilder = Builders<User>.Filter;
                var filter = Builders<User>.Filter.And(
                    filterBuilder.Eq(u => u.Email, email),
                    filterBuilder.Ne(u => u.Status, UserStatus.Deleted)
                );

                var user = await _context.Users.Find(filter).FirstOrDefaultAsync();

                return user;
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
                await _context.Users.ReplaceOneAsync(x => x.Id == user.Id, user);
                return new UserResponseModel(user);
            }
            catch (Exception e)
            {
                return new UserResponseModel(null, e.Message, true);
            }
        }
        public async Task<UserResponseModel> UpdateUserPassword(User user)
        {
            try
            {
                string salt;
                user.PasswordHash = _hashingService.Hash(user.PasswordHash, out salt);
                await _context.Users.ReplaceOneAsync(x => x.Id == user.Id, user);
                return new UserResponseModel(user);
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
                // Check if user with email already exists
                var filter = Builders<User>.Filter.Eq(u => u.Email, user.Email);
                var existingUser = await _users.Find(filter).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    return new UserResponseModel(null, "User with this email already exists", true);
                }

                // Hash password with checksum-based salt embedding
                string salt;
                user.PasswordHash = _hashingService.Hash(user.PasswordHash, out salt);

                // Insert the new user
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
                //var filter = Builders<User>.Filter.And(
                //    Builders<User>.Filter.Eq(u => u.Email, request.Email),
                //    Builders<User>.Filter.Ne(u => u.Status, UserStatus.Deleted));

                //var user = await _users.Find(filter).FirstOrDefaultAsync();
                var user = await GetUserByEmail(request.Email);

                if (user == null || !_hashingService.Verify(request.Password, user.PasswordHash))
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

        //public async Task<UserResponseModel> ForgotPassword(ChangePasswordRequestModel request)
        //{
        //    try
        //    {
        //        return new UserResponseModel(null, "Verify properly", true);
        //    }
        //    catch (Exception ex)
        //    {

        //        return new UserResponseModel(null, $"Error resetting password: {ex.Message}", true);
        //    }
        //}


        public async Task<UserResponseModel> ResetPassword(ChangePasswordRequestModel request)
        {
            try
            {
                var user = await _users.Find(
                    Builders<User>.Filter.And(
                        Builders<User>.Filter.Eq(u => u.Id, request.UserId),
                        Builders<User>.Filter.Eq(u => u.Status, UserStatus.Verified)
                    )).FirstOrDefaultAsync();

                // Verify current password
                if (user == null || !_hashingService.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return new UserResponseModel(null, "Invalid current password.", true);
                }

                // Hash new password
                string salt;
                string newPasswordHash = _hashingService.Hash(request.NewPassword, out salt);
                
                if (_hashingService.Verify(newPasswordHash, user.PasswordHash))
                {
                    return new UserResponseModel(null, "Cannot use same password.", true);
                }
                // Update password
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

        public async Task<UserResponseModel> ChangePassword(ChangePasswordRequestModel request)
        {
            try
            {
                // Find the user first
                var user = await _users.Find(
                    Builders<User>.Filter.And(
                        Builders<User>.Filter.Eq(u => u.Id, request.UserId),
                        Builders<User>.Filter.Eq(u => u.Status, UserStatus.New),
                        Builders<User>.Filter.Ne(u => u.Status, UserStatus.Deleted)
                    )).FirstOrDefaultAsync();

                // Verify current password
                if (user == null || !_hashingService.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return new UserResponseModel(null, "Invalid current password.", true);
                }

                if (user.UserType != UserType.Agent)
                {
                    return new UserResponseModel(null, "Invalid user type.", true);
                }

                // Hash new password
                string salt;
                string newPasswordHash = _hashingService.Hash(request.NewPassword, out salt);

                // Update password
                var filter = Builders<User>.Filter.Eq(u => u.Id, request.UserId);
                var update = Builders<User>.Update
                    .Set(u => u.PasswordHash, newPasswordHash)
                    .Set(u => u.Status, UserStatus.Verified)
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
                return new UserResponseModel(null, $"Error updating password: {ex.Message}", true);
            }
        }

        public async Task<bool> UpdateLastLogin(string userId)
        {
            try
            {
                // Build the filter for the user ID
                var filter = Builders<User>.Filter.Eq(u => u.Id, userId);

                // Build the update to set the last login time
                var update = Builders<User>.Update.Set(u => u.LastLogin, DateTime.UtcNow);

                // Perform the update
                var result = await _users.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> LogLoginActivity(string userId)
        {
            try
            {
                // Create a new login activity
                var activity = new LoginActivity(userId)
                {
                    ActivityType = LoginActivityType.Login,
                    DateTime = DateTime.UtcNow
                };

                // Insert the activity document
                await _loginActivities.InsertOneAsync(activity);
                return true;
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
                var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
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
    }
}
