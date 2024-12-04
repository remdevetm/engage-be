using static System.Net.WebRequestMethods;

namespace UserAuthService
{
    public static class AuthServerConfig
    {
        public const string URL = "https://thooto-dev-be-api-authserver.azurewebsites.net";
        //public const string URL = "https://localhost:7149"; 
        public const bool HTTPS_REQUIRED = false;
        public const string API_NAME = "UserAuthServiceAPI";
    }
}
