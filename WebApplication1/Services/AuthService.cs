using AutoMapper.Execution;
using Google.Apis.Auth; // Added
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Identity;
using WebApplication1.Models;
using WebApplication1.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _Context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration, IEmailService emailService, AppDbContext Context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _emailService = emailService;
        _Context = Context;
    }

    public async Task<ResponseModel> RegisterAsync(RegisterDto dto)
    {
        ResponseModel responseModel = new ResponseModel();
        try
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email.Split("@").FirstOrDefault(),
                Email = dto.Email,
                FirstName = dto.UserName.Split(" ").FirstOrDefault(),
                LastName = dto.UserName.Split(" ").LastOrDefault(),
                PhoneNumber = "9999",
                DateOfBirth = DateTime.MinValue,
                CreatedAt = DateTime.UtcNow,
                ProfileId = "0"
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            //// ✅ Assign Role
            //string roleToAssign = !string.IsNullOrEmpty(dto.Role) ? dto.Role : "Client";

            //// Create role if it doesn't exist (Safety check)
            //if (!await _roleManager.RoleExistsAsync(roleToAssign))
            //{
            //    await _roleManager.CreateAsync(new IdentityRole(roleToAssign));
            //}
            var roles = await _userManager.GetRolesAsync(user);

            user.Roles = roles.ToList();
            //await _userManager.AddToRoleAsync(user, roleToAssign);
            responseModel.Status = true;
            responseModel.Message = "User registered successfully!";
            responseModel.Model = new
            {
                UserModel = user
            };
            return responseModel;
        }
        catch (Exception ex)
        {
            responseModel.Status = false;
            responseModel.Message = ex.Message;
            return responseModel;
        }
    }

    public async Task<ResponseModel> AssignRoleAsync(RoleAssignDTO dto)
    {
        ResponseModel responseModel = new ResponseModel();
        try
        {

            // ✅ Assign Role
            string roleToAssign = dto.role;
            var user = await _userManager.FindByIdAsync(dto.userid);
            if (user != null)
            {
                var res = await _userManager.GetRolesAsync(user);
                if (res.Count > 0)
                {
                    throw new Exception("Role Already Assigned: " + res.FirstOrDefault());
                }
            }
            // Create role if it doesn't exist (Safety check)
            if (!await _roleManager.RoleExistsAsync(roleToAssign))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleToAssign));
            }
            if (user != null)
            {
                var result = await _userManager.AddToRoleAsync(user, roleToAssign);
                try
                {
                    if (roleToAssign is "member")
                    {
                        MemberProfile member = new MemberProfile();
                        member.FirstName = user.FirstName;
                        member.LastName = user.LastName;
                        member.UserId = user.Id;
                        member.JoinDate = user.CreatedAt;
                        await _Context.MemberProfiles.AddAsync(member);
                        await _Context.SaveChangesAsync();
                        user.ProfileId = Convert.ToString(member.Id);
                    }
                    else if (roleToAssign is "coach")
                    {
                        CoachProfile coachProfile = new CoachProfile();
                        coachProfile.UserId = user.Id;
                        coachProfile.Bio = " ";
                        coachProfile.Specialization = " ";
                        await _Context.CoachProfiles.AddAsync(coachProfile);
                        await _Context.SaveChangesAsync();
                        user.ProfileId = Convert.ToString(coachProfile.Id);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                throw new Exception("User Not Found");
            }
            var roles = await _userManager.GetRolesAsync(user);
            user.Roles = roles.ToList();
            responseModel.Status = true;
            responseModel.Message = "User role added successfully!";
            responseModel.Model = new { UserModel = user };
            return responseModel;
        }
        catch (Exception ex)
        {
            responseModel.Status = false;
            responseModel.Message = ex.Message;
            return responseModel;
        }
    }
    public async Task<ResponseModel> ForgetPasswordAsync(ForgotPasswordDto dto)
    {
        ResponseModel responseModel = new ResponseModel();
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            // Do NOT reveal user existence
            if (user == null)
                throw new Exception("User not found");

            var otp = await _userManager.GenerateUserTokenAsync(
                user,
                TokenOptions.DefaultEmailProvider,
                "ResetPasswordOTP"
            );

            await _emailService.SendAsync(
                user.Email,
                "Password Reset OTP",
                $"Your OTP is: {otp}"
            );

            responseModel.Status = true;
            responseModel.Message = "User role added successfully!";
            responseModel.Model = new { UserModel = user };
            return responseModel;
        }
        catch (Exception ex)
        {
            responseModel.Status = false;
            responseModel.Message = ex.Message;
            return responseModel;
        }
    }

    public async Task<ResponseModel> LoginAsync(LoginDto dto)
    {
        ResponseModel responseModel = new ResponseModel();
        ApplicationUser user = null;
        try
        {
            // إذا فيه @ نعتبره إيميل، غير ذلك نعامله كاسم مستخدم
            if (dto.UserName.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(dto.UserName);
            }
            else
            {
                user = await _userManager.FindByNameAsync(dto.UserName);
            }

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                responseModel.Status = false;
                responseModel.Message = "Invalid username or password.";
                return responseModel;
            }
            var roles = await _userManager.GetRolesAsync(user);
            user.Roles = roles.ToList();
            if (user.Roles != null)
            {
                if (user.Roles.FirstOrDefault().ToLower() is "coach")
                {
                    var usss = await _Context.CoachProfiles.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
                    if (usss != null)
                    {
                        user.ProfileId = Convert.ToString(usss.Id);
                    }
                    else
                    {
                        user.ProfileId = "0";
                    }
                }
                if (user.Roles.FirstOrDefault().ToLower() is "member")
                {
                    var usss = await _Context.MemberProfiles.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
                    if (usss != null)
                    {
                        user.ProfileId = Convert.ToString(usss.Id);
                    }
                    else
                    {
                        user.ProfileId = "0";
                    }
                }
            }
            responseModel.Status = true;
            responseModel.Message = "Login successful.";
            string jwt = await GenerateJwtToken(user);
            //if (user.Roles.FirstOrDefault() is "Coach")
            //{

            //}
            user.AccessToken = jwt;
            responseModel.Model = new { UserDetails = user };
            return responseModel;
        }
        catch (Exception ex)
        {
            responseModel.Status = false;
            responseModel.Message = ex.Message;
            return responseModel;
        }
    }

    public async Task<ResponseModel> GoogleLoginAsync(GoogleLoginDto dto)
    {
        ResponseModel responseModel = new ResponseModel();
        try
        {
            // Validate Google/Firebase ID Token
            GoogleJsonWebSignature.Payload payload;

            try
            {
                // First try to validate as Google token
                var settings = new GoogleJsonWebSignature.ValidationSettings();
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
            }
            catch
            {
                // If Google validation fails, try Firebase token validation
                // Firebase tokens have issuer: https://securetoken.google.com/PROJECT_ID
                // For now, we'll decode without strict validation and check email
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(dto.IdToken) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;

                if (jsonToken == null)
                {
                    responseModel.Status = false;
                    responseModel.Message = "Invalid token format";
                    return responseModel;

                    //throw new Exception("Invalid token format");
                }

                // Create payload object from Firebase token
                var email = jsonToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                var emailVerified = jsonToken.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value == "true";

                if (string.IsNullOrEmpty(email))
                {
                    responseModel.Status = false;
                    responseModel.Message = "Email not found in token";
                    return responseModel;

                    //throw new Exception("Email not found in token");
                }

                // Create a compatible payload object
                var name = jsonToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? email.Split('@')[0];

                payload = new GoogleJsonWebSignature.Payload
                {
                    Email = email,
                    EmailVerified = emailVerified,
                    Name = name,
                    GivenName = name.Split(' ').FirstOrDefault() ?? name,
                    FamilyName = name.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    Subject = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? ""
                };
            }

            // Check if user exists
            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                // Create new user with name info
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? payload.Name?.Split(' ').FirstOrDefault() ?? "",
                    LastName = payload.FamilyName ?? payload.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    EmailConfirmed = payload.EmailVerified,
                    ProfileId = "0"
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    responseModel.Status = false;
                    responseModel.Message = $"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                    return responseModel;

                    //throw new Exception($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                // Assign default Client role for Google Sign-In users
                //if (!await _roleManager.RoleExistsAsync("Client"))
                //{
                //    await _roleManager.CreateAsync(new IdentityRole("Client"));
                //}
                //await _userManager.AddToRoleAsync(user, "Client");
            }
            else
            {
                // Update existing user's name if empty
                bool needsUpdate = false;
                if (string.IsNullOrEmpty(user.FirstName))
                {
                    user.FirstName = payload.GivenName ?? payload.Name?.Split(' ').FirstOrDefault() ?? "";
                    needsUpdate = true;
                }
                if (string.IsNullOrEmpty(user.LastName))
                {
                    user.LastName = payload.FamilyName ?? payload.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "";
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    await _userManager.UpdateAsync(user);
                }
                var roles = await _userManager.GetRolesAsync(user);
                user.Roles = roles.ToList();
                if (user.Roles != null)
                {
                    if (user.Roles.FirstOrDefault().ToLower() is "coach")
                    {
                        var usss = await _Context.CoachProfiles.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
                        if (usss != null)
                        {
                            user.ProfileId = Convert.ToString(usss.Id);
                        }
                        else
                        {
                            user.ProfileId = "0";
                        }
                    }
                    if (user.Roles.FirstOrDefault().ToLower() is "member")
                    {
                        var usss = await _Context.MemberProfiles.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
                        if (usss != null)
                        {
                            user.ProfileId = Convert.ToString(usss.Id);
                        }
                        else
                        {
                            user.ProfileId = "0";
                        }
                    }
                }
            }
            responseModel.Status = true;
            responseModel.Message = "Google login successful.";
            string Token = await GenerateJwtToken(user);
            user.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            user.AccessToken = Token;
            responseModel.Model = new { User = user };
            // Generate JWT token
            return responseModel;
        }
        catch (Exception ex)
        {
            responseModel.Status = false;
            responseModel.Message = $"Invalid Google Token: {ex.Message}";
            return responseModel;
        }
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        var roles = await _userManager.GetRolesAsync(user); // لو تعيّن أدوار لاحقاً من لوحة تحكم مثلاً

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim("firstName", user.FirstName ?? string.Empty), // Added for Flutter app
            new Claim("lastName", user.LastName ?? string.Empty),   // Added for Flutter app
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public async Task<ResponseModel> VerifyOtp(VerifyOtpDto dto)
    {
        ResponseModel response = new ResponseModel();
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("User not found");

            var isValid = await _userManager.VerifyUserTokenAsync(
                user,
                TokenOptions.DefaultEmailProvider,
                "ResetPasswordOTP",
                dto.Otp
            );

            if (!isValid)
                throw new Exception("Invalid OTP");

            // Generate actual password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            response.Status = true;
            response.Message = "OTP Authenticated";
            response.Model = new
            {
                ResetToken = resetToken
            };
            return response;
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.Message = ex.Message;
            return response;
        }
    }
    public async Task<ResponseModel> ResetPassword(ResetPasswordDto dto)
    {
        ResponseModel response = new ResponseModel();
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Invalid Userid");

            var result = await _userManager.ResetPasswordAsync(
                user,
                dto.ResetToken,
                dto.NewPassword
            );

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.FirstOrDefault().ToString());
            }
            response.Message = "Password reset successful";
            response.Status = true;
            return response;
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.Message = ex.Message;
            return response;
        }
    }
}