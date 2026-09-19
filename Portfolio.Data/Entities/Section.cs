namespace Portfolio.Data.Entities
{
    public class Section
    {
        public Guid Id { get; set; }

        public Guid PageId { get; set; }

        public string SectionType { get; set; } = string.Empty;

        public string? Title { get; set; }

        public string? TitleEn { get; set; }
        public string? TitleAr { get; set; }

        public string? Subtitle { get; set; }

        public string? SubtitleEn { get; set; }
        public string? SubtitleAr { get; set; }

        public string? Description { get; set; }

        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }

        public string? Image { get; set; }

        public string? BackgroundImage { get; set; }

        public string? BackgroundColor { get; set; }

        public string? TextColor { get; set; }

        public string? SettingsJson { get; set; }

        public int SortOrder { get; set; }

        public bool IsVisible { get; set; } = true;

        public string? Animation { get; set; }

        public int? AnimationDuration { get; set; }

        public int? AnimationDelay { get; set; }


        // Navigation Property

        public Page Page { get; set; } = null!;
    }
}