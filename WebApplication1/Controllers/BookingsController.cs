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
                    .ToListAsync();
                responseModel.Status = true;
                responseModel.Message = "Data Fetched";
                responseModel.Model = _mapper.Map<List<BookingResponseDto>>(bookings);
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
            ResponseModel responseModel = new ResponseModel();

            try
            {
                var booking = _mapper.Map<Booking>(dto);
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                responseModel.Status = true;
                responseModel.Message = "Booking Created";
                responseModel.Model = booking;
                return new OkObjectResult(responseModel);

            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message;
                responseModel.Status = false;

                return new BadRequestObjectResult(responseModel);
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

        public async Task<ResponseModel> UpdateMemberStreakAsync([FromBody] int memberId)
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
    }
}
