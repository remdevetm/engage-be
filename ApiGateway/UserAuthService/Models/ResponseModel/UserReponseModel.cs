using UserAuthService.Models;

namespace UserAuthService.Models.ResponseModel
{
    public class UserResponseModel : IResponseModel<User>
    {
        public User? Data { get; set; }
        public string? Message { get; set; }
        public bool Error { get; set; }

        public UserResponseModel(User? data = null, string? message = "", bool error = false)
        {
            Data = data;
            Message = message;
            Error = error;
        }
    }
}
