namespace Common.Services.Services.Interface
{
    public interface IEmailService
    {
        Task SendForgotPasswordEmailAsync(string email, string userName, string resetLink);
        Task SendWelcomeEmailAsync(string email, string firstName);
        Task SendVerificationEmailAsync(string email, string verificationLink);
        Task SendInterviewLinkAsync(string email, string verificationLink);
        Task SendUserWelcomeEmailAsync(string email, string firstName, string password);
    }
}
