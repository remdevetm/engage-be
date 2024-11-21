namespace UserAuthService
{
    public class LogClaimsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogClaimsMiddleware> _logger;

        public LogClaimsMiddleware(RequestDelegate next, ILogger<LogClaimsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var user = context.User;
            if (user.Identity.IsAuthenticated)
            {
                // Log claims here
                var roles = user.FindAll("client_role").Select(c => c.Value).ToList();
                _logger.LogInformation("User Roles: {Roles}", string.Join(",", roles));

                // Alternatively, log all claims
                foreach (var claim in user.Claims)
                {
                    Console.WriteLine(string.Join(",", roles));
                    _logger.LogInformation("Claim Type: {ClaimType}, Claim Value: {ClaimValue}", claim.Type, claim.Value);
                }
            }

            await _next(context);
        }
    }
}

