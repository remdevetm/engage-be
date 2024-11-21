using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Bcpg;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using UserAuthService.Models;
using UserAuthService.Models.RequestModel;
using UserAuthService.Models.ResponseModel;
using UserAuthService.Repositories.Interfaces;
using UserAuthService.Services.Interfaces;
using UserAuthService.Templates;

namespace ApiGateway.UserAuthService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IHashingService _hashingService;
        private readonly IEmailService _emailService;
        private readonly ILoginActivityRepository _loginActivityRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserRepository userRepository,
            IHashingService hashingService,
            IEmailService emailService,
            ILoginActivityRepository loginActivityRepository,
            IConfiguration configuration,
            ILogger<UserController> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _hashingService = hashingService ?? throw new ArgumentNullException(nameof(hashingService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _loginActivityRepository = loginActivityRepository ?? throw new ArgumentNullException(nameof(loginActivityRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }




        [HttpPost("CreateAgent")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserResponseModel>> CreateAgent([FromBody] UserRequestModel request)
        {
            try
            {
                // Validate the request model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Generate temporary password
                string tempPassword = GenerateTemporaryPassword();


                // Create new user with Agent role and Status set to New
                var user = new User(request)
                {
                    PasswordHash = tempPassword,
                    UserType = UserType.Agent,
                    Status = UserStatus.New,
                    MustChangePassword = true
                };

                // Save to database
                var result = await _userRepository.CreateUserAsync(user);
                if (result.Error)
                {
                    return BadRequest(new UserResponseModel(null,
                        $"Email already exists: {user.Email}",
                        true));
                }

                // Send email with temporary password
                //string emailBody = _emailService.GetEmailBody(EmailType.LoginDetail, emailContent);
                await _emailService.SendEmail(user.Email, "Welcome to the System - Login Credentials",
                    _emailService.GetEmailBody(EmailType.LoginDetail,
                    $"{user.Name},{user.Email},{tempPassword},{user.UserType.ToString()}"));

                return result.Data != null ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating agent: {ex.Message}");
                return BadRequest(new UserResponseModel(null, ex.Message, true));
            }
        }


        [HttpPost("CreateAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserResponseModel>> CreateAdmin([FromBody] UserRequestModel request)
        {
            try
            {
                // Validate the request model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // Create new user with Admin role and Status set to Verified
                var user = new User(request)
                {
                    PasswordHash = request.Password,
                    UserType = UserType.Admin,
                    Status = UserStatus.Verified,
                    MustChangePassword = false
                };

                // Save to database
                var result = await _userRepository.CreateUserAsync(user);
                if (result.Error)
                {
                    return BadRequest(new UserResponseModel
                    {
                        Data = null,
                        Message = $"Email already exists: {user.Email}",
                        Error = true
                    });
                }

                await _emailService.SendEmail(user.Email, "Welcome to the System - Login Credentials",
                    _emailService.GetEmailBody(EmailType.LoginDetail,
                    $"{user.Name},{user.Email},{user.PasswordHash},{user.UserType.ToString()}"));

                return result.Data != null ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating admin: {ex.Message}");
                return BadRequest(new UserResponseModel(null, ex.Message, true));
            }
        }

        [HttpPost("LoginAgent")]
        [Authorize(Roles = "Agent")]
        public async Task<ActionResult<UserResponseModel>> LoginAgent([FromBody] LoginRequestModel request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Attempt to login the user
                var loginResult = await _userRepository.Login(request);

                if (loginResult.Error || loginResult.Data == null)
                {
                    return BadRequest(loginResult);
                }

                var user = loginResult.Data;

                if (user.UserType != UserType.Agent)
                {
                    return BadRequest(new UserResponseModel(null, "Invalid user type.", true));
                }

                // Check if the user must change password
                if (user.Status == UserStatus.New || user.MustChangePassword)
                {
                    return BadRequest(new UserResponseModel(null, "You must change your password before proceeding.", true));
                }

                // Update last login and log activity
                await _userRepository.UpdateLastLogin(user.Id);
                await _userRepository.LogLoginActivity(user.Id);

                return loginResult.Data != null ? Ok(loginResult) : BadRequest(loginResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during Agent login: {ex.Message}");
                return BadRequest(new UserResponseModel(null, $"Error during login: {ex.Message}", true));
            }
        }


        [HttpPost("LoginAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserResponseModel>> LoginAdmin([FromBody] LoginRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Attempt to login the user
                var loginResult = await _userRepository.Login(request);

                if (loginResult.Error || loginResult.Data == null)
                {
                    return BadRequest(loginResult);
                }
                var user = loginResult.Data;

                if (user.UserType != UserType.Admin)
                {
                    return BadRequest(new UserResponseModel(null, "Invalid user type.", true));
                }

                // Check if the user must change password
                if (user.Status == UserStatus.New || user.MustChangePassword)
                {
                    return BadRequest(new UserResponseModel(null, "You must change your password before proceeding.", true));
                }

                return loginResult.Data != null ? Ok(loginResult) : BadRequest(loginResult);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during Admin login: {ex.Message}");
                return BadRequest(new UserResponseModel(null, $"Error during login: {ex.Message}", true));
            }
        }

        [HttpPost("AgentChangePassword")]
        [Authorize(Roles = "Agent")]
        public async Task<ActionResult<UserResponseModel>> ChangePassword([FromBody] ChangePasswordRequestModel request)
        { 
            // Prevent using the same password
            try
            {
                // Validate request model
                if (!ModelState.IsValid)
                {
                    return BadRequest(new UserResponseModel(null, "Invalid request data.", true));
                }

                // Change password
                var changePasswordResult = await _userRepository.ChangePassword(request);

                return changePasswordResult.Data != null ? Ok(changePasswordResult) : BadRequest(changePasswordResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error changing password: {ex.Message}");
                return BadRequest(new UserResponseModel(null, $"Error changing password: {ex.Message}", true));
            }
        }


        //[HttpPost("LogoutAgent")]
        //[Authorize(Roles = "Agent,Admin")]
        //public async Task<IActionResult> LogoutAgent()
        //{
        //    try
        //    {
        //        // Log logout activity
        //        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (!string.IsNullOrEmpty(userId))
        //        {
        //            bool logResult = await _loginActivityRepository.LogLogoutActivity(userId);
        //            if (!logResult)
        //            {
        //                _logger.LogError($"Failed to log logout activity for User {userId}.");
        //                return StatusCode(500, "Failed to log logout activity.");
        //            }
        //        }

        //        // Note: JWT tokens are stateless. To invalidate tokens, consider implementing a token blacklist.
        //        return Ok(new { Message = "Logout successful." });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error during Agent logout: {ex.Message}");
        //        return BadRequest(new { Message = $"Error during logout: {ex.Message}" });
        //    }
        //}


        //[HttpDelete("DeleteAgent/{agentId}")]
        //[Authorize(Roles = "Agent,Admin")]
        //public async Task<ActionResult<UserResponseModel>> DeleteAgent(string agentId)
        //{

        //    try
        //    {
        //        if (string.IsNullOrEmpty(agentId))
        //        {
        //            return BadRequest(new UserResponseModel(null, "Agent ID is required.", true));
        //        }

        //        // Retrieve the agent to ensure they exist and are an Agent
        //        var getUserResult = await _userRepository.GetByIdAsync(agentId);
        //        if (getUserResult.Error || getUserResult.Data == null)
        //        {
        //            return NotFound(new UserResponseModel(null, "Agent not found.", true));
        //        }

        //        if (getUserResult.Data.UserType != UserType.Agent)
        //        {
        //            return BadRequest(new UserResponseModel(null, "Specified user is not an Agent.", true));
        //        }

        //        // Update the user's status to Deleted
        //        var updateStatusResult = await _userRepository.UpdateUserStatus(agentId, UserStatus.Deleted);
        //        if (updateStatusResult.Error)
        //        {
        //            return BadRequest(updateStatusResult);
        //        }

        //        // Log admin's action
        //        string adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        _logger.LogInformation($"Admin {adminId} deleted Agent {agentId}.");

        //        return Ok(new UserResponseModel(null, "Agent deleted successfully."));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error deleting agent: {ex.Message}");
        //        return BadRequest(new UserResponseModel(null, $"Error deleting agent: {ex.Message}", true));
        //    }
        //}


        //[HttpDelete("DeleteAdmin/{adminId}")]
        //[Authorize(Roles = "Admin")]
        //public async Task<ActionResult<UserResponseModel>> DeleteAdmin(string adminId)
        //{
        //    if (string.IsNullOrEmpty(adminId))
        //    {
        //        return BadRequest(new UserResponseModel(null, "Admin ID is required.", true));
        //    }

        //    try
        //    {
        //        // Retrieve the admin to ensure they exist and are an Admin
        //        var getUserResult = await _userRepository.GetByIdAsync(adminId);
        //        if (getUserResult.Error || getUserResult.Data == null)
        //        {
        //            return NotFound(new UserResponseModel(null, "Admin not found.", true));
        //        }

        //        if (getUserResult.Data.UserType != UserType.Admin)
        //        {
        //            return BadRequest(new UserResponseModel(null, "Specified user is not an Admin.", true));
        //        }

        //        // Prevent self-deletion
        //        string currentAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (adminId == currentAdminId)
        //        {
        //            return BadRequest(new UserResponseModel(null, "Admins cannot delete their own account.", true));
        //        }

        //        // Update the user's status to Deleted
        //        var updateStatusResult = await _userRepository.UpdateUserStatus(adminId, UserStatus.Deleted);
        //        if (updateStatusResult.Error)
        //        {
        //            return BadRequest(updateStatusResult);
        //        }

        //        // Log admin's action
        //        _logger.LogInformation($"Admin {currentAdminId} deleted Admin {adminId}.");

        //        return Ok(new UserResponseModel(null, "Admin deleted successfully."));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error deleting admin: {ex.Message}");
        //        return BadRequest(new UserResponseModel(null, $"Error deleting admin: {ex.Message}", true));
        //    }
        //}



        //[HttpPut("UpdateProfile")]
        //[Authorize(Roles = "Agent,Admin")]
        //public async Task<ActionResult<UserResponseModel>> UpdateProfile([FromBody] ProfileUpdateRequestModel updateModel)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    try
        //    {
        //        // Retrieve the user ID from the JWT token
        //        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(userId))
        //        {
        //            return Unauthorized(new UserResponseModel(null, "Invalid user token.", true));
        //        }

        //        // Update the user's profile
        //        var updateResult = await _userRepository.UpdateAsync(userId, updateModel);
        //        if (updateResult.Error)
        //        {
        //            return BadRequest(updateResult);
        //        }

        //        return Ok(updateResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error updating profile: {ex.Message}");
        //        return StatusCode(500, new UserResponseModel(null, "An unexpected error occurred while updating the profile.", true));
        //    }
        //}

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}