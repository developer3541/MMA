using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class SessionQueryService : ISessionQueryService
    {
        private readonly AppDbContext _context;
        public SessionQueryService(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }
        public async Task<ResponseModel> GetUpcomingSessionsAsync(int memberId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var now = DateTime.UtcNow;

                var res = await _context.Bookings
                    .Where(b => b.MemberId == memberId &&
                                b.Status == BookingStatus.Confirmed &&
                                b.Session.StartTime > now)
                    .Select(b => new UpcomingSessionDto
                    {
                        SessionId = b.Session.Id,
                        SessionName = b.Session.SessionName,
                        StartTime = b.Session.StartTime,
                        EndTime = b.Session.EndTime,
                        CoachName = b.Session.Coach.User.FirstName + " " + b.Session.Coach.User.LastName,
                        ClassType = b.Session.ClassType.Name
                    })
                    .OrderBy(x => x.StartTime)
                    .ToListAsync();
                responseModel.Model = res;
                responseModel.Status = true;
                responseModel.Message = "Retrieved";
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message.ToString() + " " + ex.StackTrace.ToString();
                responseModel.Status = false;

            }
            return responseModel;
        }
        public async Task<ResponseModel> GetAllSessionsAsync(int memberId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var now = DateTime.UtcNow;

                var res = await _context.Bookings
                    .Where(b => b.MemberId == memberId)
                    .Select(b => new UpcomingSessionDto
                    {
                        SessionId = b.Session.Id,
                        SessionName = b.Session.SessionName,
                        StartTime = b.Session.StartTime,
                        EndTime = b.Session.EndTime,
                        CoachName = b.Session.Coach.User.FirstName + " " + b.Session.Coach.User.LastName,
                        ClassType = b.Session.ClassType.Name
                    })
                    .OrderBy(x => x.StartTime)
                    .ToListAsync();
                responseModel.Model = res;
                responseModel.Status = true;
                responseModel.Message = "Retrieved";
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message.ToString() + " " + ex.StackTrace.ToString();
                responseModel.Status = false;

            }
            return responseModel;
        }
        public async Task<ResponseModel> GetCalendarAsync(
        int memberId,
        int month,
        int year)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var monthStart = new DateTime(year, month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var now = DateTime.UtcNow;

                var sessions = await _context.Bookings
                    .Where(b =>
                        b.MemberId == memberId &&
                        b.Session.StartTime >= monthStart &&
                        b.Session.StartTime < monthEnd)
                    .Select(b => new
                    {
                        b.SessionId,
                        b.Status,
                        b.Session.StartTime,
                        b.Session.EndTime,
                        b.Session.SessionName,
                        ClassType = b.Session.ClassType.Name,
                        CoachName = b.Session.Coach.User.FirstName + " " +
                                    b.Session.Coach.User.LastName,
                        Attendance = b.Session.Attendances
                            .Where(a => a.MemberId == memberId)
                            .Select(a => (AttendanceStatus?)a.Status)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                var res = sessions
                    .GroupBy(s => s.StartTime.Date)
                    .Select(g => new MemberScheduleDayDto
                    {
                        Date = g.Key,
                        Sessions = g.Select(s => new MemberScheduleItemDto
                        {
                            SessionId = s.SessionId,
                            Date = s.StartTime.Date,
                            StartTime = s.StartTime.TimeOfDay,
                            EndTime = s.EndTime.TimeOfDay,
                            SessionName = s.SessionName,
                            ClassType = s.ClassType,
                            CoachName = s.CoachName,
                            BookingStatus = s.Status,
                            AttendanceStatus = s.Attendance,
                            SessionState = s.StartTime > now ? "Upcoming" : "Completed"
                        }).OrderBy(x => x.StartTime).ToList()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
                responseModel.Model = res;
                responseModel.Status = true;
                responseModel.Message = "Retrieved";
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message.ToString() + " " + ex.StackTrace.ToString();
                responseModel.Status = false;

            }
            return responseModel;
        }
        public async Task<ResponseModel> GetAvailableSessionsAsync(int memberId)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                var now = DateTime.UtcNow;

                var res = await _context.Sessions
                    .Where(s => s.StartTime > now &&
                                s.Bookings.Count(b => b.Status == BookingStatus.Confirmed) < s.Capacity &&
                                !s.Bookings.Any(b => b.MemberId == memberId))
                    .Select(s => new UpcomingSessionDto
                    {
                        SessionId = s.Id,
                        SessionName = s.SessionName,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        CoachName = s.Coach.User.FirstName,
                        ClassType = s.ClassType.Name
                    })
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();
                responseModel.Model = res;
                responseModel.Status = true;
                responseModel.Message = "Retrieved";
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message.ToString() + " " + ex.StackTrace.ToString();
                responseModel.Status = false;

            }
            return responseModel;
        }


    }
}
