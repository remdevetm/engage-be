using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using UserAuthService.Attributes;

namespace UserAuthService.Models
{
    public class LoginActivity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [Hex24String]
        public string UserId { get; set; }
        public LoginActivityType ActivityType { get; set; }
        public DateTime DateTime { get; set; }

        public LoginActivity(String userId)
        {
            UserId = userId;
        }

    }

    public enum LoginActivityType
    {
        Login = 0,
        Logout = 1,
    }
}