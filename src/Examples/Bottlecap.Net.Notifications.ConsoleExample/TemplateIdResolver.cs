using Bottlecap.Net.Notifications.Transporters.SendGrid;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.ConsoleExample
{
    public class TemplateIdResolver : ITemplateIdResolver
    {
        private readonly string _templateId;
        public TemplateIdResolver(string templateId)
        {
            _templateId = templateId;
        }

        public Task<string> GetTemplateIdAsync(string notificationType)
        {
            return Task.FromResult(_templateId);
        }
    }
}
