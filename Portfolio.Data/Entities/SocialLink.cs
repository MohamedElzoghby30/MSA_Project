namespace Portfolio.Data.Entities
{
    public class SocialLink
    {
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }

        public string Platform { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string? Icon { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }


        // Navigation Property

        public Company Company { get; set; } = null!;
    }
}