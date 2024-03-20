namespace UTask.Models
{
    public class ProviderNotification
    {
        public int ProviderId { get; set; }
        public Provider Provider { get; set; }

        public int NotificationId { get; set; }
        public Notification Notification { get; set; }
    }
}
