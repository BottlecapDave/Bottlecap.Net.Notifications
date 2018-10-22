using System;
using Bottlecap.Net.Notifications.Data;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Bottlecap.Net.Notifications.EF
{
    public class NotificationData : INotificationData
    {
        public long Id { get; set; }

        public string NotificationType { get; set; }

        public string TransportType { get; set; }

        [NotMapped]
        public object Recipients { get; set; }

        [Column("Recipients")]
        [JsonIgnore]
        public string RecipientsAsJson
        {
            get
            {
                return JsonConvert.SerializeObject(this.Recipients);
            }
            set
            {
                this.Recipients = JsonConvert.DeserializeObject(value);
            }
        }

        [NotMapped]
        public object Content { get; set; }

        [Column("Content")]
        [JsonIgnore]
        public string ContentAsJson
        {
            get
            {
                return JsonConvert.SerializeObject(this.Content);
            }
            set
            {
                this.Content = JsonConvert.DeserializeObject(value);
            }
        }

        [Column(TypeName = "string")]
        public NotificationState State { get; set; }

        public int RetryCount { get; set; }

        public DateTime? NextExecutionTimestamp { get; set; }

        public DateTime CreationTimestamp { get; set; }

        public DateTime? LastUpdatedTimestamp { get; set; }
    }
}