using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;
        private readonly IMemberStreakService _streakService;

        public AttendanceService(AppDbContext context, IMemberStreakService streakService)
        {
            _context = context;
            _streakService = streakService;
        }

        // 1️⃣ Get all participants for a session
        public async Task<List<SessionParticipantDto>> GetSessionParticipantsAsync(int sessionId)
        {
            return await _context.Bookings
                .Where(b => b.SessionId == sessionId &&
                            b.Status == BookingStatus.Confirmed)
                .Select(b => new SessionParticipantDto
                {
                    MemberId = b.MemberId,
                    FullName = b.Member.FirstName + " " + b.Member.LastName,
                    IsPresent = _context.Attendances.Any(a =>
                        a.SessionId == sessionId &&
                        a.MemberId == b.MemberId &&
                        a.Status == AttendanceStatus.Present)
                })
                .ToListAsync();
        }

        // 2️⃣ Mark attendance (bulk)
        public async Task MarkAttendanceAsync(MarkAttendanceDto dto)
        {
            var session = await _context.Sessions
                .SingleAsync(s => s.Id == dto.SessionId);

            var bookedMemberIds = await _context.Bookings
                .Where(b => b.SessionId == dto.SessionId &&
                            b.Status == BookingStatus.Confirmed)
                .Select(b => b.MemberId)
                .ToListAsync();

            foreach (var memberId in bookedMemberIds)
            {
                var attendance = await _context.Attendances
                    .SingleOrDefaultAsync(a =>
                        a.SessionId == dto.SessionId &&
                        a.MemberId == memberId);

                var newStatus = dto.PresentMemberIds.Contains(memberId)
                    ? AttendanceStatus.Present
                    : AttendanceStatus.Absent;

                // 🟢 CASE 1: Attendance does not exist
                if (attendance == null)
                {
                    attendance = new Attendance
                    {
                        SessionId = dto.SessionId,
                        MemberId = memberId,
                        Status = newStatus
                    };

                    _context.Attendances.Add(attendance);

                    if (newStatus == AttendanceStatus.Present)
                    {
                        await _streakService.UpdateMemberStreakAsync(
                            memberId,
                            session.StartTime);
                    }
                }
                // 🟡 CASE 2: Attendance exists
                else
                {
                    // Only update streak if Absent → Present
                    if (attendance.Status == AttendanceStatus.Absent &&
                        newStatus == AttendanceStatus.Present)
                    {
                        await _streakService.UpdateMemberStreakAsync(
                            memberId,
                            session.StartTime);
                    }

                    attendance.Status = newStatus;
                }
            }

            await _context.SaveChangesAsync();
        }

    }

}
