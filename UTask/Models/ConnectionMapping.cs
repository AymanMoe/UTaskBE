﻿using System.ComponentModel.DataAnnotations;

namespace UTask.Models
{
    public class ConnectionMapping
    {
        [Key]
        public string ConnectionId { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
