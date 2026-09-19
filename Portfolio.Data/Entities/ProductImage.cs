namespace Portfolio.Data.Entities
{
    public class ProductImage
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public bool IsMain { get; set; }

        public int SortOrder { get; set; }


        // Navigation Property

        public Product Product { get; set; } = null!;
    }
}