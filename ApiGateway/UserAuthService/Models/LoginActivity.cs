using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace UserAuthService.Models
{
    public class LoginActivity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
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