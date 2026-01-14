namespace WebApplication1.Services
{
    public interface IMemberStreakService
    {
        Task UpdateMemberStreakAsync(int memberId, DateTime sessionDate);
    }

}
