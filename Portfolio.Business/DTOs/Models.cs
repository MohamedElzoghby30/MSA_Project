namespace Portfolio.Business.DTOs;

public record DashboardStatsDto(int Visitors, int PageViews, int Products, int Services, int UnreadMessages, int PublishedPages);
public record TrendPointDto(string Label, int Value);
public record DashboardDto(DashboardStatsDto Stats, IReadOnlyList<TrendPointDto> VisitorTrend, IReadOnlyList<TrendPointDto> PageViewTrend, IReadOnlyList<RecentMessageDto> RecentMessages);
public record RecentMessageDto(Guid Id, string Name, string Subject, string MessageType, DateTime CreatedAt, bool IsRead);

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? Favicon { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? WorkingHours { get; set; }
    public List<CompanyPhoneDto> Phones { get; set; } = [];
    public List<SocialLinkDto> SocialLinks { get; set; } = [];
}
public class CompanyPhoneDto { public Guid Id { get; set; } public string PhoneNumber { get; set; } = ""; public string? Label { get; set; } public bool IsWhatsApp { get; set; } public bool IsActive { get; set; } = true; public int SortOrder { get; set; } }
public class SocialLinkDto { public Guid Id { get; set; } public string Platform { get; set; } = ""; public string Url { get; set; } = ""; public string? Icon { get; set; } public bool IsActive { get; set; } = true; public int SortOrder { get; set; } }

public class ProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public string? MainImage { get; set; }
    public int ImageCount { get; set; }
}
public class ProductEditDto { public Guid Id { get; set; } public string Name { get; set; } = ""; public string? Description { get; set; } public decimal Price { get; set; } public string Currency { get; set; } = "USD"; public bool IsActive { get; set; } = true; public int SortOrder { get; set; } }

public class ServiceDto { public Guid Id { get; set; } public string Title { get; set; } = ""; public string? Description { get; set; } public string? Icon { get; set; } public string? Image { get; set; } public bool IsActive { get; set; } = true; public int SortOrder { get; set; } }

public class PageListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public string Slug { get; set; } = "";
    public string? Title { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public bool IsPublished { get; set; }
    public int SectionCount { get; set; }
    public int SortOrder { get; set; }
}
public class PageEditDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? NameEn { get; set; }
    public string? NameAr { get; set; }
    public string Slug { get; set; } = "";
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
}
public class SectionStatDto
{
    public string? ValueEn { get; set; }
    public string? ValueAr { get; set; }

    public string? LabelEn { get; set; }
    public string? LabelAr { get; set; }

    // Optional icon path
    public string? Icon { get; set; }
}
public class SectionEditDto
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string SectionType { get; set; } = "Hero";

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

    public List<SectionStatDto> Stats { get; set; } = [];

    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;

    public string? Animation { get; set; }
    public int? AnimationDuration { get; set; }
    public int? AnimationDelay { get; set; }
}

public class ThemeDto
{
    public Guid Id { get; set; }

    // Shared brand controls.
    public string PrimaryColor { get; set; } = "#C88A24";
    public string SecondaryColor { get; set; } = "#08233F";
    public string AccentColor { get; set; } = "#D6A84F";
    public string SuccessColor { get; set; } = "#1FA774";
    public string WarningColor { get; set; } = "#D9A441";
    public string DangerColor { get; set; } = "#D9534F";

    // Backward-compatible aliases used by older views/services.
    public string BodyColor { get; set; } = "#FFFFFF";
    public string HeadingColor { get; set; } = "#08233F";

    // Light palette.
    public string LightBackground { get; set; } = "#F7F8FA";
    public string LightSurface { get; set; } = "#FFFFFF";
    public string LightSurfaceAlt { get; set; } = "#F1F3F6";
    public string LightText { get; set; } = "#102033";
    public string LightMutedText { get; set; } = "#66758A";
    public string LightBorder { get; set; } = "#DDE3EA";
    public string LightHeader { get; set; } = "#FFFFFF";
    public string LightFooter { get; set; } = "#0D2136";
    public string LightInput { get; set; } = "#FFFFFF";
    public string LightShadow { get; set; } = "rgba(16,32,51,.12)";

    // Dark palette / night mode.
    public string DarkBackground { get; set; } = "#061827";
    public string DarkSurface { get; set; } = "#0B243B";
    public string DarkSurfaceAlt { get; set; } = "#0F2D49";
    public string DarkText { get; set; } = "#F7FAFC";
    public string DarkMutedText { get; set; } = "#91A1B5";
    public string DarkBorder { get; set; } = "rgba(255,255,255,.09)";
    public string DarkHeader { get; set; } = "#061827";
    public string DarkFooter { get; set; } = "#03121F";
    public string DarkInput { get; set; } = "#0B253C";
    public string DarkShadow { get; set; } = "rgba(0,0,0,.35)";

    // Behavior / typography.
    public string ThemeMode { get; set; } = "system"; // system, light, dark
    public string FontFamily { get; set; } = "Inter";
    public string HeadingFont { get; set; } = "Poppins";
    public int BorderRadius { get; set; } = 14;
    public int ButtonRadius { get; set; } = 8;
    public int ContainerWidth { get; set; } = 1240;
    public bool EnableAnimations { get; set; } = true;
}

public class MessageDto { public Guid Id { get; set; } public string Name { get; set; } = ""; public string Email { get; set; } = ""; public string? Country { get; set; } public string? Phone { get; set; } public string? Subject { get; set; } public string? MessageType { get; set; } public string Message { get; set; } = ""; public DateTime CreatedAt { get; set; } public bool IsRead { get; set; } public bool IsReplied { get; set; } public bool IsArchived { get; set; } }
public class AnalyticsDto { public int UniqueVisitors { get; set; } public int PageViews { get; set; } public List<TrendPointDto> Trend { get; set; } = []; public List<(string Url,int Count)> PopularPages { get; set; } = []; }

public class UserEditDto { public Guid Id { get; set; } public string Email { get; set; } = ""; public string UserName { get; set; } = ""; public string? FullName { get; set; } public bool IsActive { get; set; } = true; public string? Password { get; set; } public string Role { get; set; } = ""; }
public class RoleEditDto { public Guid Id { get; set; } public string Name { get; set; } = ""; public string? Description { get; set; } public List<Guid> PermissionIds { get; set; } = []; }
