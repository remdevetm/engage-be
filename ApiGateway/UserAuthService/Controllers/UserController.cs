using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UserAuthService.Models.Model;
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
        private readonly IEmailService _emailService;
        private readonly ILoginActivityRepository _loginActivityRepository;
        private readonly ILogger<UserController> _logger;
   

        public UserController(
            IUserRepository userRepository,
            IEmailService emailService,
            ILoginActivityRepository loginActivityRepository,
            ILogger<UserController> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _loginActivityRepository = loginActivityRepository ?? throw new ArgumentNullException(nameof(loginActivityRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("CreateAgent")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<UserResponseModel>> CreateAgent([FromBody] AgentRegisterRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new UserResponseModel(null, "Invalid request data.", true));
            }

            try
            {

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
                if (result.Error) return BadRequest(result);


                // Send welcome email
                try
                {
                    await _emailService.SendEmail(
                        user.Email,
                        "Welcome to the System - Login Credentials",
                        _emailService.GetEmailBody(
                            EmailType.LoginDetail,
                            $"{user.Name},{user.Email},{tempPassword},{user.UserType}"
                        )
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
                }

                //// Clear sensitive data
                //if ( result.Data != null)
                //{
                //    result.Data.PasswordHash = null;
                //}
                
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
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<UserResponseModel>> CreateAdmin([FromBody] UserRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new UserResponseModel(null, "Invalid request data.", true));
            }

            try
            {

                var user = new User(request)
                {
                    PasswordHash = request.Password,
                    UserType = UserType.Admin,
                    Status = UserStatus.Verified,
                    MustChangePassword = false
                };

                
                var result = await _userRepository.CreateUserAsync(user);
                if (result.Error) return BadRequest(result);


                //await _emailService.SendEmail(user.Email, "Welcome to the System - Login Credentials",
                //    _emailService.GetEmailBody(EmailType.LoginDetail,
                //    $"{user.Name},{user.Email},{user.PasswordHash},{user.UserType.ToString()}"));

                return result.Data != null ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating admin: {ex.Message}");
                return BadRequest(new UserResponseModel(null, ex.Message, true));
            }
        }

        [HttpPost("Login")]
        [Authorize(Roles = "Agent,Admin")]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<UserResponseModel>> LoginAgent([FromBody] LoginRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new UserResponseModel(null, "Invalid request data.", true));
            }

            try
            {

                var loginResult = await _userRepository.Login(request);

                if (loginResult.Error || loginResult.Data == null) return BadRequest(loginResult);

                var user = loginResult.Data;

                if (user.Status == UserStatus.New || user.MustChangePassword)
                {
                    return BadRequest(new UserResponseModel(null, "You must change your password before proceeding.", true));
                }

                await _userRepository.UpdateLastLogin(user.Id);

                if (user.UserType == UserType.Agent) await _loginActivityRepository.LogLoginActivity(user.Id);

                return loginResult.Data != null ? Ok(loginResult) : BadRequest(loginResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during Agent login: {ex.Message}");
                return BadRequest(new UserResponseModel(null, $"Error during login: {ex.Message}", true));
            }
        }


        [HttpPost("AgentChangePassword")]
        [Authorize(Roles = "Agent")]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<UserResponseModel>> ChangePassword([FromBody] ChangePasswordRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new UserResponseModel(null, "Invalid request data.", true));
            }

            try
            {
                var changePasswordResult = await _userRepository.AgentChangePassword(request);
                return changePasswordResult.Error ? BadRequest(changePasswordResult) : Ok(changePasswordResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error changing password: {ex.Message}");
                return BadRequest(new UserResponseModel(null, $"Error changing password: {ex.Message}", true));
            }
        }

        [HttpPut("ResetPassword")]
        [Authorize(Roles = "Agent,Admin")]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<UserResponseModel>> ResetPassword([FromBody] ChangePasswordRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new UserResponseModel(null, "Invalid request data.", true));
            }

            try
            {
                var changePasswordResult = await _userRepository.ResetPassword(request);
                return changePasswordResult.Error ? BadRequest(changePasswordResult) : Ok(changePasswordResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error changing password: {ex.Message}");
                return BadRequest(new UserResponseModel(null, $"Error changing password: {ex.Message}", true));
            }
        }

        [HttpPost("SendForgotPasswordEmailOtp")]
        [Authorize(Roles = "Agent,Admin")]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<UserResponseModel>> SendResetPasswordEmailOtp([FromBody] SendEmailOtpRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new UserResponseModel(null, "Invalid request data.", true));
            }

            try
            {
                var email = request.Email.ToLower();

                var user = await _userRepository.GetUserByEmail(email);

                if (user == null) return NotFound(new UserResponseModel(null, "user not found", true));

                var otp = GenerateOtp();
                user.Otp = otp;

                
                var emailBody = _emailService.GetEmailBody(EmailType.OTP, otp);
                await _emailService.SendEmail(email, $"Your OTP: {otp}", emailBody);

                var result = await _userRepository.UpdateUserOTP(user);

                if (result.Data != null) result.Data.Otp = string.Empty;
                return result.Data != null ? Ok(result) : BadRequest(result);
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
        public async Task<ActionResult<UserResponseModel>> ForgotPassword([FromBody] UserVerifyEmailOtpRequestModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new UserResponseModel(null, "Invalid request data.", true));
            }

            try
            {
                var email = userModel.Email.ToLower();
                var user = await _userRepository.GetUserByEmail(email);

                if (user == null)
                    return NotFound(new UserResponseModel(null, $"User with email {email} does not exist.", true));

                var (isValid, message) = await _userRepository.ValidateOtp(user, userModel.Otp);
                if (!isValid)
                    return BadRequest(new UserResponseModel(null, message, true));

                var updateResult = await _userRepository.UpdateUserPassword(userModel.Password, user);
                return updateResult.Data != null ? Ok(updateResult) : BadRequest(updateResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing ResetPasswordAsync for email: {Email}.", userModel?.Email);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPut("Delete/{userId}")]
        [Authorize(Roles = "Agent,Admin")]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<UserResponseModel>> DeleteAgent(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new UserResponseModel(null, "Null or Empty user id", true));
            }

            try
            {
                var user = await _userRepository.GetUserById(userId);
                if (user == null) return NotFound(new UserResponseModel(null, $"User of id {userId} not found", true));

                user.Status = UserStatus.Deleted;

                var updateResult = await _userRepository.UpdateUserStatus(user);

                if (updateResult.Data != null) updateResult.Message = "User deleted successully.";

                return updateResult.Data != null ? Ok(updateResult) : BadRequest(updateResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting agent with ID: {Id}", userId);
                return StatusCode((int)HttpStatusCode.InternalServerError, "An unexpected error occurred while deleting the agent.");
            }
        }

        [HttpPut("UpdateProfile")]
        [Authorize(Roles = "Agent,Admin")]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<UserResponseModel>> UpdateProfile([FromBody] ProfileUpdateRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new UserResponseModel(null, "Invalid request data.", true));
            }

            try
            {
                var user = await _userRepository.GetUserById(request.UserId);
                if (user == null) return NotFound(new UserResponseModel(null, $"User of id {request.UserId} not found", true));

                user.Name = request.Name;
                user.Surname = request.Surname;
                user.WorkingHours = request.WorkingHours;
                user.Position = request.Position;

                var updateResult = await _userRepository.UpdateUserProfile(user);

                return updateResult.Data != null ? Ok(updateResult) : BadRequest(updateResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating profile for user with ID: {Id}", request.UserId);
                return StatusCode((int)HttpStatusCode.InternalServerError, "An unexpected error occurred while updating the profile.");
            }
        }

        [HttpPost("Logout/{userId}")]
        [Authorize(Roles = "Agent,Admin")]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<UserResponseModel>> LogoutAgent(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new UserResponseModel(null, "Null or Empty user id",true));
            }

            try
            {
                var user = await _userRepository.GetUserById(userId);
                if (user == null) return NotFound(new UserResponseModel(null, $"User of id {userId} not found", true));

                if (user.UserType == UserType.Agent) await _loginActivityRepository.LogLogoutActivity(userId);

                var result = new UserResponseModel(user, "log out successful");
                return result.Data != null ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during agent logout for user ID: {UserId}", userId);
                return StatusCode((int)HttpStatusCode.InternalServerError, "An unexpected error occurred during logout");
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


    }
}