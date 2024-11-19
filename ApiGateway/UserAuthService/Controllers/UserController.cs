using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
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
        [Authorize(Policy = "AdminPolicy")]
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

                // Hash the temporary password
                string hashedTempPassword = _hashingService.Hash(tempPassword);

                // Create new user with Agent role and Status set to New
                var user = new User(request)
                {
                    PasswordHash = hashedTempPassword,
                    UserType = UserType.Agent,
                    Status = UserStatus.New,
                    MustChangePassword = true
                };

                // Save to database
                var createResult = await _userRepository.CreateAsync(user);
                if (createResult.Error)
                {
                    return BadRequest(createResult);
                }

                // Prepare email content
                string emailContent = $"Dear {user.Name},\n\n" +
                                      $"Your agent account has been created. Please use the following temporary password to log in and change your password immediately:\n\n" +
                                      $"{tempPassword}\n\n" +
                                      $"Best Regards,\nAdmin Team";

                // Send email with temporary password
                string emailBody = _emailService.GetEmailBody(EmailType.LoginDetail, emailContent);
                await _emailService.SendEmail(user.Email, "Welcome to the System - Login Credentials", emailBody);

                // Optional: Log admin's action
                string adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"Admin {adminId} created Agent {user.Id}.");

                return Ok(new UserResponseModel(user, "Agent created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating agent: {ex.Message}");
                return BadRequest(new UserResponseModel(null, ex.Message, true));
            }
        }


        [HttpPost("CreateAdmin")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<ActionResult<UserResponseModel>> CreateAdmin([FromBody] UserRequestModel request)
        {
            try
            {
                // Validate the request model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Hash the provided password
                string hashedPassword = _hashingService.Hash(request.PasswordHash);

                // Create new user with Admin role and Status set to Verified
                var user = new User(request)
                {
                    PasswordHash = hashedPassword,
                    UserType = UserType.Admin,
                    Status = UserStatus.Verified,
                    MustChangePassword = false
                };

                // Save to database
                var createResult = await _userRepository.CreateAsync(user);
                if (createResult.Error)
                {
                    return BadRequest(createResult);
                }

                // Prepare email content
                string emailContent = $"Dear {user.Name},\n\n" +
                                      $"Your admin account has been created. Please use your credentials to log in.\n\n" +
                                      $"Best Regards,\nAdmin Team";

                // Send email with login credentials
                string emailBody = _emailService.GetEmailBody(EmailType.LoginDetail, emailContent);
                await _emailService.SendEmail(user.Email, "Welcome to the System - Admin Login Credentials", emailBody);

                // Optional: Log admin's action
                string adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"Admin {adminId} created Admin {user.Id}.");

                return Ok(new UserResponseModel(user, "Admin created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating admin: {ex.Message}");
                return BadRequest(new UserResponseModel(null, ex.Message, true));
            }
        }

        [HttpPost("LoginAgent")]
        [AllowAnonymous]
        public async Task<ActionResult<UserLoginResponseModel>> LoginAgent([FromBody] LoginRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Hash the provided password
                var hashedPassword = _hashingService.Hash(request.Password);

                // Attempt to login the user
                var loginResult = await _userRepository.Login(request.Email, hashedPassword);

                if (loginResult.Error || loginResult.Data == null)
                {
                    return BadRequest(loginResult);
                }

                if (loginResult.Data.UserType != UserType.Agent)
                {
                    return BadRequest(new UserResponseModel(null, "Invalid user type.", true));
                }

                // Check if the user must change password
                if (loginResult.Data.MustChangePassword)
                {
                    return BadRequest(new UserResponseModel(null, "You must change your password before proceeding.", true));
                }

                // Generate JWT Token
                var token = GenerateJwtToken(loginResult.Data);

                return Ok(new UserLoginResponseModel
                {
                    Data = loginResult.Data,
                    Token = token,
                    Message = "Login successful",
                    Error = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during Agent login: {ex.Message}");
                return BadRequest(new UserResponseModel(null, $"Error during login: {ex.Message}", true));
            }
        }


        [HttpPost("LoginAdmin")]
        [AllowAnonymous]
        public async Task<ActionResult<UserLoginResponseModel>> LoginAdmin([FromBody] LoginRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Hash the provided password
                var hashedPassword = _hashingService.Hash(request.Password);

                // Attempt to login the user
                var loginResult = await _userRepository.Login(request.Email, hashedPassword);

                if (loginResult.Error || loginResult.Data == null)
                {
                    return BadRequest(loginResult);
                }

                if (loginResult.Data.UserType != UserType.Admin)
                {
                    return BadRequest(new UserResponseModel(null, "Invalid user type.", true));
                }

                // Generate JWT Token
                var token = GenerateJwtToken(loginResult.Data);

                return Ok(new UserLoginResponseModel
                {
                    Data = loginResult.Data,
                    Token = token,
                    Message = "Login successful",
                    Error = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during Admin login: {ex.Message}");
                return BadRequest(new UserResponseModel(null, $"Error during login: {ex.Message}", true));
            }
        }


        [HttpPost("LogoutAgent")]
        [Authorize]
        public async Task<IActionResult> LogoutAgent()
        {
            try
            {
                // Log logout activity
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    bool logResult = await _loginActivityRepository.LogLogoutActivity(userId);
                    if (!logResult)
                    {
                        _logger.LogError($"Failed to log logout activity for User {userId}.");
                        return StatusCode(500, "Failed to log logout activity.");
                    }
                }

                // Note: JWT tokens are stateless. To invalidate tokens, consider implementing a token blacklist.
                return Ok(new { Message = "Logout successful." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during Agent logout: {ex.Message}");
                return BadRequest(new { Message = $"Error during logout: {ex.Message}" });
            }
        }


        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT Secret Key is not configured.");
            }

            var key = System.Text.Encoding.UTF8.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.UserType.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        private string GenerateTemporaryPassword()
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()-_=+";
            var passwordChars = new char[12];
            var res = new char[12];
            byte[] uintBuffer = new byte[sizeof(uint)];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                for (int i = 0; i < res.Length; i++)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res[i] = validChars[(int)(num % (uint)validChars.Length)];
                }
            }
            return new string(res);
        }


        [HttpDelete("DeleteAgent/{agentId}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<ActionResult<UserResponseModel>> DeleteAgent(string agentId)
        {
            if (string.IsNullOrEmpty(agentId))
            {
                return BadRequest(new UserResponseModel(null, "Agent ID is required.", true));
            }

            try
            {
                // Retrieve the agent to ensure they exist and are an Agent
                var getUserResult = await _userRepository.GetByIdAsync(agentId);
                if (getUserResult.Error || getUserResult.Data == null)
                {
                    return NotFound(new UserResponseModel(null, "Agent not found.", true));
                }

                if (getUserResult.Data.UserType != UserType.Agent)
                {
                    return BadRequest(new UserResponseModel(null, "Specified user is not an Agent.", true));
                }

                // Update the user's status to Deleted
                var updateStatusResult = await _userRepository.UpdateUserStatus(agentId, UserStatus.Deleted);
                if (updateStatusResult.Error)
                {
                    return BadRequest(updateStatusResult);
                }

                // Log admin's action
                string adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"Admin {adminId} deleted Agent {agentId}.");

                return Ok(new UserResponseModel(null, "Agent deleted successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting agent: {ex.Message}");
                return BadRequest(new UserResponseModel(null, $"Error deleting agent: {ex.Message}", true));
            }
        }


        [HttpDelete("DeleteAdmin/{adminId}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<ActionResult<UserResponseModel>> DeleteAdmin(string adminId)
        {
            if (string.IsNullOrEmpty(adminId))
            {
                return BadRequest(new UserResponseModel(null, "Admin ID is required.", true));
            }

            try
            {
                // Retrieve the admin to ensure they exist and are an Admin
                var getUserResult = await _userRepository.GetByIdAsync(adminId);
                if (getUserResult.Error || getUserResult.Data == null)
                {
                    return NotFound(new UserResponseModel(null, "Admin not found.", true));
                }

                if (getUserResult.Data.UserType != UserType.Admin)
                {
                    return BadRequest(new UserResponseModel(null, "Specified user is not an Admin.", true));
                }

                // Prevent self-deletion
                string currentAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (adminId == currentAdminId)
                {
                    return BadRequest(new UserResponseModel(null, "Admins cannot delete their own account.", true));
                }

                // Update the user's status to Deleted
                var updateStatusResult = await _userRepository.UpdateUserStatus(adminId, UserStatus.Deleted);
                if (updateStatusResult.Error)
                {
                    return BadRequest(updateStatusResult);
                }

                // Log admin's action
                _logger.LogInformation($"Admin {currentAdminId} deleted Admin {adminId}.");

                return Ok(new UserResponseModel(null, "Admin deleted successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting admin: {ex.Message}");
                return BadRequest(new UserResponseModel(null, $"Error deleting admin: {ex.Message}", true));
            }
        }


        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<ActionResult<UserResponseModel>> ChangePassword([FromBody] ChangePasswordRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Hash the current and new passwords
                string currentPasswordHash = _hashingService.Hash(request.CurrentPassword);
                string newPasswordHash = _hashingService.Hash(request.NewPassword);

                // Change password
                var changePasswordResult = await _userRepository.ChangePassword(userId, currentPasswordHash, newPasswordHash);
                if (changePasswordResult.Error)
                {
                    return BadRequest(changePasswordResult);
                }

                // Reset MustChangePassword flag
                var resetFlagResult = await _userRepository.SetMustChangePassword(userId, false);
                if (resetFlagResult.Error)
                {
                    return BadRequest(resetFlagResult);
                }

                return Ok(new UserResponseModel(null, "Password changed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error changing password: {ex.Message}");
                return BadRequest(new UserResponseModel(null, $"Error changing password: {ex.Message}", true));
            }
        }


        [HttpPut("UpdateProfile")]
        [Authorize]
        public async Task<ActionResult<UserResponseModel>> UpdateProfile([FromBody] ProfileUpdateRequestModel updateModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Retrieve the user ID from the JWT token
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new UserResponseModel(null, "Invalid user token.", true));
                }

                // Update the user's profile
                var updateResult = await _userRepository.UpdateAsync(userId, updateModel);
                if (updateResult.Error)
                {
                    return BadRequest(updateResult);
                }

                return Ok(updateResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating profile: {ex.Message}");
                return StatusCode(500, new UserResponseModel(null, "An unexpected error occurred while updating the profile.", true));
            }
        }
    }
}