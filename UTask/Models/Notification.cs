using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UTask.Models
{
    public struct NotificationType
    {
        public const string Booking = "Booking";
        public const string Review = "Review";
        public const string Rating = "Rating";
        public const string Invoice = "Invoice";
        public const string Reminder = "Reminder";
        public const string Alert = "Alert";
    }
    public class Notification
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Data { get; set; }
        public string? Action { get; set; }
        public string? Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [JsonIgnore]
        public ICollection<ClientNotification> ClientNotifications { get; set; }
        [JsonIgnore]
        public ICollection<ProviderNotification> ProviderNotifications { get; set; }
       
    }
}
