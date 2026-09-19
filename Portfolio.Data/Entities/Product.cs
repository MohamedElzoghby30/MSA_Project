namespace Portfolio.Data.Entities
{
    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; } = "USD";

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }


        // Navigation Property

        public ICollection<ProductImage> Images { get; set; }
            = new List<ProductImage>();
    }
}