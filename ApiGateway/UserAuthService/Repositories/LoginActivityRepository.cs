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

        public async Task<bool> LogLoginActivity(string userId)
        {
            try
            {
                var activity = new LoginActivity(userId)
                {
                    //UserId = userId,
                    ActivityType = LoginActivityType.Login,
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
            try
            {
                var activity = new LoginActivity(userId)
                {
                    //UserId = userId,
                    ActivityType = LoginActivityType.Logout,
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
    }
}
