using MongoDB.Driver;
using UserAuthService.Data.Interfaces;
using UserAuthService.Models;
using UserAuthService.Models.RequestModel;
using UserAuthService.Models.ResponseModel;
using UserAuthService.Repositories.Interfaces;

namespace UserAuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoDBContext _context;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<LoginActivity> _loginActivities;

        public UserRepository(IMongoDBContext context)
        {
            _users = context.Users;
            _loginActivities = context.LoginActivities;
        }

        public async Task<UserResponseModel> CreateAsync(User user)
        {
            try
            {
                // Check if user with same email already exists
                var existingUser = await _users.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    return new UserResponseModel(null, "User with this email already exists", true);
                }

                // Insert the new user
                await _users.InsertOneAsync(user);
                return new UserResponseModel(user, "User created successfully");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error creating user: {ex.Message}", true);
            }
        }

        public async Task<UserResponseModel> GetUserByEmail(string email)
        {
            try
            {
                var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
                if (user == null)
                {
                    return new UserResponseModel(null, "User not found", true);
                }
                return new UserResponseModel(user);
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error retrieving user: {ex.Message}", true);
            }
        }

        public async Task<UserResponseModel> GetByIdAsync(string userId)
        {
            try
            {
                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    return new UserResponseModel(null, "User not found", true);
                }
                return new UserResponseModel(user);
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error retrieving user by ID: {ex.Message}", true);
            }
        }

        public async Task<UserResponseModel> Login(string email, string passwordHash)
        {
            try
            {
                var user = await _users.Find(u =>
                    u.Email == email &&
                    u.PasswordHash == passwordHash &&
                    u.Status != UserStatus.Deleted
                ).FirstOrDefaultAsync();

                if (user == null)
                {
                    return new UserResponseModel(null, "Invalid credentials or user deleted", true);
                }

                // Update last login
                await UpdateLastLogin(user.Id);

                // Log login activity for agents
                if (user.UserType == UserType.Agent)
                {
                    await LogLoginActivity(user.Id, LoginActivityType.Login);
                }

                return new UserResponseModel(user, "Login successful");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error during login: {ex.Message}", true);
            }
        }

        public async Task<UserResponseModel> UpdateUserStatus(string userId, UserStatus status)
        {
            try
            {
                var update = Builders<User>.Update.Set(u => u.Status, status);
                var result = await _users.UpdateOneAsync(u => u.Id == userId, update);

                if (result.ModifiedCount == 0)
                {
                    return new UserResponseModel(null, "User status not updated", true);
                }

                return new UserResponseModel(null, "User status updated successfully");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error updating user status: {ex.Message}", true);
            }
        }

        public async Task<UserResponseModel> UpdateProfile(string userId, ProfileUpdateRequestModel request)
        {
            try
            {
                var update = Builders<User>.Update
                    .Set(u => u.Name, request.Name)
                    .Set(u => u.Surname, request.Surname)
                    .Set(u => u.WorkingHours, request.WorkingHours)
                    .Set(u => u.Position, request.Position);

                var result = await _users.UpdateOneAsync(u => u.Id == userId, update);

                if (result.ModifiedCount == 0)
                {
                    return new UserResponseModel(null, "User profile not updated", true);
                }

                var updatedUser = await GetByIdAsync(userId);
                return new UserResponseModel(updatedUser.Data, "User profile updated successfully");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error updating profile: {ex.Message}", true);
            }
        }

        public async Task<UserResponseModel> ChangePassword(string userId, string currentPasswordHash, string newPasswordHash)
        {
            try
            {
                var filter = Builders<User>.Filter.And(
                    Builders<User>.Filter.Eq(u => u.Id, userId),
                    Builders<User>.Filter.Eq(u => u.PasswordHash, currentPasswordHash),
                    Builders<User>.Filter.Ne(u => u.Status, UserStatus.Deleted)
                );

                var update = Builders<User>.Update
                    .Set(u => u.PasswordHash, newPasswordHash)
                    .Set(u => u.MustChangePassword, false); // Reset the flag after password change

                var result = await _users.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    return new UserResponseModel(null, "Password not updated. Current password may be incorrect.", true);
                }

                return new UserResponseModel(null, "Password updated successfully");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error updating password: {ex.Message}", true);
            }
        }

        public async Task<UserResponseModel> UpdatePassword(string userId, string newPasswordHash)
        {
            try
            {
                var update = Builders<User>.Update
                    .Set(u => u.PasswordHash, newPasswordHash)
                    .Set(u => u.MustChangePassword, false); // Reset the flag after password change

                var result = await _users.UpdateOneAsync(u => 
                    u.Id == userId && 
                    u.Status != UserStatus.Deleted, 
                    update);

                if (result.ModifiedCount == 0)
                {
                    return new UserResponseModel(null, "User not found or password not updated", true);
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
                var update = Builders<User>.Update.Set(u => u.LastLogin, DateTime.UtcNow);
                var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LogLoginActivity(string userId, LoginActivityType type)
        {
            try
            {
                var activity = new LoginActivity
                {
                    UserId = userId,
                    ActivityType = type,
                    DateTime = DateTime.UtcNow
                };
                await _loginActivities.InsertOneAsync(activity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserResponseModel> SetMustChangePassword(string userId, bool mustChangePassword)
        {
            try
            {
                var update = Builders<User>.Update.Set(u => u.MustChangePassword, mustChangePassword);
                var result = await _users.UpdateOneAsync(u => u.Id == userId, update);

                if (result.ModifiedCount == 0)
                {
                    return new UserResponseModel(null, "Failed to update password change requirement", true);
                }

                return new UserResponseModel(null, "Password change requirement updated successfully");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error updating password change requirement: {ex.Message}", true);
            }
        }

        public async Task<UserResponseModel> UpdateAsync(string userId, ProfileUpdateRequestModel updateModel)
        {
            try
            {
                var updateDefinition = Builders<User>.Update
                    .Set(u => u.Name, updateModel.Name)
                    .Set(u => u.Surname, updateModel.Surname)
                    .Set(u => u.WorkingHours, updateModel.WorkingHours)
                    .Set(u => u.Position, updateModel.Position);
                // Add additional fields as needed

                var result = await _users.UpdateOneAsync(u => u.Id == userId, updateDefinition);

                if (result.ModifiedCount == 0)
                {
                    return new UserResponseModel(null, "No changes were made to the profile.", true);
                }

                // Retrieve the updated user
                var updatedUser = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

                return new UserResponseModel(updatedUser, "Profile updated successfully.");
            }
            catch (Exception ex)
            {
                return new UserResponseModel(null, $"Error updating profile: {ex.Message}", true);
            }
        }
    }
}
