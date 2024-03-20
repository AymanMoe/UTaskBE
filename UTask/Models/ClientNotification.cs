namespace UTask.Models
{
    public class ClientNotification
    {
        public int ClientId { get; set; }
        public Client Client { get; set; }

        public int NotificationId { get; set; }
        public Notification Notification { get; set; }
    }
}
