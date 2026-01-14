using WebApplication1.DTOs;

namespace WebApplication1.Services
{
    public interface IAttendanceService
    {
        Task<List<SessionParticipantDto>> GetSessionParticipantsAsync(int sessionId);
        Task MarkAttendanceAsync(MarkAttendanceDto dto);
    }
}
