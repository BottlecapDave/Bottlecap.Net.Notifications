using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IO;
using System;
using System.Collections.Generic;

namespace Bottlecap.Net.Notifications.Transporters.SendGrid
{
    public class SendGridTransporter : INotificationTransporter
    {
        private readonly SendGridOptions _options;
        private readonly ITemplateService _templateService;
        private readonly SendGridClient _client;

        public string TransporterType {  get { return "Email"; } }

        public Transporters.INotificationRecipientExtractor RecipientExtractor { get; private set; }

        public SendGridTransporter(SendGridOptions options, 
                                   INotificationRecipientExtractor recipientExtractor, 
                                   ITemplateService templateService = null)
        {
            _options = options;
            _templateService = templateService;
            _client = new SendGridClient(options.ApiKey);

            RecipientExtractor = recipientExtractor;
        }

        public async Task<bool> SendAsync(string notificationType, object recipients, object content)
        {
            var emailRecipients = recipients as EmailRecipients;
            if (recipients == null)
            {
                throw new ArgumentException("Recipients was not of type 'EmailRecipients'");
            }

            var message = new SendGridMessage();

            if (_templateService != null)
            {
                var email = await _templateService.GenerateEmailAsync(notificationType, content);
                if (email == null)
                {
                    throw new InvalidDataException($"Failed to generate email for '{notificationType}'");
                }

                message.Subject = email.Subject;
                message.HtmlContent = email.Content;
            }
            else
            {
                message.TemplateId = notificationType;
            }

            AddRecipients(message.AddTo, emailRecipients.To);
            AddRecipients(message.AddCc, emailRecipients.CC);
            AddRecipients(message.AddBcc, emailRecipients.BCC);

            var response = await _client.SendEmailAsync(message);
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        private void AddRecipients(Action<string, string> addAddress, IEnumerable<string> addresses)
        {
            if (addresses != null)
            {
                foreach (var address in addresses)
                {
                    addAddress(address, null);
                }
            }
        }
    }
}
