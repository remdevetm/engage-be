//using GlobalShared.Lib.Helpers;
//using GlobalShared.Lib.Services;
//using GlobalShared.Lib.Templates;
//using Microsoft.AspNetCore.Mvc;
//using System.Net;
//using System.Reflection;
//using UserShared.Lib.Models;
//using UserShared.Lib.RequestModels;
//using UserShared.Lib.ResponseModels;
//using UserShared.Lib.Services;
//using UserWrite.API.Repositories.Interfaces;
//using UserWrite.API.Services;
//using UserWrite.API.Services.Interfaces;

//namespace UserWrite.API.Controllers
//{
//    [Route("api/v1/[controller]")]
//    [ApiController]
//    // [Authorize]
//    public class UsersController : ControllerBase
//    {
//        private readonly ILogger<UsersController> _logger;
//        private readonly IUserRepository _userRepository;
//        private readonly IHashingService _hashingService;
//        private readonly IEmailService _emailService;
//        private readonly ISmsService _smsService;
//        private readonly IProfileRepository _profileRepository;
//        private readonly IEnrollmentWriteService _writeService;
//        private readonly IStudentRepository _studentRepository;
//        private readonly IOrganizationProgramEnrollmentRepository _organizationProgramEnrollmentRepository;
//        private readonly ITenantService _tenantService;
//        public UsersController(
//            ILogger<UsersController> logger,
//            IUserRepository userRepository,
//            IHashingService hashingService,
//            IEmailService emailService,
//            ISmsService smsService,
//            IProfileRepository profileRepository,
//            IEnrollmentWriteService writeService,
//            IStudentRepository studentRepository,
//            IOrganizationProgramEnrollmentRepository organizationProgramEnrollmentRepository,
//            ITenantService tenantService)
//        {
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
//            _hashingService = hashingService ?? throw new ArgumentNullException(nameof(hashingService));
//            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
//            _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
//            _writeService = writeService ?? throw new ArgumentNullException(nameof(writeService));
//            _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
//            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
//            _organizationProgramEnrollmentRepository = organizationProgramEnrollmentRepository ?? throw new ArgumentNullException(nameof(organizationProgramEnrollmentRepository));
//            _tenantService = tenantService ?? throw new ArgumentNullException();
//        }

//        [HttpPost("Login")]
//        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType((int)HttpStatusCode.NotFound)]
//        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
//        public async Task<ActionResult<UserResponseModel>> Login(UserLoginRequestModel request)
//        {
//            var user = await _userRepository.GetUserByEmail(request.Email.ToLower());
//            if (user == null)
//            {
//                return NotFound(new UserResponseModel(null,"User is not found"));
//            }
//            if(user.IsEmailVerified==false)
//            {
//                return Ok(new UserResponseModel(null, "User is not verified"));
//            }

//            var result = await  _userRepository.Login(request, _hashingService.Hash(request.Password));
//            if (result.Data == null && !result.Error)
//                return NotFound(result);
//            if (result.Error)
//                return StatusCode(400, result);
//            return Ok(result);
//        }

//        [HttpPost("AdminLogin")]
//        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType((int)HttpStatusCode.NotFound)]
//        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
//        public async Task<ActionResult<UserResponseModel>> AdminLogin(UserLoginRequestModel request)
//        {
//            var result = await ExecuteWithLogging(() => _userRepository.AdminLogin(request, _hashingService.Hash(request.Password)));
//            if (result.Data == null && !result.Error)
//                return NotFound(result);
//            if (result.Error)
//                return StatusCode(401, result);
//            return Ok(result);
//        }

//        [HttpPost("SuperAdminLogin")]
//        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType((int)HttpStatusCode.NotFound)]
//        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
//        public async Task<ActionResult<UserResponseModel>> SuperAdminLogin(UserLoginRequestModel request)
//        {
//            var result = await ExecuteWithLogging(() => _userRepository.SuperAdminLogin(request, _hashingService.Hash(request.Password)));
//            if (result.Data == null && !result.Error)
//                return NotFound(result);
//            if (result.Error)
//                return StatusCode(401, result);
//            return Ok(result);
//        }

//        [HttpPost("AddAdmin")] // Also Applies to freemium
//        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
//        public async Task<ActionResult<UserResponseModel>> AddAdminAsync(UserRequestModel model)
//        {

//            var otp = new Random().Next(10000, 100000).ToString();
//            model.Status = UserStatus.New;
//            model.Role =model.UserType==UserTypes.Freemium ?"Freemium":"Admin";
//            model.Password = _hashingService.Hash(model.Password);
//            model.Otp = otp;
//            await _emailService.SendEmail(model.Email.ToLower(), $"Your OTP: {otp}", _emailService.GetEmailBody(EmailType.OTP,otp));
//            var result = await _userRepository.CreateUser(model);
//            return result.Data != null ? Ok(result) : BadRequest(result);
//        }

//        [HttpPost("AddHostUser")]
//        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
//        public async Task<ActionResult<UserResponseModel>> AddHostAsync(UserRequestModel model)
//        {
//            var tempPassword = GeneratePassword.GenerateTemporaryPassword();
//            var otp = new Random().Next(10000, 100000).ToString();
//            model.Status = UserStatus.New;
//            model.Role = "Host";
//            model.Password = _hashingService.Hash(tempPassword);
//            model.Otp = otp;
//            var result = await _userRepository.CreateUser(model);

//            await _emailService.SendEmail(model.Email.ToLower(),
//                  "Welcome to our Platform",
//                  _emailService.GetEmailBody(EmailType.LoginDetail,
//                  $"{model.FirstName},{model.Email},{tempPassword},Host"));

//            return result.Data != null ? Ok(result) : BadRequest(result);
//        }

//        [HttpPost("AddASuperAdmin")]
//        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
//        public async Task<ActionResult<UserResponseModel>> AddSuperAdminAsync(UserRequestModel model)
//        {
//            model.Status = UserStatus.New;
//            model.Role = "SuperAdmin";
//            model.Password = _hashingService.Hash(model.Password);
//            var result = await _userRepository.CreateUser(model);
//            return result.Data != null ? Ok(result) : BadRequest(result);
//        }

//        [HttpPut("AdminResetPassword")]
//        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]

//        public async Task<ActionResult<UserResponseModel>> AdminResetPassword(AdminResetPasswordRequestModel  request)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }

//            var newPasswordHash= _hashingService.Hash(request.NewPassword);
//            var oldPasswordHash = _hashingService.Hash(request.OldPassword);
//            var result = await _userRepository.AdminResetPassword(request, newPasswordHash, oldPasswordHash);
//            return result.Data != null ? Ok(result) : BadRequest(result);
//        }

//        [HttpPut("AdminUpdateUser")]  // Also Applies to freemium
//        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
//        public async Task<ActionResult<UserResponseModel>> AdminUpdateUser(AdminUpdateRequestModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }
//            var result = await ExecuteWithLogging(() => _userRepository.AdminUpdateUser(model));
//            return result.Data != null ? Ok(result) : NotFound(result);
//        }

//        [HttpPut("UpdateHostUser")] 
//        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
//        public async Task<ActionResult<UserResponseModel>> UpdateHostUser(UserRequestModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }
//            var result = await ExecuteWithLogging(() => _userRepository.UpdateHostUser(model));
//            return result.Data != null ? Ok(result) : NotFound(result);
//        }

//        [HttpPost("RegisterStudent")]
//        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
//        public async Task<ActionResult<ProfileResponseModel>> RegisterStudent(CourseRegisterRequestModel model)
//        {
//            if (model.Password != model.ConfirmPassword)
//            {
//                return BadRequest("Passwords do not match.");
//            }
//            if(model.CourseId==null)
//            {
//                return BadRequest("CourseId is required");
//            }
//            if
//                (!Request.Headers.TryGetValue("Client-Key", out var clientKey))
//            {
//                return Unauthorized("Client key is missing");
//            }

//            var tenant = await _tenantService.GetTenantNameByClientKey(clientKey);

//            var otp = new Random().Next(10000, 100000).ToString();
//            model.Password = _hashingService.Hash(model.Password);
//            var newUser = new RegisterRequestModel()
//            {
//                Email = model.Email.ToLower(),
//                Password = model.Password,
//                Username = model.Username,
//                PhoneNumber = model.PhoneNumber
//            };
//            var result = await _userRepository.RegisterUser(newUser, otp,"Student");
         
           

//            if (string.IsNullOrEmpty(result.Message))
//            {
//                await _studentRepository.AddStudentInformation(result.Data.Id);
//                await _smsService.SendSms($"{model.PhoneNumber}", $"Dear recipient,\n\nPlease find below the details of your important notification.\n\nOTP: {otp}\n\nBest regards,\n{tenant}");
//                // await _emailService.SendEmail(model.Email, $"Your OTP: {otp}", $"Dear recipient,\n\nPlease find below the details of your important notification.\n\nOTP: {otp}\n\nBest regards,\nThooto");
//                if (model.StudentRegistrationType == UserShared.Lib.SharedModels.StudentRegistrationType.Program)
//                {
//                    var organizationProgramEnrollmentRequest = new OrganizationProgramEnrollmentRequestModel()
//                    {
//                        OrganizationProgramId = model.CourseId,
//                        UserId = result.Data.Id,
//                        CreatingUser = result.Data.Id
//                    };
//                    _ = await _organizationProgramEnrollmentRepository.CreateOrganizationProgramEnrollment(organizationProgramEnrollmentRequest);
//                }
//                else
//                {
//                    var enrollmentRequest = new EnrollmentRequestModel()
//                    {
//                        Course = model.CourseId,
//                        UserId = result.Data.Id,
//                        OrganizationId = model.OrganizationId,
//                        CreatingUser = result.Data.Id,
                        
//                    };
//                    _ = await _writeService.CreateEnrollment(enrollmentRequest);
//                }
//                var request = new ProfileRequestModel()
//                {
//                    UserId = result.Data.Id,
//                    Email = result.Data.Email,
//                    PhoneNumber = model.PhoneNumber
//                };
//                var response = await _profileRepository.SubmitStudentDetails(request);

//                return result.Data != null ? Ok(response) : BadRequest(response);
//            }
//            var profileResponse = await _profileRepository.GetProfile(result.Data.Id);
//            profileResponse.Message = result.Message;
//            return Ok(profileResponse);
           
//        }


//        [HttpPost("RegisterUser")]
//        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
//        public async Task<ActionResult<RegisterResponse>> RegisterUser(RegisterRequestModel model)
//        {

//            if (model.Password != model.ConfirmPassword)
//            {
//                return BadRequest("Passwords do not match.");
//            }
//            var otp = new Random().Next(10000, 100000).ToString();
//            model.Password = _hashingService.Hash(model.Password);
//            var result = await _userRepository.RegisterUser(model, otp,"Admin");
//            if (string.IsNullOrEmpty(result.Message))
//            {
//                //await _smsService.SendSms(model.Phone, $"Dear recipient,\n\nPlease find below the details of your important notification.\n\nOTP: {otp}\n\nBest regards,\nThooto");
//                await _emailService.SendEmail(model.Email, $"Your OTP: {otp}", _emailService.GetEmailBody(EmailType.OTP, otp));
//            }

//            var request = new ProfileRequestModel()
//            {
//                UserId = result.Data.Id,
//                Email = result.Data.Email.ToLower(),
//            };
//            _ = await _profileRepository.SubmitStudentDetails(request);
//            return result.Data != null ? Ok(result) : BadRequest(result);
//        }

//        [HttpPost("ReSendOTP")]
//        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
//        public async Task<ActionResult<UserResponseModel>> ReSendOTP(SmsOTPRequestModel model)
//        {
//            var user = await _userRepository.GetUserPhoneNumber(model.PhoneNumber);
//            if (user== null)
//            {
//                _logger.LogError($"User with Phone: {model.PhoneNumber} does not exist");
//                return NotFound("User does not exist");
//            }
//            if(!Request.Headers.TryGetValue("Client-Key", out var clientKey))
//            {
//                return Unauthorized("Client key is missing");
//            }

//            var tenant = await _tenantService.GetTenantNameByClientKey(clientKey);

//            var otp = new Random().Next(10000, 100000).ToString();
//            user.Otp = otp;

//            await _smsService.SendSms(model.PhoneNumber, $"Dear recipient,\n\nPlease find below the details of your important notification.\n\nOTP: {otp}\n\nBest regards,\n{tenant}");
//            await _userRepository.UpdateUserOTP(user);
//            var response = new {Email=user.Email,PhoneNumber=user.PhoneNumber};
//            return Ok(response);
//        }

//        [HttpPost("VerifyOTP")]
//        public async Task<ActionResult<UserResponseModel>> VerifyOTP(VerifyOtpRequestModel model)
//        {
//            var result = await _userRepository.VerifyOTP(model);
//            return result.Data!=null?Ok(result):BadRequest(result);
//        }

//        [HttpPost("SendResetPasswordOtp")]
//        [ProducesResponseType((int)HttpStatusCode.NotFound)]
//        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
//        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
//        public async Task<ActionResult<UserResponseModel>> SendResetPasswordOtpAsync(SendOTPRequestModel request)
//        {

//            var user = await _userRepository.GetUserByEmail(request.Email.ToLower());
//            if (user == null)
//            {
//                _logger.LogError($"User with email: {request.Email} does not exist");
//                return NotFound("User does not exist");
//            }

//            var result = await ExecuteWithLogging(async () =>
//            {
//                var otp = new Random().Next(10000, 100000).ToString();
//                user.Otp = otp;
//                await _emailService.SendEmail(request.Email, $"Your OTP: {otp}", _emailService.GetEmailBody(EmailType.OTP, otp));
//                //await _smsService.SendSms("model.Phone", $"Dear recipient,\n\nPlease find below the details of your important notification.\n\nOTP: {otp}\n\nBest regards,\nThooto");
//                await _userRepository.UpdateUserOTP(user);
//                return new UserResponseModel(user);
//            });
//            if (result.Data == null)
//                return BadRequest();
//            result.Data.Otp = "";
//            return Ok(result);
//        }

//        [HttpPut("ResetPassword")]
//        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
//        [ProducesResponseType((int)HttpStatusCode.NotFound)]
//        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
//        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
//        public async Task<ActionResult<UserResponseModel>> ResetPasswordAsync(UserVerifyOtpRequestModel userModel)
//        {
//            try
//            {
//                var user = await _userRepository.GetUserByEmail(userModel.Email.ToLower());
//                if (user!= null)
//                {
//                    if (user.Otp != userModel.Otp)
//                    {
//                        _logger.LogError($"Otps does not match");
//                        return BadRequest();
//                    }
//                    user.Password = _hashingService.Hash(userModel.Password);
//                    var result = await ExecuteWithLogging(() => _userRepository.UpdateUserPasssword(user));
//                    if (result.Error)
//                        return StatusCode(500, result);
//                    return Ok(result);
//                }
//                else
//                    return NotFound(user);
//            }
//            catch (Exception e)
//            {
//                return StatusCode(500, $"An error occurred, ERROR: {e.Message}");
//            }
//        }

//        [HttpPut("UpdateUser")]
//        [ProducesResponseType(typeof(UserResponseModel), (int)HttpStatusCode.OK)]
//        public async Task<ActionResult<UserResponseModel>> UpdateUserAsync(UserRequestModel model)
//        {
//            var result = await ExecuteWithLogging(() => _userRepository.UpdateUser(model));
//            return result.Data != null ? Ok(result) : NotFound(result);
//        }

//        [HttpDelete("DeleteUser")]
//        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
//        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
//        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
//        public async Task<IActionResult> DeleteUserAsync(string Id)
//        {
//            var rowsDeleted = await _userRepository.DeleteUser(Id);
//            if (rowsDeleted == 0)
//                return Unauthorized("User not deleted");
//            else if (rowsDeleted == 1)
//                return Ok("Successfully deleted user");
//            else
//            {
//                _logger.LogError($"Multiple users deleted, USERID {Id}, ROWS DELETED{rowsDeleted}");
//                return StatusCode(500, "Multiple users deleted");
//            }
//        }

//        private async Task<UserResponseModel> ExecuteWithLogging(Func<Task<UserResponseModel>> operation)
//        {
//            var logName = MethodBase.GetCurrentMethod()?.Name;
//            _logger.LogInformation("[BEGIN] " + logName);
//            var result = await operation.Invoke();
//            _logger.LogInformation("[END] " + logName);
//            return result;
//        }
//    }

//}
