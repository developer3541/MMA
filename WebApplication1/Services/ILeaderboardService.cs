using WebApplication1.DTOs;

namespace WebApplication1.Services
{
    public interface ILeaderboardService
    {
        Task<ResponseModel> GetTopStreaksAsync(int top = 10);
    }
}
