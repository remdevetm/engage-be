using System.Threading.Tasks;
using UserAuthService.Models;

namespace UserAuthService.Repositories.Interfaces
{
    public interface ILoginActivityRepository
    {
        Task<bool> LogLoginActivity(string userId, LoginActivityType type);
        Task<bool> LogLogoutActivity(string userId);

    }
}
