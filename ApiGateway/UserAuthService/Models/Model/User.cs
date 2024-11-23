using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using UserAuthService.Models.RequestModel;


namespace UserAuthService.Models.Model
{
    public class User
    {
        public User() { }
        public User(AgentRegisterRequestModel request)
        {
            Name = request.Name;
            Surname = request.Surname;
            Email = request.Email.ToLower();
            WorkingHours = request.WorkingHours;
            Position = request.Position;

            Status = UserStatus.New;
            UserType = UserType.Agent;
            LastLogin = DateTime.UtcNow;
            MustChangePassword = true;
        }

        public User(UserRequestModel request)
        {
            Name = request.Name;
            Surname = request.Surname;
            Email = request.Email.ToLower();
            WorkingHours = request.WorkingHours;
            Position = request.Position;
            PasswordHash = request.Password;

            Status = UserStatus.New;
            UserType = UserType.Agent;
            LastLogin = DateTime.UtcNow;
            MustChangePassword = true;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string WorkingHours { get; set; }
        public string Position { get; set; }
        public DateTime LastLogin { get; set; }
        public UserStatus Status { get; set; }
        public UserType UserType { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool MustChangePassword { get; set; }
        public string Otp { get; set; }
        public DateTime? OtpExpiry { get; set; }
        public int OtpAttempts { get; set; }
        public DateTime? OtpLockoutEnd { get; set; }
    }

    public enum UserStatus
    {
        New = 0,
        Verified = 1,
        Deleted = 2
    }

    public enum UserType
    {
        Agent,
        Admin
    }
}
