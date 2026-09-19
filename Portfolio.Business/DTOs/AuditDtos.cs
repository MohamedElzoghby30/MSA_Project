namespace Portfolio.Business.DTOs;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = "Anonymous";
    public string ActionType { get; set; } = string.Empty;
    public string Area { get; set; } = "Admin";
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Path { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Details { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}

public class ProductImagesDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public List<ProductImageDto> Images { get; set; } = [];
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }
}
