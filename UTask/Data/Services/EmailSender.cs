using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using SendGrid.Helpers.Mail;
using SendGrid;
using UTask.Data.Dtos;

namespace UTask.Data.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public EmailSender(IOptions<SenderOptionsDto> optionsAccessor,
                           ILogger<EmailSender> logger,
                           IConfiguration configuration)
        {
            Options = optionsAccessor.Value;
            _logger = logger;
            _configuration = configuration; 
        }

        public SenderOptionsDto Options { get; }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            string sendGridApiKey = _configuration.GetConnectionString("TwilioAPIKey"); // Retrieve TwilioAPIKey from appsettings.json

            if (string.IsNullOrEmpty(sendGridApiKey))
            {
                throw new Exception("Null SendGridKey");
            }

            await Execute(sendGridApiKey, subject, message, toEmail);
        }

        public async Task Execute(string apiKey, string subject, string message, string toEmail)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("Ayman.khalef1999@gmail.com", "UTask Dev Environment"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(toEmail));

            msg.SetClickTracking(false, false);
            var response = await client.SendEmailAsync(msg);
            _logger.LogInformation(response.IsSuccessStatusCode
                                   ? $"Email to {toEmail} queued successfully!"
                                   : $"Failure Email to {toEmail}");
        }
    }
}
