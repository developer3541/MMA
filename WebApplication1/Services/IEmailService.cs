namespace WebApplication1.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }

}
