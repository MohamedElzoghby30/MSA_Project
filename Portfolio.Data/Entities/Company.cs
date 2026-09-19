namespace Portfolio.Data.Entities
{
    public class Company
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Logo { get; set; }

        public string? Favicon { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? WorkingHours { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }


        // Navigation Properties

        public ICollection<CompanyPhone> Phones { get; set; }
            = new List<CompanyPhone>();

        public ICollection<SocialLink> SocialLinks { get; set; }
            = new List<SocialLink>();
    }
}