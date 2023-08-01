using BanklyEmailNotificationApplication.Model;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BanklyEmailNotificationApplication.Service
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly ILogger<EmailBackgroundService> _logger;
        private readonly IOptions<EmailSettings> _emailSettings;

        public EmailBackgroundService(ILogger<EmailBackgroundService> logger, IOptions<EmailSettings> emailSettings)
        {
            _logger = logger;
            _emailSettings = emailSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // logic to get users' onboarding data from the database
                    // For demonstration purposes, I'll use some sample data
                    var usersOnboardedViaMobileApp = GetUsersOnboardedViaMobileApp();
                    var usersOnboardedViaWebsite = GetUsersOnboardedViaWebsite();

                    // Send emails for users onboarded via mobile app
                    foreach (var user in usersOnboardedViaMobileApp)
                    {
                        await SendOnboardEmail(user.Email, user.FullName, user.PhoneNumber, user.Passcode, false);
                    }

                    // Send emails for users onboarded via website
                    foreach (var user in usersOnboardedViaWebsite)
                    {
                        await SendOnboardEmail(user.Email, user.FullName, user.PhoneNumber, user.Password, true);
                    }

                    _logger.LogInformation("Emails sent successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while sending emails.");
                }

                // Wait for 24 hours (86400 seconds) before sending emails again
                await Task.Delay(TimeSpan.FromSeconds(86400), stoppingToken);
            }
        }

        private async Task SendOnboardEmail(string toEmail, string fullName, string phoneNumber, string passcodeOrPassword, bool isWebsiteUser)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Your App", _emailSettings.Value.Username));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Welcome to Your App!";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = GetOnboardEmailHtml(fullName, phoneNumber, passcodeOrPassword, isWebsiteUser);

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.Value.SmtpServer, _emailSettings.Value.Port, _emailSettings.Value.UseSsl);
                await client.AuthenticateAsync(_emailSettings.Value.Username, _emailSettings.Value.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        // Helper method to generate email content
        private string GetOnboardEmailHtml(string fullName, string phoneNumber, string passcodeOrPassword, bool isWebsiteUser)
        {
            string loginDetails = isWebsiteUser
                ? $"<p>Username: {fullName}</p><p>Password: {passcodeOrPassword}</p>"
                : $"<p>Passcode: {passcodeOrPassword}</p>";

            return $@"
            <html>
            <body>
                <h1>Welcome to Your App!</h1>
                <p>Thank you for joining us!</p>
                <p>Here are your onboard details:</p>
                <p>Full Name: {fullName}</p>
                <p>Phone Number: {phoneNumber}</p>
                {loginDetails}
            </body>
            </html>";
        }

        // Helper method to get sample users' data 
        private IEnumerable<User> GetUsersOnboardedViaMobileApp()
        {
            return new List<User>
        {
            new User { Email = "user1@example.com", FullName = "User 1", PhoneNumber = "123456789", Passcode = "123456" },
            new User { Email = "user2@example.com", FullName = "User 2", PhoneNumber = "987654321", Passcode = "654321" }
        };
        }

        // Helper method to get sample users' data 
        private IEnumerable<User> GetUsersOnboardedViaWebsite()
        {
            return new List<User>
            {
            new User { Email = "user3@example.com", FullName = "User 3", PhoneNumber = "987654321", Password = "P@ssw0rd" },
            new User { Email = "user4@example.com", FullName = "User 4", PhoneNumber = "123456789", Password = "S3cr3t" }
            };
        }
    }
}

