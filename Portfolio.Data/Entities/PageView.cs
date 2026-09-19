namespace Portfolio.Data.Entities
{
    public class PageView
    {
        public Guid Id { get; set; }

        public Guid VisitorId { get; set; }

        public Guid? PageId { get; set; }

        public string? Url { get; set; }

        public DateTime VisitedAt { get; set; }


        // Navigation Properties

        public Visitor Visitor { get; set; } = null!;

        public Page? Page { get; set; }
    }
}