namespace Portfolio.Data.Entities
{
    public class CompanyPhone
    {
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Label { get; set; }

        public bool IsWhatsApp { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }


        // Navigation Property

        public Company Company { get; set; } = null!;
    }
}