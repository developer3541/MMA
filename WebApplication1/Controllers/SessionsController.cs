using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop.Infrastructure;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public SessionsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var sessions = await _context.Sessions
    .Select(s => new SessionListDto
    {
        Id = s.Id,
        CoachId = s.CoachId,
        CoachName = s.Coach.User.FirstName + " " + s.Coach.User.LastName,
        ClassTypeId = s.ClassTypeId,
        ClassTypeName = s.ClassType.Name,
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        Capacity = s.Capacity,
        Description = s.Description,
        SessionName = s.SessionName,
        BookingsCount = s.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
        AttendanceCount = s.Attendances.Count(a => a.Status == AttendanceStatus.Present)
    })
    .ToListAsync();

                responseModel.Status = true;
                responseModel.Message = "Data Fetched";
                responseModel.Model = sessions;
                //responseModel.Model = _mapper.Map<List<SessionResponseDto>>(sessions);
                //return _mapper.Map<List<SessionResponseDto>>(sessions);
                return new OkObjectResult(responseModel);

            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message;
                responseModel.Status = false;

                return new BadRequestObjectResult(responseModel);
            }
        }

        [HttpPost("get-single-session")]
        public async Task<IActionResult> Get([FromBody] int id)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var sessions = await _context.Sessions
                .Select(s => new SessionListDto
                {
                    Id = s.Id,
                    CoachId = s.CoachId,
                    CoachName = s.Coach.User.FirstName + " " + s.Coach.User.LastName,
                    ClassTypeId = s.ClassTypeId,
                    ClassTypeName = s.ClassType.Name,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Capacity = s.Capacity,
                    Description = s.Description,
                    SessionName = s.SessionName,
                    BookingsCount = s.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
                    AttendanceCount = s.Attendances.Count(a => a.Status == AttendanceStatus.Present)
                })
              .FirstOrDefaultAsync(s => s.Id == id);

                if (sessions == null) throw new Exception("Not Found");
                responseModel.Status = true;
                responseModel.Message = "Data Fetched";
                responseModel.Model = sessions;
                //return _mapper.Map<SessionResponseDto>(session);
                return new OkObjectResult(responseModel);

            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message;
                responseModel.Status = false;

                return new BadRequestObjectResult(responseModel);
            }
        }

        [HttpPost("create-new-session")]
        public async Task<IActionResult> Create(CreateSessionDto dto)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var session = _mapper.Map<Session>(dto);
                _context.Sessions.Add(session);
                await _context.SaveChangesAsync();
                responseModel.Message = "Session Created";
                responseModel.Status = true;
                var sessions = await _context.Sessions
             .Select(s => new SessionListDto
             {
                 Id = s.Id,
                 CoachId = s.CoachId,
                 CoachName = s.Coach.User.FirstName + " " + s.Coach.User.LastName,
                 ClassTypeId = s.ClassTypeId,
                 ClassTypeName = s.ClassType.Name,
                 StartTime = s.StartTime,
                 EndTime = s.EndTime,
                 Capacity = s.Capacity,
                 Description = s.Description,
                 SessionName = s.SessionName,
                 BookingsCount = s.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
                 AttendanceCount = s.Attendances.Count(a => a.Status == AttendanceStatus.Present)
             })
           .FirstOrDefaultAsync(s => s.Id == session.Id);
                responseModel.Model = sessions;
                return new OkObjectResult(responseModel);
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message.ToString();
                responseModel.Status = false;
                responseModel.Model = null;
                return new BadRequestObjectResult(responseModel);
            }
            //return CreatedAtAction(nameof(Get), new { id = session.Id }, _mapper.Map<SessionResponseDto>(session));
        }

        [HttpPost("update-session")]
        public async Task<IActionResult> Update([FromBody] UpdateSessionDto dto)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                var session = await _context.Sessions.FindAsync(dto.Id);
                if (session == null) return NotFound();

                _mapper.Map(dto, session);
                await _context.SaveChangesAsync();
                responseModel.Message = "Session Updated";
                responseModel.Status = true;
                responseModel.Model = session;
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

        [HttpPost("delete-session")]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                var session = await _context.Sessions.FindAsync(id);
                if (session == null) throw new Exception("Not Found");

                _context.Sessions.Remove(session);
                await _context.SaveChangesAsync();
                responseModel.Message = "Session Deleted";
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
        [HttpPost("get-coach-sessions")]

        public async Task<IActionResult> GetCoachSessionsAsync([FromBody] int coachId)
        {
            ResponseModel responseModel = new ResponseModel();

            try
            {
                var session = await _context.Sessions
                .Where(s => s.CoachId == coachId)
                .OrderByDescending(s => s.StartTime)
                .Select(s => new CoachSessionDto
                {
                    Id = s.Id,
                    SessionName = s.SessionName,
                    ClassTypeName = s.ClassType.Name,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Capacity = s.Capacity,
                    BookingsCount = s.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
                    AttendanceCount = s.Attendances.Count(a => a.Status == AttendanceStatus.Present)
                })
                .ToListAsync();
                responseModel.Message = "Session Updated";
                responseModel.Status = true;
                responseModel.Model = session;
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

    }
}
