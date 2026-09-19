using Portfolio.Business.DTOs;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Portfolio.Business.Interfaces;

public interface IDashboardService { Task<DashboardDto> GetAsync(int days = 30, CancellationToken ct = default); }
public interface ICompanyService { Task<CompanyDto?> GetAsync(CancellationToken ct = default); Task<CompanyDto> SaveAsync(CompanyDto dto, IFormFile? logo, IFormFile? favicon, CancellationToken ct = default); Task DeletePhoneAsync(Guid id, CancellationToken ct = default); Task SavePhoneAsync(Guid companyId, CompanyPhoneDto dto, CancellationToken ct = default); Task DeleteSocialAsync(Guid id, CancellationToken ct = default); Task SaveSocialAsync(Guid companyId, SocialLinkDto dto, CancellationToken ct = default); }
public interface IProductService { Task<List<ProductListDto>> ListAsync(CancellationToken ct = default); Task<ProductEditDto?> GetAsync(Guid id, CancellationToken ct = default); Task<ProductImagesDto?> GetImagesAsync(Guid id, CancellationToken ct = default); Task<ProductEditDto> SaveAsync(ProductEditDto dto, CancellationToken ct = default); Task DeleteAsync(Guid id, CancellationToken ct = default); Task<ProductImageUploadResult> AddImageAsync(Guid productId, IFormFile file, CancellationToken ct = default); Task DeleteImageAsync(Guid imageId, CancellationToken ct = default); Task SetMainImageAsync(Guid imageId, CancellationToken ct = default); }
public record ProductImageUploadResult(bool Success, string? Error, string? Path);
public interface IServiceService { Task<List<ServiceDto>> ListAsync(CancellationToken ct = default); Task<ServiceDto?> GetAsync(Guid id, CancellationToken ct = default); Task<ServiceDto> SaveAsync(ServiceDto dto, IFormFile? image, CancellationToken ct = default); Task DeleteAsync(Guid id, CancellationToken ct = default); }
public interface IPageService { Task<List<PageListDto>> ListAsync(CancellationToken ct = default); Task<PageEditDto?> GetAsync(Guid id, CancellationToken ct = default); Task<(PageEditDto? Page, List<SectionEditDto> Sections)> GetPublishedBySlugAsync(string slug, CancellationToken ct = default); Task<PageEditDto> SaveAsync(PageEditDto dto, CancellationToken ct = default); Task DeleteAsync(Guid id, CancellationToken ct = default); Task<List<SectionEditDto>> SectionsAsync(Guid pageId, CancellationToken ct = default); Task<SectionEditDto?> GetSectionAsync(Guid id, CancellationToken ct = default); Task<SectionEditDto> SaveSectionAsync(SectionEditDto dto, IFormFile? image, IFormFile? background, CancellationToken ct = default); Task DeleteSectionAsync(Guid id, CancellationToken ct = default); }
public interface IThemeService { Task<ThemeDto> GetAsync(CancellationToken ct = default); Task SaveAsync(ThemeDto dto, CancellationToken ct = default); }
public interface IWebsiteSettingService { Task<Dictionary<string,string>> GetAllAsync(CancellationToken ct = default); Task SaveAsync(Dictionary<string,string> values, CancellationToken ct = default); }
public interface IContactMessageService { Task<List<MessageDto>> ListAsync(bool includeArchived = false, CancellationToken ct = default); Task<MessageDto?> GetAsync(Guid id, CancellationToken ct = default); Task MarkReadAsync(Guid id, CancellationToken ct = default); Task ToggleReplyAsync(Guid id, CancellationToken ct = default); Task ArchiveAsync(Guid id, CancellationToken ct = default); Task DeleteAsync(Guid id, CancellationToken ct = default); Task CreateAsync(MessageDto dto, CancellationToken ct = default); }
public interface IAnalyticsService { Task<AnalyticsDto> GetAsync(int days = 30, CancellationToken ct = default); Task TrackAsync(HttpContext httpContext, Guid? pageId = null, string? url = null, CancellationToken ct = default); }
public interface IFileStorageService { Task<string> SaveAsync(IFormFile file, string folder, CancellationToken ct = default); Task DeleteAsync(string? relativePath); }
public interface IPermissionService { Task<bool> HasAsync(ClaimsPrincipal user, string permission, CancellationToken ct = default); Task<bool> HasRolePermissionAsync(Guid roleId, string permission, CancellationToken ct = default); }

public interface IAuditLogService
{
    Task CreateAsync(AuditLogDto dto, CancellationToken ct = default);
    Task<List<AuditLogDto>> ListAsync(string? userName = null, string? action = null, string? ipAddress = null, DateTime? fromUtc = null, DateTime? toUtc = null, int take = 200, CancellationToken ct = default);
}
