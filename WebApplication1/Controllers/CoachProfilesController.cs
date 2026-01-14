using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Identity;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoachProfilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoachProfilesController(AppDbContext context, IMapper mapper,
        UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;

        }

        // GET: api/CoachProfiles
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var coaches = await _context.CoachProfiles
            .AsNoTracking()
            .Select(c => new CoachListDto
            {
                CoachId = c.Id,
                FullName = c.User.FirstName + " " + c.User.LastName,
                Email = c.User.Email,
                Specialization = c.Specialization,
                Certifications = c.Certifications,
                CoachingYears = c.CoachingYears,
                BlackBeltRanking = c.BlackBeltRanking
            })
            .OrderBy(c => c.FullName)
            .ToListAsync();
                responseModel.Message = "Session Updated";
                responseModel.Status = true;
                responseModel.Model = coaches;
                return new OkObjectResult(responseModel);
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message.ToString();
                responseModel.Status = false;
                responseModel.Model = null;
                return new BadRequestObjectResult(responseModel);
            }
        }

        // GET: api/CoachProfiles/me
        [HttpGet("me")]
        public async Task<ActionResult<CoachProfileResponseDto>> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var coach = await _context.CoachProfiles
                .Include(c => c.User)
                .Include(c => c.Sessions)
                .Include(c => c.FeedbacksReceived)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (coach == null)
                return NotFound(new { message = "Coach profile not found for current user" });

            return _mapper.Map<CoachProfileResponseDto>(coach);
        }

        // GET: api/CoachProfiles/5
        [HttpPost("get-single-coach")]
        public async Task<IActionResult> Get([FromBody] int id)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var coach = await _context.CoachProfiles
                    .Include(c => c.User)
                    .Include(c => c.Sessions)
                    .Include(c => c.FeedbacksReceived)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (coach == null)
                    throw new Exception("Not Found");

                responseModel.Model = _mapper.Map<CoachProfileResponseDto>(coach);
                responseModel.Status = true;
                responseModel.Message = "Retrieved";
                return new OkObjectResult(responseModel);
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message;
                responseModel.Status = false;
                return new BadRequestObjectResult(responseModel);
            }
        }

        // POST: api/CoachProfiles
        [HttpPost]
        public async Task<ActionResult<CoachProfileResponseDto>> Create(CreateCoachProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userNameFromToken = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            bool isAdmin = User.IsInRole("Admin");

            string targetUserId;
            string targetUserName;

            if (isAdmin && !string.IsNullOrEmpty(dto.UserName))
            {
                // Admin يمكنه تحديد أي مستخدم
                var targetUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == dto.UserName);

                if (targetUser == null)
                    return BadRequest("User not found.");

                targetUserId = targetUser.Id;
                targetUserName = targetUser.UserName;
            }
            else
            {
                // المستخدم العادي فقط لحسابه
                targetUserId = userId;
                targetUserName = userNameFromToken;
            }

            // منع التكرار
            var exists = await _context.CoachProfiles
                .AnyAsync(c => c.UserId == targetUserId);

            if (exists)
                return BadRequest("Coach profile already exists for this user.");

            var coach = _mapper.Map<CoachProfile>(dto);
            coach.UserId = targetUserId;
            coach.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);

            _context.CoachProfiles.Add(coach);
            await _context.SaveChangesAsync();

            var responseDto = _mapper.Map<CoachProfileResponseDto>(coach);
            return CreatedAtAction(nameof(Get), new { id = coach.Id }, responseDto);
        }

        // PUT: api/CoachProfiles/5
        [HttpPost("update-coach-profile")]
        public async Task<IActionResult> Update([FromBody] UpdateCoachProfileDto dto)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var coach = await _context.CoachProfiles
       .Include(c => c.User)
       .SingleAsync(c => c.Id == dto.Id);

                coach.User.FirstName = dto.FirstName;
                coach.User.LastName = dto.LastName;

                if (coach.User.Email != dto.Email)
                {
                    await _userManager.SetEmailAsync(coach.User, dto.Email);
                    await _userManager.SetUserNameAsync(coach.User, dto.Email);
                }

                coach.Specialization = dto.Specialization;
                coach.BlackBeltRanking = dto.BlackBeltRanking;
                coach.CoachingYears = dto.CoachingYears;

                await _context.SaveChangesAsync();

                responseModel.Message = "Session Updated";
                responseModel.Status = true;
                responseModel.Model = coach;
                return new OkObjectResult(responseModel);
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message.ToString();
                responseModel.Status = false;
                responseModel.Model = null;
                return new BadRequestObjectResult(responseModel);
            }
        }

        // DELETE: api/CoachProfiles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var coach = await _context.CoachProfiles.FindAsync(id);
            if (coach == null)
                return NotFound();

            _context.CoachProfiles.Remove(coach);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
