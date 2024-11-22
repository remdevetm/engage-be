using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Bcpg;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
            try
            {
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

        [HttpPut("ResetPassword")]
        [Authorize(Roles = "Agent,Admin")]
        public async Task<ActionResult<UserResponseModel>> ResetPassword([FromBody] ChangePasswordRequestModel request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new UserResponseModel(null, "Invalid request data.", true));
                }

                // Change password
                var changePasswordResult = await _userRepository.ResetPassword(request);

                return changePasswordResult.Data != null ? Ok(changePasswordResult) : BadRequest(changePasswordResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error changing password: {ex.Message}");
                return BadRequest(new UserResponseModel(null, $"Error changing password: {ex.Message}", true));
            }
        }

        [HttpPost("SendForgotPasswordEmailOtp")]
        [Authorize(Roles = "Agent,Admin")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendResetPasswordEmailOtp([FromBody] SendEmailOtpRequestModel request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("Invalid request received for SendResetPasswordEmailOtp.");
                return BadRequest("Invalid request data.");
            }

            try
            {
                var email = request.Email.ToLower();

                
                var user = await _userRepository.GetUserByEmail(email);
                if (user == null)
                {
                    _logger.LogError("User with email: {Email} does not exist", email);
                    return NotFound("User does not exist.");
                }

                
                var otp = GenerateOtp();
                user.Otp = otp;

                
                var emailBody = _emailService.GetEmailBody(EmailType.OTP, otp);
                await _emailService.SendEmail(email, $"Your OTP: {otp}", emailBody);

                
                await _userRepository.UpdateUserOTP(user);

                
                var response = new UserResponseModel(user);
                if (response.Data == null)
                { 
                    return BadRequest();
                }
                response.Data.Otp = string.Empty;
                _logger.LogInformation("OTP sent successfully to user with email: {Email}.", email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the SendResetPasswordEmailOtp request for email: {Email}.", request?.Email);
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing the request.");
            }
        }


        [HttpPut("ForgotPassword")]
        [Authorize(Roles = "Agent,Admin")]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] UserVerifyEmailOtpRequestModel userModel)
        {
            if (userModel == null || 
                string.IsNullOrWhiteSpace(userModel.Email) || 
                string.IsNullOrWhiteSpace(userModel.Otp))
            {
                _logger.LogWarning("Invalid request received for ResetPasswordAsync.");
                return BadRequest("Invalid request data.");
            }

            try
            {

                var email = userModel.Email.ToLower();

                var user = await _userRepository.GetUserByEmail(email);
                if (user == null)
                {
                    _logger.LogError("User with email: {Email} not found.", email);
                    return NotFound($"User with email {email} does not exist.");
                }

                if (user.Otp != userModel.Otp)
                {
                    _logger.LogWarning("Invalid OTP provided for email: {Email}.", email);
                    return BadRequest("Invalid OTP.");
                }

                var updateResult = await _userRepository.UpdateUserPassword(user);
                if (updateResult.Error)
                {
                    _logger.LogError("Failed to update password for email: {Email}. Error: {Error}", email, updateResult.Message);
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to update password.");
                }

                _logger.LogInformation("Password reset successfully for email: {Email}.", email);
                return Ok(updateResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing ResetPasswordAsync for email: {Email}.", userModel?.Email);
                return StatusCode((int)HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }


        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GenerateOtp()
        {
            return new Random().Next(10000, 100000).ToString();
        }

        [HttpPut("DeleteAgent/{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteAgent(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("Invalid agent ID.");
            }

            try
            {
                var user = await _userRepository.GetUserById(userId);
                if (user == null)
                {
                    return NotFound("Agent not found.");
                }

                if (user.UserType != UserType.Agent)
                {
                    return BadRequest("Only agents can be deleted through this endpoint.");
                }

                user.Status = UserStatus.Deleted;
                
                var updateResult = await _userRepository.UpdateUserStatus(user);
                if (updateResult.Error)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to delete agent.");
                }
                return Ok(updateResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting agent with ID: {Id}", userId);
                return StatusCode((int)HttpStatusCode.InternalServerError, "An unexpected error occurred while deleting the agent.");
            }
        }
    }
}