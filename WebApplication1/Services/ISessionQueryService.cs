using WebApplication1.DTOs;

namespace WebApplication1.Services
{
    public interface ISessionQueryService
    {
        Task<ResponseModel> GetUpcomingSessionsAsync(int memberId);
        Task<ResponseModel> GetAvailableSessionsAsync(int memberId);
        Task<ResponseModel> GetAllSessionsAsync(int memberId);
        Task<ResponseModel> GetCalendarAsync(
        int memberId,
        int month,
        int year);

    }

}
