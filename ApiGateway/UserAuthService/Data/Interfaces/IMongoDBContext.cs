using MongoDB.Driver;
using UserAuthService.Models.Model;
namespace UserAuthService.Data.Interfaces
{
    public interface IMongoDBContext
    {
        IMongoCollection<User> Users { get; }
        IMongoCollection<LoginActivity> LoginActivities { get; }
        IMongoDatabase Database { get; }
    }
}