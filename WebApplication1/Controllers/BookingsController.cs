using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BookingsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet("get-all-bookings")]
        public async Task<IActionResult> GetAll()
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var bookings = await _context.Bookings
                 .Include(b => b.Member)
                   .ThenInclude(m => m.User)
                    .Include(b => b.Session)
                    .ThenInclude(s => s.Coach)
                    .ThenInclude(c => c.User)
                    .Include(b => b.Session)
                     .ThenInclude(s => s.ClassType)
                      .Include(b => b.Session)
                      .ThenInclude(s => s.Bookings)
                          .Include(b => b.Session)
                          .ThenInclude(s => s.Attendances)
                         .ToListAsync();
                var now = DateTime.UtcNow;

                var result = bookings.Select(b => new BookingResponseDto
                {
                    BookingId = b.Id,
                    BookingStatus = b.Status.ToString(),
                    BookingTime = b.BookingTime,

                    MemberId = b.MemberId,
                    MemberName = $"{b.Member.User.FirstName} {b.Member.User.LastName}",

                    Session = new SessionDto
                    {
                        Id = b.Session.Id,
                        Title = b.Session.SessionName,
                        StartTime = b.Session.StartTime,
                        EndTime = b.Session.EndTime,

                        EnrolledCount = b.Session.Bookings.Count(x =>
                            x.Status == BookingStatus.Confirmed ||
                            x.Status == BookingStatus.Attended),

                        TotalSpots = b.Session.Capacity,

                        Description = b.Session.Description,
                        WhatToBring = b.Session.WhattoBring,

                        CoachName = $"{b.Session.Coach.User.FirstName} {b.Session.Coach.User.LastName}",
                        ClassTypeName = b.Session.ClassType.Name,

                        Status = GetSessionStatus(b.Session, b.MemberId, now).ToString()
                    }
                }).ToList();

                responseModel.Status = true;
                responseModel.Message = "Data Fetched";
                responseModel.Model = result;
                //responseModel.Model = _mapper.Map<List<BookingResponseDto>>(bookings);
                return new OkObjectResult(responseModel);

            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message;
                responseModel.Status = false;

                return new BadRequestObjectResult(responseModel);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> Get(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Member)
                    .ThenInclude(m => m.User)
                .Include(b => b.Session)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();
            return _mapper.Map<BookingResponseDto>(booking);
        }

        [HttpPost("create-booking")]
        public async Task<IActionResult> Create(CreateBookingDto dto)
        {
            var response = new ResponseModel();

            try
            {
                var session = await _context.Sessions
                    .Include(s => s.Bookings)
                    .FirstOrDefaultAsync(s => s.Id == dto.SessionId);

                if (session == null)
                    return BadRequest(new ResponseModel { Status = false, Message = "Session not found" });

                var now = DateTime.UtcNow;
                if (session.StartTime <= now)
                    return BadRequest(new ResponseModel { Status = false, Message = "Cannot book an ongoing or completed session" });

                var alreadyBooked = session.Bookings.Any(b =>
                    b.MemberId == dto.MemberId &&
                    b.Status != BookingStatus.Cancelled);

                if (alreadyBooked)
                    return BadRequest(new ResponseModel { Status = false, Message = "Member is already enrolled in this session" });

                var confirmedCount = session.Bookings.Count(b =>
                    b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended);

                if (confirmedCount >= session.Capacity)
                    return BadRequest(new ResponseModel { Status = false, Message = "Session is full" });

                var booking = new Booking
                {
                    SessionId = dto.SessionId,
                    MemberId = dto.MemberId,
                    BookingTime = DateTime.UtcNow,
                    Status = BookingStatus.Confirmed
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // ✅ Map to DTO to avoid circular reference
                var bookingDto = new CreateBookingResponseDto
                {
                    BookingId = booking.Id,
                    MemberId = booking.MemberId,
                    MemberName = $"{(await _context.MemberProfiles
                           .Where(m => m.Id == booking.MemberId)
                           .Select(m => m.User.FirstName + " " + m.User.LastName)
                           .FirstOrDefaultAsync())}",
                    SessionId = booking.SessionId,
                    SessionName = (await _context.Sessions
                        .Where(s => s.Id == booking.SessionId)
                        .Select(s => s.SessionName)
                        .FirstOrDefaultAsync()),
                    BookingTime = booking.BookingTime,
                    BookingStatus = booking.Status.ToString()
                };



                response.Status = true;
                response.Message = "Booking created successfully";
                response.Model = bookingDto;

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateBookingDto dto)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            _mapper.Map(dto, booking);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPost("update-member-streak")]

        public async Task<ResponseModel> UpdateMemberStreakAsync(int memberId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var today = DateTime.UtcNow.Date;

                var streak = await _context.MemberStreaks
                    .SingleOrDefaultAsync(x => x.MemberId == memberId);

                if (streak == null)
                {
                    streak = new MemberStreak
                    {
                        MemberId = memberId,
                        CurrentStreak = 1,
                        BestStreak = 1,
                        LastSessionDate = today
                    };

                    _context.MemberStreaks.Add(streak);
                }
                else
                {
                    if (streak.LastSessionDate == today)
                        return null;

                    if (streak.LastSessionDate == today.AddDays(-1))
                        streak.CurrentStreak++;
                    else
                        streak.CurrentStreak = 1;

                    if (streak.CurrentStreak > streak.BestStreak)
                        streak.BestStreak = streak.CurrentStreak;

                    streak.LastSessionDate = today;
                    streak.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                responseModel.Status = true;
                responseModel.Message = "Streak Updated";
                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message;
                responseModel.Status = false;
                return responseModel;
            }
        }
        [HttpGet("get-member-bookings")]
        public async Task<IActionResult> GetMemberBookings(int memberId)
        {
            var now = DateTime.UtcNow;

            var bookings = await _context.Bookings
                .Where(b => b.MemberId == memberId)
                .Include(b => b.Member)
                    .ThenInclude(m => m.User)
                .Include(b => b.Session)
                    .ThenInclude(s => s.Coach)
                        .ThenInclude(c => c.User)
                .Include(b => b.Session)
                    .ThenInclude(s => s.ClassType)
                .Include(b => b.Session)
                    .ThenInclude(s => s.Bookings)
                .ToListAsync();

            var result = bookings.Select(b => new BookingResponseDto
            {
                BookingId = b.Id,
                BookingStatus = b.Status.ToString(),
                BookingTime = b.BookingTime,

                MemberId = b.MemberId,
                MemberName = $"{b.Member.User.FirstName} {b.Member.User.LastName}",

                Session = new SessionDto
                {
                    Id = b.Session.Id,
                    Title = b.Session.SessionName,
                    StartTime = b.Session.StartTime,
                    EndTime = b.Session.EndTime,

                    EnrolledCount = b.Session.Bookings.Count(x =>
                        x.Status == BookingStatus.Confirmed ||
                        x.Status == BookingStatus.Attended),

                    TotalSpots = b.Session.Capacity,

                    Description = b.Session.Description,
                    WhatToBring = b.Session.WhattoBring,

                    CoachName = $"{b.Session.Coach.User.FirstName} {b.Session.Coach.User.LastName}",
                    ClassTypeName = b.Session.ClassType.Name,

                    Status = GetSessionStatus(b.Session, memberId, now).ToString()
                }
            }).ToList();

            return Ok(new
            {
                status = true,
                message = "Member bookings fetched",
                model = result
            });
        }

        [NonAction]
        private static SessionStatus GetSessionStatus(
    Session session,
    int memberId,
    DateTime now)
        {
            if (session.StartTime > now)
                return SessionStatus.Upcoming;

            if (session.StartTime <= now && session.EndTime >= now)
                return SessionStatus.InProgress;

            // Session ended → check booking status
            var booking = session.Bookings
                .FirstOrDefault(b => b.MemberId == memberId);

            if (booking == null)
                return SessionStatus.Missed;

            return booking.Status == BookingStatus.Attended
                ? SessionStatus.Completed
                : SessionStatus.Missed;
        }

    }
}
