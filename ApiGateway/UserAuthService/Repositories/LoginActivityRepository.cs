using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using UserAuthService.Data.Interfaces;
using UserAuthService.Models;
using UserAuthService.Repositories.Interfaces;

namespace UserAuthService.Repositories
{
    public class LoginActivityRepository : ILoginActivityRepository
    {
        private readonly IMongoCollection<LoginActivity> _loginActivities;

        public LoginActivityRepository(IMongoDBContext context)
        {
            _loginActivities = context.LoginActivities;
        }

        public async Task<bool> LogLoginActivity(string userId, LoginActivityType type)
        {
            try
            {
                var activity = new LoginActivity(userId)
                {
                    //UserId = userId,
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

        public async Task<bool> LogLogoutActivity(string userId)
        {
            return await LogLoginActivity(userId, LoginActivityType.Logout);
        }
    }
}
