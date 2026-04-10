namespace SYSGES_MAGs.Services.IServices
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string to, string subject, string body);
    }
}
