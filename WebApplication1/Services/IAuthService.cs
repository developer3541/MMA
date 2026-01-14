using System.Threading.Tasks;
using WebApplication1.DTOs; // تأكد من مسار DTOs
using WebApplication1.Identity;

public interface IAuthService
{
    Task<ResponseModel> RegisterAsync(RegisterDto dto);
    Task<ResponseModel> LoginAsync(LoginDto dto);
    Task<ResponseModel> GoogleLoginAsync(GoogleLoginDto dto);
    Task<ResponseModel> AssignRoleAsync(RoleAssignDTO dto);
    Task<ResponseModel> ForgetPasswordAsync(ForgotPasswordDto dto);
    Task<ResponseModel> ResetPassword(ResetPasswordDto dto);
    Task<ResponseModel> VerifyOtp(VerifyOtpDto dto);
}
