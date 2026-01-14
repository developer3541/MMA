using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class MemberStreakService : IMemberStreakService
    {
        private readonly AppDbContext _context;

        public MemberStreakService(AppDbContext context)
        {
            _context = context;
        }

        public async Task UpdateMemberStreakAsync(int memberId, DateTime sessionDate)
        {
            var date = sessionDate.Date;

            var streak = await _context.MemberStreaks
                .SingleOrDefaultAsync(x => x.MemberId == memberId);

            if (streak == null)
            {
                _context.MemberStreaks.Add(new MemberStreak
                {
                    MemberId = memberId,
                    CurrentStreak = 1,
                    BestStreak = 1,
                    LastSessionDate = date
                });
                return;
            }

            if (streak.LastSessionDate == date)
                return; // already counted for this day

            if (streak.LastSessionDate == date.AddDays(-1))
                streak.CurrentStreak++;
            else
                streak.CurrentStreak = 1;

            if (streak.CurrentStreak > streak.BestStreak)
                streak.BestStreak = streak.CurrentStreak;

            streak.LastSessionDate = date;
            streak.UpdatedAt = DateTime.UtcNow;
        }
    }

}
