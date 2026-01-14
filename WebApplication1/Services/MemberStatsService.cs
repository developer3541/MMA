using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class MemberStatsService : IMemberStatsService
    {
        private readonly AppDbContext _context;

        public MemberStatsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MemberStatsDto> GetMemberStatsAsync(int memberId)
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var streak = await _context.MemberStreaks
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.MemberId == memberId);

            var totalAttended = await _context.Attendances
                .CountAsync(a => a.MemberId == memberId &&
                                 a.Status == AttendanceStatus.Present);

            var monthlyAttended = await _context.Attendances
                .Include(a => a.Session)
                .CountAsync(a => a.MemberId == memberId &&
                                 a.Status == AttendanceStatus.Present &&
                                 a.Session.StartTime >= monthStart);

            return new MemberStatsDto
            {
                CurrentStreak = streak?.CurrentStreak ?? 0,
                BestStreak = streak?.BestStreak ?? 0,
                TotalSessionsAttended = totalAttended,
                SessionsThisMonth = monthlyAttended
            };
        }
    }

}
