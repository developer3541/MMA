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
