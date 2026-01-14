using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly AppDbContext _context;
        public LeaderboardService(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }
        public async Task<ResponseModel> GetTopStreaksAsync(int top = 10)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {

                var res = await _context.MemberStreaks
                                .OrderByDescending(s => s.BestStreak)
                                .ThenByDescending(s => s.CurrentStreak)
                                .Take(top)
                                .Select(s => new LeaderboardDto
                                {
                                    MemberId = s.MemberId,
                                    MemberName = s.Member.User.FirstName + " " + s.Member.User.LastName,
                                    CurrentStreak = s.CurrentStreak,
                                    BestStreak = s.BestStreak,
                                    TotalSessions = _context.Attendances
                                        .Count(a => a.MemberId == s.MemberId &&
                                                    a.Status == AttendanceStatus.Present)
                                })
                                .ToListAsync();
                responseModel.Message = "Data Retrieved";
                responseModel.Status = true;
                responseModel.Model = res;
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
