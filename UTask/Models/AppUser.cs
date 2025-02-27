﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace UTask.Models
{
    public enum UserType
    {
        Client,
        Provider,
        Admin
    }
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePicture { get; set; }
        public UserType Type { get; set; }

        // Navigation properties
        public Client ClientDetails { get; set; }
        public Provider ProviderDetails { get; set; }
        public Address Address { get; set; }
        public ICollection<ConnectionMapping> ConnectionMappings { get; set; }
    }
}
