namespace Portfolio.Data.Entities
{
    public class ThemeSetting
    {
        public Guid Id { get; set; }

        public string? PrimaryColor { get; set; }

        public string? SecondaryColor { get; set; }

        public string? AccentColor { get; set; }

        public string? BodyColor { get; set; }

        public string? HeadingColor { get; set; }

        public string? FontFamily { get; set; }

        public string? HeadingFont { get; set; }

        public int BorderRadius { get; set; } = 8;

        public int ButtonRadius { get; set; } = 6;

        public int ContainerWidth { get; set; } = 1200;

        public bool EnableAnimations { get; set; } = true;
    }
}