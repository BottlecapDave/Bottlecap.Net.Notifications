using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IO;
using System;
using System.Collections.Generic;
using Bottlecap.Net.Notifications.Transporters.Resolvers.Emails;

namespace Bottlecap.Net.Notifications.Transporters.SendGrid
{
    /// <summary>
    /// Transporter for sending notifications via SendGrid.
    /// </summary>
    /// <typeparam name="TRecipient"></typeparam>
    public class SendGridTransporter<TRecipient> : INotificationTransporter<TRecipient>
    {
        private readonly SendGridOptions _options;
        private readonly IEmailResolver _templateContentResolver;
        private readonly ITemplateIdResolver _templateIdResolver;
        private readonly ISendGridClient _client;

        public string TransporterType {  get { return "Email"; } }

        public Transporters.INotificationRecipientResolver<TRecipient> RecipientResolver { get; private set; }

        public SendGridTransporter(SendGridOptions options, 
                                   IEmailNotificationRecipientResolver<TRecipient> recipientResolver, 
                                   IEmailResolver templateContentResolver) :
            this (new SendGridClient(options.ApiKey), options, recipientResolver, templateContentResolver)
        {
        }

        public SendGridTransporter(SendGridOptions options,
                                   IEmailNotificationRecipientResolver<TRecipient> recipientResolver,
                                   ITemplateIdResolver templateIdResolver) :
            this(new SendGridClient(options.ApiKey), options, recipientResolver, templateIdResolver)
        {
        }

        /// <summary>
        /// This constructor exists for mocking purposes only.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="options"></param>
        /// <param name="recipientResolver"></param>
        /// <param name="templateContentResolver"></param>
        /// <param name="templateIdResolver"></param>
        public SendGridTransporter(ISendGridClient client,
                                   SendGridOptions options,
                                   IEmailNotificationRecipientResolver<TRecipient> recipientResolver,
                                   IEmailResolver templateContentResolver)
            : this(client, options, recipientResolver)
        {
            if (templateContentResolver == null)
            {
                throw new ArgumentNullException(nameof(templateContentResolver));
            }
            
            _templateContentResolver = templateContentResolver;
        }

        /// <summary>
        /// This constructor exists for mocking purposes only.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="options"></param>
        /// <param name="recipientResolver"></param>
        /// <param name="templateContentResolver"></param>
        /// <param name="templateIdResolver"></param>
        public SendGridTransporter(ISendGridClient client,
                                   SendGridOptions options,
                                   IEmailNotificationRecipientResolver<TRecipient> recipientResolver,
                                   ITemplateIdResolver templateIdResolver)
            : this(client, options, recipientResolver)
        {
            if (templateIdResolver == null)
            {
                throw new ArgumentNullException(nameof(_templateIdResolver));
            }

            _templateIdResolver = templateIdResolver;
        }

        protected SendGridTransporter(ISendGridClient client,
                                      SendGridOptions options,
                                      IEmailNotificationRecipientResolver<TRecipient> recipientResolver)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            else if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            else if (recipientResolver == null)
            {
                throw new ArgumentNullException(nameof(recipientResolver));
            }

            _options = options;
            _client = client;
            RecipientResolver = recipientResolver;
        }

        public async Task<IEnumerable<string>> SendAsync(string notificationType, object recipients, object content)
        {
            var emailRecipients = recipients as EmailRecipients;
            if (emailRecipients == null)
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
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new string[0];
            }

            return new string[1] { responseBody };
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
