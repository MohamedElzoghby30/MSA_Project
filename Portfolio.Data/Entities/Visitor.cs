namespace Portfolio.Data.Entities
{
    public class Visitor
    {
        public Guid Id { get; set; }

        public string VisitorKey { get; set; } = string.Empty;

        public string? UserAgent { get; set; }

        public string? Referrer { get; set; }

        public DateTime FirstVisitAt { get; set; }

        public DateTime LastVisitAt { get; set; }


        // Navigation Property

        public ICollection<PageView> PageViews { get; set; }
            = new List<PageView>();
    }
}