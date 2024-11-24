using System;
using System.Threading.Tasks;
using UserAuthService.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace UserAuthService.Repositories.Interfaces
{
    public interface ILoginActivityRepository
    {
        Task<bool> IsUserLoggedIn(string userId);
        Task<bool> LogLoginActivity(string userId);
        Task<bool> LogLogoutActivity(string userId);
    }
}
