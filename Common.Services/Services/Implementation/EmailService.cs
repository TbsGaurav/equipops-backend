using Common.Services.Services.Interface;


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Net;
using System.Net.Mail;

namespace Common.Services.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _config;

        public EmailService(ILogger<EmailService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPortString = _config["Smtp:Port"];
            var smtpUser = _config["Smtp:User"];
            var smtpPass = _config["Smtp:Password"];
            var fromMail = _config["Smtp:From"];
            var enableSslString = _config["Smtp:EnableSsl"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPortString) ||
                string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass) || string.IsNullOrEmpty(fromMail))
            {
                _logger.LogError("SMTP configuration is missing.");
                return;
            }

            if (!int.TryParse(smtpPortString, out int smtpPort))
            {
                _logger.LogError("Invalid SMTP port: {Port}", smtpPortString);
                return;
            }

            bool enableSsl = bool.TryParse(enableSslString, out var ssl) && ssl;

            using var client = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromMail),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);

            _logger.LogInformation("{Subject} email sent to {Email}", subject, toEmail);
        }

        public async Task SendForgotPasswordEmailAsync(string email, string userName, string resetLink)
        {
            var subject = "🔐 Reset Your Password – AI Recruiter Portal";

            var body = $@"
<table width='100%' cellspacing='0' cellpadding='0' style='font-family: Arial, sans-serif;'>
    <tr>
        <td style='padding: 20px; background-color: #f7f7f7;'>
            <table width='600' align='center' cellpadding='0' cellspacing='0' style='background: #ffffff; border-radius: 8px; padding: 20px;'>
                <tr>
                    <td style='font-size: 18px; font-weight: bold; color: #333;'>Hi {userName},</td>
                </tr>
                <tr>
                    <td style='padding-top: 15px; font-size: 15px; color: #555;'>
                        You recently requested to reset your password for your AI Recruiter Portal account.
                        Click the button below to reset it.
                    </td>
                </tr>

                <tr>
                    <td style='padding-top: 25px; text-align: center;'>
                        <a href='{resetLink}' 
                           style='background-color: #4f46e5;
                                  padding: 12px 20px;
                                  border-radius: 6px;
                                  color: #ffffff;
                                  text-decoration: none;
                                  font-size: 16px;
                                  font-weight: bold;'>
                            Reset Password
                        </a>
                    </td>
                </tr>

                <tr>
                    <td style='padding-top: 20px; font-size: 14px; color: #777;'>
                        If the button doesn’t work, copy and paste this link into your browser:<br/>
                        <span style='color: #4f46e5;'>{resetLink}</span>
                    </td>
                </tr>

                <tr>
                    <td style='padding-top: 15px; font-size: 14px; color: #777;'>
                        This link is valid for <strong>15 minutes</strong>.  
                        If you didn’t request a password reset, you can safely ignore this email.
                    </td>
                </tr>

                <tr>
                    <td style='padding-top: 30px; font-size: 15px; color: #555;'>
                        Best Regards,<br/>
                        <strong>Team Support</strong><br/>
                        AI Recruiter Portal
                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table>
";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName)
        {
            var subject = "🎉 Welcome to Our Platform!";
            var body = $@"
				<p>Hi {firstName},</p>
				<p>Welcome to AI Recruiter Portal!</p>
				<p>Your organization has been created successfully.</p>
				<p>You can now log in using:<br/>Email: {email}</p>
				<p>We're happy to have you onboard!</p>
				<br/>Best Regards,<br/>Team Support
			";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendVerificationEmailAsync(string email, string verificationLink)
        {
            var subject = "Verify your email address";
            var body = $@"
				<h2>AI Recruiter Portal!</h2>
				<p>Thank you for registering. Please verify your email by clicking the link below:</p>
				<p><a href='{verificationLink}' target='_blank'>Verify Your Email</a></p>
				<p>This link will expire in 24 hours.</p>
				<br/>Warm Regards,<br/>AI Recruiter Team
			";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendUserWelcomeEmailAsync(string email, string firstName, string password)
        {
            var subject = "Your AI Recruiter Account Password";
            var body = $@"
			<p>Hi {firstName},</p>
			<p>Welcome to AI Recruiter Portal!</p>

			<p><b>Login Credentials:</b></p>
			<ul>
			  <li>Email: <b>{email}</b></li>
			  <li>Password: <b>{password}</b></li>
			</ul>

			<p>Please change your password after logging in.</p>
			<br>Best Regards,<br>Team Support
			";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendInterviewLinkAsync(string email, string verificationLink)
        {
            var subject = "Your Interview Link";
            var body = $@"
				<h2>AI Recruiter Portal!</h2>
				<p>Thank you for intrest. Your are shortlisted for the first round. Please join interview by clicking the link below:</p>
				<p><a href='{verificationLink}' target='_blank'>Join Interview</a></p>
				<p>This link will expire in 24 hours.</p>
				<br/>Warm Regards,<br/>AI Recruiter Team
			";

            await SendEmailAsync(email, subject, body, true);
        }
    }
}
