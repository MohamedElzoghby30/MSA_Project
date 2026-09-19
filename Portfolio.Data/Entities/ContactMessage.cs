namespace Portfolio.Data.Entities
{
    public class ContactMessage
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Country { get; set; }

        public string? Phone { get; set; }

        public string? Subject { get; set; }

        public string? MessageType { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }

        public bool IsReplied { get; set; }

        public bool IsArchived { get; set; }
    }
}