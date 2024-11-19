using MongoDB.Driver;
using UserAuthService.Data.Interfaces;
using UserAuthService.Models;
using Microsoft.Extensions.Configuration;

namespace UserAuthService.Data
{
    public class MongoDbContext : IMongoDBContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoClient client, IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var settings = configuration.GetSection("MongoDb");

            var connectionString = settings.GetValue<string>("ConnectionString");
            var databaseName = settings.GetValue<string>("DatabaseName");
            var usersCollection = settings.GetValue<string>("UsersCollection");
            var loginActivityCollection = settings.GetValue<string>("LoginActivityCollection");

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("MongoDB ConnectionString is missing or invalid");

            _database = client.GetDatabase(databaseName);

            Users = _database.GetCollection<User>(usersCollection);
            LoginActivities = _database.GetCollection<LoginActivity>(loginActivityCollection);
        }

        public IMongoCollection<User> Users { get; }
        public IMongoCollection<LoginActivity> LoginActivities { get; }
        public IMongoDatabase Database => _database;
    }
}