using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IO;
using System;
using System.Collections.Generic;
using Bottlecap.Net.Notifications.Transporters.Resolvers.Emails;

namespace Bottlecap.Net.Notifications.Transporters.SendGrid
{
    public class SendGridTransporter : INotificationTransporter
    {
        private readonly SendGridOptions _options;
        private readonly ITemplateContentResolver _templateContentResolver;
        private readonly ITemplateIdResolver _templateIdResolver;
        private readonly ISendGridClient _client;

        public string TransporterType {  get { return "Email"; } }

        public Transporters.INotificationRecipientResolver RecipientResolver { get; private set; }

        public SendGridTransporter(SendGridOptions options, 
                                   INotificationRecipientResolver recipientResolver, 
                                   ITemplateContentResolver templateContentResolver = null,
                                   ITemplateIdResolver templateIdResolver = null) :
            this (new SendGridClient(options.ApiKey), options, recipientResolver, templateContentResolver, templateIdResolver)
        {
        }

        public SendGridTransporter(ISendGridClient client,
                                   SendGridOptions options,
                                   INotificationRecipientResolver recipientResolver,
                                   ITemplateContentResolver templateContentResolver = null,
                                   ITemplateIdResolver templateIdResolver = null)
        {
            if (templateContentResolver == null && templateIdResolver == null)
            {
                throw new ArgumentNullException("Either a template content resolver or template id resolver must be supplied");
            }

            _options = options;
            _templateContentResolver = templateContentResolver;
            _templateIdResolver = templateIdResolver;
            _client = client;

            RecipientResolver = recipientResolver;
        }

        public async Task<bool> SendAsync(string notificationType, object recipients, object content)
        {
            var emailRecipients = recipients as EmailRecipients;
            if (recipients == null)
            {
                throw new ArgumentException("Recipients was not of type 'EmailRecipients'");
            }

            var message = new SendGridMessage()
            {
                From = new EmailAddress(_options.FromEmailAddress, _options.FromEmailName)
            };

            if (_templateContentResolver != null)
            {
                var email = await _templateContentResolver.GenerateEmailAsync(notificationType, content);
                if (email == null)
                {
                    throw new InvalidDataException($"Failed to generate email for '{notificationType}'");
                }

                message.Subject = email.Subject;
                message.HtmlContent = email.Content;
            }
            else
            {
                message.TemplateId = await _templateIdResolver.GetTemplateIdAsync(notificationType);
                if (String.IsNullOrEmpty(message.TemplateId))
                {
                    throw new InvalidDataException($"Failed to find template id for '{notificationType}'");
                }

                message.SetTemplateData(content);
            }

            AddRecipients(message.AddTo, emailRecipients.To);
            AddRecipients(message.AddCc, emailRecipients.CC);
            AddRecipients(message.AddBcc, emailRecipients.BCC);

            var response = await _client.SendEmailAsync(message);
            var responseBody = await response.Body.ReadAsStringAsync();
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }

        private void AddRecipients(Action<string, string> addAddress, IEnumerable<string> addresses)
        {
            if (addresses != null)
            {
                foreach (var address in addresses)
                {
                    if (String.IsNullOrEmpty(address) == false)
                    {
                        addAddress(address, String.Empty);
                    }
                }
            }
        }
    }
}
