namespace Portfolio.Data.Entities
{
    public class Page
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Bilingual content. Name remains the legacy/English fallback value.
        public string? NameEn { get; set; }
        public string? NameAr { get; set; }

        public string Slug { get; set; } = string.Empty;

        public string? Title { get; set; }

        public string? TitleEn { get; set; }
        public string? TitleAr { get; set; }

        public string? MetaTitle { get; set; }

        public string? MetaTitleEn { get; set; }
        public string? MetaTitleAr { get; set; }

        public string? MetaDescription { get; set; }

        public string? MetaDescriptionEn { get; set; }
        public string? MetaDescriptionAr { get; set; }

        public bool IsPublished { get; set; } = true;

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }


        // Navigation Property

        public ICollection<Section> Sections { get; set; }
            = new List<Section>();
    }
}