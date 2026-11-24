using System;

namespace ThirdSpace.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }

        public string SenderId { get; set; } = string.Empty;
        public User Sender { get; set; } = null!;

        public string ReceiverId { get; set; } = string.Empty;
        public User Receiver { get; set; } = null!;

        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}