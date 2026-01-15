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
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "")] // حماية بالـ JWT
    public class MemberProfilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemberStatsService _statsService;
        private readonly ISessionQueryService _sessionQueryService;
        private readonly ILeaderboardService _leaderboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberProfilesController(AppDbContext context, IMapper mapper, IMemberStatsService memberStatsService, ISessionQueryService sessionQueryService, ILeaderboardService leaderboardService,
        UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _statsService = memberStatsService;
            _sessionQueryService = sessionQueryService;
            _leaderboardService = leaderboardService;
            _userManager = userManager;
        }

        // GET: api/MemberProfiles
        [HttpGet("get-all-members")]
        public async Task<IActionResult> GetAll()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                var members = await _context.MemberProfiles
                    .Include(m => m.User)
                    .Include(m => m.Bookings)
                    .Include(m => m.Attendances)
                    .Include(m => m.FeedbacksGiven)
                    .Include(m => m.ProgressRecords)
                    .ToListAsync();

                var res = _mapper.Map<List<MemberProfileResponseDto>>(members);
                resp.Model = res;
                resp.Status = true;
                resp.Message = "Data Found";
                return new OkObjectResult(resp);
            }
            catch (Exception ex)
            {
                resp.Model = null;
                resp.Status = false;
                resp.Message = ex.Message.ToString();
                return new BadRequestObjectResult(resp);
            }
        }

        // GET: api/MemberProfiles/me
        [HttpPost("get-my-profile")]
        public async Task<ActionResult<MemberProfileResponseDto>> GetMyProfile([FromBody] int memberid)
        {
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //if (string.IsNullOrEmpty(userId))
            //    return Unauthorized();
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var member = await _context.MemberProfiles
                    .Include(m => m.User)
                    .Include(m => m.Bookings)
                    .Include(m => m.Attendances)
                    .Include(m => m.FeedbacksGiven)
                    .Include(m => m.ProgressRecords)
                    .FirstOrDefaultAsync(m => m.Id == memberid);
                if (member == null)
                    throw new Exception("Member profile not found for current user");
                responseModel.Status = true;
                responseModel.Message = "Fetched Data";
                responseModel.Model = _mapper.Map<MemberProfileResponseDto>(member);
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

        // GET: api/MemberProfiles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MemberProfileResponseDto>> Get(int id)
        {
            var member = await _context.MemberProfiles
                .Include(m => m.User)
                .Include(m => m.Bookings)
                .Include(m => m.Attendances)
                .Include(m => m.FeedbacksGiven)
                .Include(m => m.ProgressRecords)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
                return NotFound();

            return _mapper.Map<MemberProfileResponseDto>(member);
        }

        // POST: api/MemberProfiles
        [HttpPost]
        public async Task<ActionResult<MemberProfileResponseDto>> Create(CreateMemberProfileDto dto)
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
                // Admin يحدد username
                var targetUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == dto.UserName);

                if (targetUser == null)
                    return BadRequest("User not found.");

                targetUserId = targetUser.Id;
                targetUserName = targetUser.UserName;
            }
            else
            {
                // المستخدم العادي ينشئ فقط لحسابه
                targetUserId = userId;
                targetUserName = userNameFromToken;
            }

            // منع التكرار
            var exists = await _context.MemberProfiles
                .AnyAsync(m => m.UserId == targetUserId);

            if (exists)
                return BadRequest("Member profile already exists for this user.");

            var member = _mapper.Map<MemberProfile>(dto);
            member.UserId = targetUserId;
            member.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);

            _context.MemberProfiles.Add(member);
            await _context.SaveChangesAsync();

            var responseDto = _mapper.Map<MemberProfileResponseDto>(member);
            return CreatedAtAction(nameof(Get), new { id = member.Id }, responseDto);
        }

        // PUT: api/MemberProfiles/5
        [HttpPost("update-member-profile")]
        public async Task<IActionResult> Update([FromBody] UpdateMemberProfileDto dto)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var member = await _context.MemberProfiles.FindAsync(dto.Id);
                if (member == null)
                    throw new Exception("Not Found");

                //_mapper.Map(dto, member);

                if (member.User.Email != dto.Email)
                {
                    await _userManager.SetEmailAsync(member.User, dto.Email);
                    await _userManager.SetUserNameAsync(member.User, dto.Email);
                }
                member.FirstName = dto.FirstName;
                member.LastName = dto.LastName;

                _context.MemberProfiles.Update(member);
                await _context.SaveChangesAsync();
                responseModel.Message = "Member Profile Updated";
                responseModel.Status = true;
                responseModel.Model = member;
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

        // DELETE: api/MemberProfiles/5
        [HttpPost("delete-member-profile")]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var member = await _context.MemberProfiles.FindAsync(id);
                if (member == null)
                    throw new Exception("Not Found");

                _context.MemberProfiles.Remove(member);
                await _context.SaveChangesAsync();
                responseModel.Message = "Member Profile Deleted";
                responseModel.Status = true;
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
        [HttpPost("get-member-stats")]
        public async Task<IActionResult> GetStats([FromBody] int memberId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                responseModel.Model = await _statsService.GetMemberStatsAsync(memberId);
                responseModel.Status = true;
                responseModel.Message = "Retrieved Data";
                return new OkObjectResult(responseModel);
            }
            catch (Exception ex)
            {
                responseModel.Status = false;
                responseModel.Message = ex.Message.ToString() + " " + ex.StackTrace.ToString();
                return new BadRequestObjectResult(responseModel);
            }
        }
        [HttpPost("members-available-sessions")]
        public async Task<IActionResult> AvailableSessions([FromBody] int memberId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                responseModel = await _sessionQueryService.GetAvailableSessionsAsync(memberId);
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
                responseModel.Status = false;
                responseModel.Message = ex.Message.ToString() + " " + ex.StackTrace.ToString();
                return new BadRequestObjectResult(responseModel);
            }
        }
        [HttpPost("member-upcoming-sessions")]
        public async Task<IActionResult> UpcomingSessions([FromBody] int memberId)
        {
            try
            {
                ResponseModel res = await _sessionQueryService.GetUpcomingSessionsAsync(memberId);
                if (res.Status)
                {
                    return new OkObjectResult(res);
                }
                else
                {
                    return new BadRequestObjectResult(res);
                }
            }
            catch (Exception ex)
            {
                ResponseModel responseModel = new ResponseModel();
                responseModel.Status = false;
                responseModel.Message = ex.Message.ToString() + " " + ex.StackTrace.ToString();
                return new BadRequestObjectResult(responseModel);
            }
        }
        [HttpPost("member-getall-sessions")]
        public async Task<IActionResult> MemberSessions([FromBody] int memberId)
        {
            try
            {
                ResponseModel res = await _sessionQueryService.GetAllSessionsAsync(memberId);
                if (res.Status)
                {
                    return new OkObjectResult(res);
                }
                else
                {
                    return new BadRequestObjectResult(res);
                }
            }
            catch (Exception ex)
            {
                ResponseModel responseModel = new ResponseModel();
                responseModel.Status = false;
                responseModel.Message = ex.Message.ToString() + " " + ex.StackTrace.ToString();
                return new BadRequestObjectResult(responseModel);
            }
        }
        [HttpPost("member-calendar")]
        public async Task<IActionResult> MemberSessions([FromBody] MemberScheduleRequestDto dto)
        {
            try
            {
                ResponseModel res = await _sessionQueryService.GetCalendarAsync(dto.MemberId, dto.Month, dto.Year);
                if (res.Status)
                {
                    return new OkObjectResult(res);
                }
                else
                {
                    return new BadRequestObjectResult(res);
                }
            }
            catch (Exception ex)
            {
                ResponseModel responseModel = new ResponseModel();
                responseModel.Status = false;
                responseModel.Message = ex.Message.ToString() + " " + ex.StackTrace.ToString();
                return new BadRequestObjectResult(responseModel);
            }
        }
        [HttpGet("leaderboard")]
        public async Task<IActionResult> Leaderboard()
        {
            try
            {
                ResponseModel res = await _leaderboardService.GetTopStreaksAsync();
                if (res.Status)
                {
                    return new OkObjectResult(res);
                }
                else
                {
                    return new BadRequestObjectResult(res);
                }
            }
            catch (Exception ex)
            {
                ResponseModel responseModel = new ResponseModel();
                responseModel.Status = false;
                responseModel.Message = ex.Message.ToString() + " " + ex.StackTrace.ToString();
                return new BadRequestObjectResult(responseModel);
            }
        }
        [HttpPost("members-activity-summary")]
        public async Task<IActionResult> GetActivitySummary([FromBody] int memberId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var result = await _sessionQueryService.GetMemberActivityAsync(memberId);
                if (result.Status)
                {
                    return new OkObjectResult(result);
                }
                else
                {
                    return new BadRequestObjectResult(result);
                }
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message.ToString();
                responseModel.Status = false;
                return new BadRequestObjectResult(responseModel);

            }
        }
    }
}
