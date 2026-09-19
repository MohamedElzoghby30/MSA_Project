namespace Portfolio.Data.Entities
{
    public class WebsiteSetting
    {
        public Guid Id { get; set; }

        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}