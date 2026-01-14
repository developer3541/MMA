using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // -----------------------------
        // Register User
        // -----------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var registermodel = await _authService.RegisterAsync(model);
                //responseModel.Status = true;
                //responseModel.Message = "User registered successfully!";
                //responseModel.Model = new { UserId = userId };
                if (registermodel.Status)
                {
                    return new OkObjectResult(registermodel);
                }
                else
                {
                    return new BadRequestObjectResult(registermodel);
                }
                //return Ok(new { Message = "User registered successfully!", UserId = userId });
            }
            catch (Exception ex)
            {
                responseModel.Status = false;
                responseModel.Message = ex.Message + ex.StackTrace;
                return new BadRequestObjectResult(responseModel);

                //return BadRequest(new { Message = ex.Message });
            }
        }

        // -----------------------------
        // Login User
        // -----------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                var token = await _authService.LoginAsync(model); // لاحقًا يمكن تعديل Login لإرجاع JWT
                if (token.Status)
                {
                    return new OkObjectResult(token);
                }
                else
                {
                    return new UnauthorizedObjectResult(token);
                }
                //return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                ResponseModel responseModel = new ResponseModel();
                responseModel.Status = false;
                responseModel.Message = ex.Message + ex.StackTrace;
                return new BadRequestObjectResult(responseModel);
                //return Unauthorized(new { Message = ex.Message });
            }
        }

        // -----------------------------
        // Google Login
        // -----------------------------
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto model)
        {
            try
            {
                var token = await _authService.GoogleLoginAsync(model);
                if (token.Status)
                {
                    return new OkObjectResult(token);
                }
                else
                {
                    return new UnauthorizedObjectResult(token);
                }
            }
            catch (Exception ex)
            {
                ResponseModel responseModel = new ResponseModel();
                responseModel.Status = false;
                responseModel.Message = ex.Message + ex.StackTrace;
                return new BadRequestObjectResult(responseModel);
            }
        }
        [HttpPost("role-assign")]
        public async Task<IActionResult> AssignRoleAsync([FromBody] RoleAssignDTO model)
        {
            try
            {
                var token = await _authService.AssignRoleAsync(model);
                if (token.Status)
                {
                    return new OkObjectResult(token);
                }
                else
                {
                    return new BadRequestObjectResult(token);
                }
            }
            catch (Exception ex)
            {
                ResponseModel responseModel = new ResponseModel();
                responseModel.Status = false;
                responseModel.Message = ex.Message + ex.StackTrace;
                return new BadRequestObjectResult(responseModel);
            }
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            try
            {
                ResponseModel response = await _authService.ForgetPasswordAsync(dto);
                if (response.Status)
                {
                    return new OkObjectResult(response);
                }
                else
                {
                    return new BadRequestObjectResult(response);

                }
            }
            catch (Exception ex)
            {
                ResponseModel responseModel = new ResponseModel();
                responseModel.Status = false;
                responseModel.Message = ex.Message + ex.StackTrace;
                return new BadRequestObjectResult(responseModel);
            }
        }
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            try
            {
                ResponseModel responseModel = new ResponseModel();
                responseModel = await _authService.VerifyOtp(dto);
                if (responseModel.Status)
                {
                    return new OkObjectResult(responseModel);
                }
                else
                {
                    return new BadRequestObjectResult(responseModel);
                }
            }
            catch (Exception ex)
            {
                ResponseModel responseModel = new ResponseModel();
                responseModel.Status = false;
                responseModel.Message = ex.Message + ex.StackTrace;
                return new BadRequestObjectResult(responseModel);
            }
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            try
            {
                ResponseModel responseModel = new ResponseModel();
                responseModel = await _authService.ResetPassword(dto);
                if (responseModel.Status)
                {
                    return new OkObjectResult(responseModel);
                }
                else
                {
                    return new BadRequestObjectResult(responseModel);
                }
            }
            catch (Exception ex)
            {
                ResponseModel responseModel = new ResponseModel();
                responseModel.Status = false;
                responseModel.Message = ex.Message + ex.StackTrace;
                return new BadRequestObjectResult(responseModel);
            }
        }
    }
}
