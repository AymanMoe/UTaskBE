namespace UTask.Data.Dtos
{
    public class NotificationDto
    {
        public int? Id { get; set; }    
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Body { get; set; }
        public object Data { get; set; }
        public string? Action { get; set; }
        public int? ClientId { get; set; }
        public int? ProviderId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
