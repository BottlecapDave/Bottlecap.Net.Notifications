using System.Collections.Generic;

namespace Bottlecap.Net.Notifications.Transporters.Extractors.Emails
{
    public class EmailRecipients
    {
        public IEnumerable<string> To { get; set; }
        public IEnumerable<string> CC { get; set; }
        public IEnumerable<string> BCC { get; set; }
    }
}