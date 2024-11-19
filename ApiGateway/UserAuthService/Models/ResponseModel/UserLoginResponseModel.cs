namespace UserAuthService.Models.ResponseModel
{
      public class UserLoginResponseModel
  {
      public User Data { get; set; }
      public string Token { get; set; }
      public string Message { get; set; }
      public bool Error { get; set; }

      public UserLoginResponseModel() { }

      public UserLoginResponseModel(User data, string token, string message = "", bool error = false)
      {
          Data = data;
          Token = token;
          Message = message;
          Error = error;
      }
  }
}