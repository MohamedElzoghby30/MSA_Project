using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Business.DTOs;
using Portfolio.Business.Interfaces;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;
using Portfolio.Data.Identity;
using System.Security.Claims;
using System.Text.Json;
namespace Portfolio.Business.Services;

public class FileStorageService(IWebHostEnvironment env) : IFileStorageService
{
    private static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".gif",
            ".bmp",
            ".ico",
            ".svg",
            ".avif",
            ".tif",
            ".tiff"
        };

    public async Task<string> SaveAsync(
        IFormFile file,
        string folder,
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new InvalidOperationException("The uploaded file is empty.");

        const long maxBytes = 10L * 1024 * 1024;

        if (file.Length > maxBytes)
            throw new InvalidOperationException(
                $"The file exceeds the maximum size of {maxBytes / (1024 * 1024)} MB.");

        var ext = Path.GetExtension(file.FileName);

        if (string.IsNullOrWhiteSpace(ext) || !Allowed.Contains(ext))
            throw new InvalidOperationException(
                "Unsupported image format. Allowed formats: JPG, JPEG, PNG, WEBP, GIF, BMP, ICO, SVG, AVIF, TIF and TIFF.");

        var allowedContentTypes = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif",
            "image/bmp",
            "image/x-icon",
            "image/vnd.microsoft.icon",
            "image/svg+xml",
            "image/avif",
            "image/tiff"
        };

        if (!string.IsNullOrWhiteSpace(file.ContentType) &&
            !allowedContentTypes.Contains(file.ContentType))
        {
            throw new InvalidOperationException(
                "The uploaded file is not a supported image type.");
        }

        var safeFolder = new string(
            folder
                .Where(c => char.IsLetterOrDigit(c) || c is '-' or '_')
                .ToArray());

        if (string.IsNullOrWhiteSpace(safeFolder))
            safeFolder = "general";

        var root = Path.Combine(
            env.WebRootPath,
            "uploads",
            safeFolder);

        Directory.CreateDirectory(root);

        var fileName =
            $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";

        var fullPath = Path.Combine(root, fileName);

        await using var stream = File.Create(fullPath);

        await file.CopyToAsync(stream, ct);

        return $"/uploads/{safeFolder}/{fileName}";
    }

    public Task DeleteAsync(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return Task.CompletedTask;

        var normalized =
            relativePath
                .Replace('\\', '/')
                .TrimStart('/');

        if (!normalized.StartsWith(
                "uploads/",
                StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var root = Path.GetFullPath(
            Path.Combine(
                env.WebRootPath,
                "uploads"));

        var full = Path.GetFullPath(
            Path.Combine(
                env.WebRootPath,
                normalized));

        if (!full.StartsWith(
                root + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        if (File.Exists(full))
            File.Delete(full);

        return Task.CompletedTask;
    }
}

public class PermissionService(PortfolioDbContext db, UserManager<ApplicationUser> users) : IPermissionService
{
    public async Task<bool> HasAsync(ClaimsPrincipal user, string permission, CancellationToken ct = default)
    {
        if (user?.Identity?.IsAuthenticated != true) return false;
        var appUser = await users.GetUserAsync(user);
        if (appUser is null || !appUser.IsActive) return false;
        if (await users.IsInRoleAsync(appUser, Portfolio.Data.Seed.RoleConstants.SuperAdmin)) return true;
        var roleIds = await users.GetRolesAsync(appUser);
        return await db.RolePermissions.AsNoTracking().Include(x => x.Permission).Include(x => x.Role)
            .AnyAsync(x => roleIds.Contains(x.Role.Name!) && x.Permission.Name == permission, ct);
    }
    public Task<bool> HasRolePermissionAsync(Guid roleId, string permission, CancellationToken ct = default) =>
        db.RolePermissions.AsNoTracking().Include(x => x.Permission).AnyAsync(x => x.RoleId == roleId && x.Permission.Name == permission, ct);
}

public class DashboardService(PortfolioDbContext db) : IDashboardService
{
    public async Task<DashboardDto> GetAsync(int days = 30, CancellationToken ct = default)
    {
        var from = DateTime.UtcNow.Date.AddDays(-(days - 1));
        var stats = new DashboardStatsDto(
            await db.Visitors.CountAsync(ct),
            await db.PageViews.CountAsync(ct),
            await db.Products.CountAsync(x => x.IsActive, ct),
            await db.Services.CountAsync(x => x.IsActive, ct),
            await db.ContactMessages.CountAsync(x => !x.IsRead && !x.IsArchived, ct),
            await db.Pages.CountAsync(x => x.IsPublished, ct));
        var visitorGroups = await db.Visitors.AsNoTracking().Where(x => x.LastVisitAt >= from).GroupBy(x => x.LastVisitAt.Date).Select(g => new { Date = g.Key, Count = g.Count() }).ToListAsync(ct);
        var pageGroups = await db.PageViews.AsNoTracking().Where(x => x.VisitedAt >= from).GroupBy(x => x.VisitedAt.Date).Select(g => new { Date = g.Key, Count = g.Count() }).ToListAsync(ct);
        var labels = Enumerable.Range(0, days).Select(i => from.AddDays(i)).ToList();
        var vt = labels.Select(d => new TrendPointDto(d.ToString("MM/dd"), visitorGroups.FirstOrDefault(x => x.Date == d)?.Count ?? 0)).ToList();
        var pt = labels.Select(d => new TrendPointDto(d.ToString("MM/dd"), pageGroups.FirstOrDefault(x => x.Date == d)?.Count ?? 0)).ToList();
        var recent = await db.ContactMessages.AsNoTracking().OrderByDescending(x => x.CreatedAt).Take(6).Select(x => new RecentMessageDto(x.Id, x.Name, x.Subject ?? "No subject", x.MessageType ?? "General Inquiry", x.CreatedAt, x.IsRead)).ToListAsync(ct);
        return new DashboardDto(stats, vt, pt, recent);
    }
}

public class CompanyService(PortfolioDbContext db, IFileStorageService files) : ICompanyService
{
    public async Task<CompanyDto?> GetAsync(CancellationToken ct = default) => await db.Companies.AsNoTracking().Include(x => x.Phones).Include(x => x.SocialLinks).Select(x => new CompanyDto { Id = x.Id, Name = x.Name, Description = x.Description, Logo = x.Logo, Favicon = x.Favicon, Email = x.Email, Address = x.Address, WorkingHours = x.WorkingHours, Phones = x.Phones.OrderBy(p => p.SortOrder).Select(p => new CompanyPhoneDto { Id = p.Id, PhoneNumber = p.PhoneNumber, Label = p.Label, IsWhatsApp = p.IsWhatsApp, IsActive = p.IsActive, SortOrder = p.SortOrder }).ToList(), SocialLinks = x.SocialLinks.OrderBy(s => s.SortOrder).Select(s => new SocialLinkDto { Id = s.Id, Platform = s.Platform, Url = s.Url, Icon = s.Icon, IsActive = s.IsActive, SortOrder = s.SortOrder }).ToList() }).FirstOrDefaultAsync(ct);
    public async Task<CompanyDto> SaveAsync(CompanyDto dto, IFormFile? logo, IFormFile? favicon, CancellationToken ct = default)
    {
        var entity = await db.Companies.FirstOrDefaultAsync(x => x.Id == dto.Id, ct) ?? await db.Companies.FirstOrDefaultAsync(ct) ?? new Company { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        entity.Name = dto.Name; entity.Description = dto.Description; entity.Email = dto.Email; entity.Address = dto.Address; entity.WorkingHours = dto.WorkingHours; entity.UpdatedAt = DateTime.UtcNow;
        if (logo is not null) { await files.DeleteAsync(entity.Logo); entity.Logo = await files.SaveAsync(logo, "company", ct); }
        if (favicon is not null) { await files.DeleteAsync(entity.Favicon); entity.Favicon = await files.SaveAsync(favicon, "company", ct); }
        if (entity.CreatedAt == default) entity.CreatedAt = DateTime.UtcNow;
        if (entity.Id == dto.Id && db.Entry(entity).State == EntityState.Detached) db.Companies.Add(entity); else if (entity.Id != dto.Id && !db.Companies.Local.Contains(entity)) db.Companies.Add(entity);
        await db.SaveChangesAsync(ct); dto.Id = entity.Id; dto.Logo = entity.Logo; dto.Favicon = entity.Favicon; return dto;
    }
    public async Task DeletePhoneAsync(Guid id, CancellationToken ct = default) { var x = await db.CompanyPhones.FindAsync([id], ct); if (x is not null) { db.CompanyPhones.Remove(x); await db.SaveChangesAsync(ct); } }
    public async Task SavePhoneAsync(
    Guid companyId,
    CompanyPhoneDto dto,
    CancellationToken ct = default)
    {
        if (!await db.Companies.AnyAsync(x => x.Id == companyId, ct))
            throw new InvalidOperationException("Company not found.");

        CompanyPhone x;

        var isNew = dto.Id == Guid.Empty;

        if (isNew)
        {
            x = new CompanyPhone
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId
            };

            db.CompanyPhones.Add(x);
        }
        else
        {
            x = await db.CompanyPhones.FirstOrDefaultAsync(
                    p => p.Id == dto.Id && p.CompanyId == companyId,
                    ct)
                ?? throw new InvalidOperationException("Phone not found.");
        }

        x.PhoneNumber = dto.PhoneNumber?.Trim() ?? "";
        x.Label = dto.Label?.Trim();
        x.IsWhatsApp = dto.IsWhatsApp;
        x.IsActive = dto.IsActive;
        x.SortOrder = dto.SortOrder;

        await db.SaveChangesAsync(ct);
    }
    public async Task DeleteSocialAsync(Guid id, CancellationToken ct = default) { var x = await db.SocialLinks.FindAsync([id], ct); if (x is not null) { db.SocialLinks.Remove(x); await db.SaveChangesAsync(ct); } }
    public async Task SaveSocialAsync(
       Guid companyId,
       SocialLinkDto dto,
       CancellationToken ct = default)
    {
        if (!await db.Companies.AnyAsync(x => x.Id == companyId, ct))
            throw new InvalidOperationException("Company not found.");

        SocialLink x;

        var isNew = dto.Id == Guid.Empty;

        if (isNew)
        {
            x = new SocialLink
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId
            };

            db.SocialLinks.Add(x);
        }
        else
        {
            x = await db.SocialLinks.FirstOrDefaultAsync(
                    s => s.Id == dto.Id && s.CompanyId == companyId,
                    ct)
                ?? throw new InvalidOperationException("Social link not found.");
        }

        x.Platform = dto.Platform?.Trim() ?? "";
        x.Url = dto.Url?.Trim() ?? "";
        x.Icon = dto.Icon?.Trim();
        x.IsActive = dto.IsActive;
        x.SortOrder = dto.SortOrder;

        await db.SaveChangesAsync(ct);
    }
}

public class ProductService(PortfolioDbContext db, IFileStorageService files) : IProductService
{
    public Task<List<ProductListDto>> ListAsync(CancellationToken ct = default) => db.Products.AsNoTracking().Include(x => x.Images).OrderBy(x => x.SortOrder).ThenBy(x => x.Name).Select(x => new ProductListDto { Id = x.Id, Name = x.Name, Description = x.Description, Price = x.Price, Currency = x.Currency, IsActive = x.IsActive, SortOrder = x.SortOrder, MainImage = x.Images.Where(i => i.IsMain).Select(i => i.ImagePath).FirstOrDefault() ?? x.Images.OrderBy(i => i.SortOrder).Select(i => i.ImagePath).FirstOrDefault(), ImageCount = x.Images.Count }).ToListAsync(ct);
    public Task<ProductEditDto?> GetAsync(Guid id, CancellationToken ct = default) => db.Products.AsNoTracking().Where(x => x.Id == id).Select(x => new ProductEditDto { Id = x.Id, Name = x.Name, Description = x.Description, Price = x.Price, Currency = x.Currency, IsActive = x.IsActive, SortOrder = x.SortOrder }).FirstOrDefaultAsync(ct);
    public async Task<ProductImagesDto?> GetImagesAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Products.AsNoTracking().Where(x => x.Id == id).Select(x => new ProductImagesDto
        {
            ProductId = x.Id,
            ProductName = x.Name,
            Images = x.Images.OrderBy(i => i.SortOrder).Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImagePath = i.ImagePath,
                IsMain = i.IsMain,
                SortOrder = i.SortOrder
            }).ToList()
        }).FirstOrDefaultAsync(ct);
    }
    public async Task<ProductEditDto> SaveAsync(
      ProductEditDto dto,
      CancellationToken ct = default)
    {
        Product x;

        if (dto.Id == Guid.Empty)
        {
            x = new Product
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            db.Products.Add(x);
        }
        else
        {
            x = await db.Products.FindAsync([dto.Id], ct)
                ?? throw new InvalidOperationException("Product not found.");
        }

        x.Name = dto.Name?.Trim() ?? "";
        x.Description = dto.Description?.Trim();
        x.Price = dto.Price;
        x.Currency = dto.Currency?.Trim() ?? "AED";
        x.IsActive = dto.IsActive;
        x.SortOrder = dto.SortOrder;
        x.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        dto.Id = x.Id;

        return dto;
    }
    public async Task DeleteAsync(Guid id, CancellationToken ct = default) { var x = await db.Products.Include(p => p.Images).FirstOrDefaultAsync(x => x.Id == id, ct); if (x is null) return; foreach (var image in x.Images) await files.DeleteAsync(image.ImagePath); db.Products.Remove(x); await db.SaveChangesAsync(ct); }
    public async Task<ProductImageUploadResult> AddImageAsync(
    Guid productId,
    IFormFile file,
    CancellationToken ct = default)
    {
        if (!await db.Products.AnyAsync(x => x.Id == productId, ct))
            return new(false, "Product not found.", null);

        if (file is null || file.Length == 0)
            return new(false, "Please select an image.", null);

        try
        {
            var path = await files.SaveAsync(
                file,
                "products",
                ct);

            var max =
                await db.ProductImages
                    .Where(x => x.ProductId == productId)
                    .Select(x => (int?)x.SortOrder)
                    .MaxAsync(ct) ?? 0;

            var hasImages =
                await db.ProductImages
                    .AnyAsync(x => x.ProductId == productId, ct);

            var image = new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ImagePath = path,
                SortOrder = max + 1,
                IsMain = !hasImages
            };

            db.ProductImages.Add(image);

            await db.SaveChangesAsync(ct);

            return new(true, null, path);
        }
        catch (Exception ex)
        {
            return new(false, ex.Message, null);
        }
    }
    public async Task DeleteImageAsync(Guid id, CancellationToken ct = default) { var x = await db.ProductImages.FindAsync([id], ct); if (x is null) return; await files.DeleteAsync(x.ImagePath); db.ProductImages.Remove(x); await db.SaveChangesAsync(ct); }
    public async Task SetMainImageAsync(Guid imageId, CancellationToken ct = default) { var x = await db.ProductImages.FindAsync([imageId], ct); if (x is null) return; var images = await db.ProductImages.Where(i => i.ProductId == x.ProductId).ToListAsync(ct); foreach (var i in images) i.IsMain = i.Id == imageId; await db.SaveChangesAsync(ct); }
}

public class ServiceService(PortfolioDbContext db, IFileStorageService files) : IServiceService
{
    public Task<List<ServiceDto>> ListAsync(CancellationToken ct = default) => db.Services.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Title).Select(x => new ServiceDto { Id = x.Id, Title = x.Title, Description = x.Description, Icon = x.Icon, Image = x.Image, IsActive = x.IsActive, SortOrder = x.SortOrder }).ToListAsync(ct);
    public Task<ServiceDto?> GetAsync(Guid id, CancellationToken ct = default) => db.Services.AsNoTracking().Where(x => x.Id == id).Select(x => new ServiceDto { Id = x.Id, Title = x.Title, Description = x.Description, Icon = x.Icon, Image = x.Image, IsActive = x.IsActive, SortOrder = x.SortOrder }).FirstOrDefaultAsync(ct);
    public async Task<ServiceDto> SaveAsync(
     ServiceDto dto,
     IFormFile? image,
     CancellationToken ct = default)
    {
        Service x;

        if (dto.Id == Guid.Empty)
        {
            x = new Service
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            db.Services.Add(x);
        }
        else
        {
            x = await db.Services.FindAsync([dto.Id], ct)
                ?? throw new InvalidOperationException("Service not found.");
        }

        x.Title = dto.Title?.Trim() ?? "";
        x.Description = dto.Description?.Trim();
        x.Icon = dto.Icon?.Trim();
        x.IsActive = dto.IsActive;
        x.SortOrder = dto.SortOrder;

        if (image is not null && image.Length > 0)
        {
            await files.DeleteAsync(x.Image);

            x.Image = await files.SaveAsync(
                image,
                "services",
                ct);
        }

        x.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        dto.Id = x.Id;
        dto.Image = x.Image;

        return dto;
    }
    public async Task DeleteAsync(Guid id, CancellationToken ct = default) { var x = await db.Services.FindAsync([id], ct); if (x is null) return; await files.DeleteAsync(x.Image); db.Services.Remove(x); await db.SaveChangesAsync(ct); }
}

public class PageService(PortfolioDbContext db, IFileStorageService files) : IPageService
{
    private static string NormalizeSlug(string slug) =>
        (slug ?? "").Trim().Trim('/').ToLowerInvariant().Replace(" ", "-");

    private static string Legacy(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;

    private static List<SectionStatDto> DefaultAboutStats()
    {
        return
        [
            new SectionStatDto
        {
            ValueEn = "2025",
            ValueAr = "2025",
            LabelEn = "Established",
            LabelAr = "?????"
        },
        new SectionStatDto
        {
            ValueEn = "Dubai, UAE",
            ValueAr = "???? ????????",
            LabelEn = "Gold Souq",
            LabelAr = "??? ?????"
        },
        new SectionStatDto
        {
            ValueEn = "999.9",
            ValueAr = "999.9",
            LabelEn = "Fine gold standard",
            LabelAr = "???? ?????"
        },
        new SectionStatDto
        {
            ValueEn = "Global",
            ValueAr = "?????",
            LabelEn = "Wholesale distribution",
            LabelAr = "??????? ???????"
        }
        ];
    }

    private static List<SectionStatDto> ReadAboutStats(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return DefaultAboutStats();

        try
        {
            var stats = JsonSerializer.Deserialize<List<SectionStatDto>>(json);

            return stats is { Count: > 0 }
                ? stats
                : DefaultAboutStats();
        }
        catch
        {
            return DefaultAboutStats();
        }
    }
    public Task<List<PageListDto>> ListAsync(CancellationToken ct = default) =>
        db.Pages.AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new PageListDto
            {
                Id = x.Id,
                Name = x.Name,
                NameEn = x.NameEn ?? x.Name,
                NameAr = x.NameAr,
                Slug = x.Slug,
                Title = x.Title,
                TitleEn = x.TitleEn ?? x.Title,
                TitleAr = x.TitleAr,
                IsPublished = x.IsPublished,
                SectionCount = x.Sections.Count,
                SortOrder = x.SortOrder
            })
            .ToListAsync(ct);

    public Task<PageEditDto?> GetAsync(Guid id, CancellationToken ct = default) =>
        db.Pages.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PageEditDto
            {
                Id = x.Id,
                Name = x.Name,
                NameEn = x.NameEn ?? x.Name,
                NameAr = x.NameAr,
                Slug = x.Slug,
                Title = x.Title,
                TitleEn = x.TitleEn ?? x.Title,
                TitleAr = x.TitleAr,
                MetaTitle = x.MetaTitle,
                MetaTitleEn = x.MetaTitleEn ?? x.MetaTitle,
                MetaTitleAr = x.MetaTitleAr,
                MetaDescription = x.MetaDescription,
                MetaDescriptionEn = x.MetaDescriptionEn ?? x.MetaDescription,
                MetaDescriptionAr = x.MetaDescriptionAr,
                IsPublished = x.IsPublished,
                SortOrder = x.SortOrder
            })
            .FirstOrDefaultAsync(ct);

    public async Task<(PageEditDto? Page, List<SectionEditDto> Sections)> GetPublishedBySlugAsync(
        string slug,
        CancellationToken ct = default)
    {
        slug = NormalizeSlug(slug);

        var p = await db.Pages.AsNoTracking()
            .Where(x => x.IsPublished && x.Slug == slug)
            .Select(x => new PageEditDto
            {
                Id = x.Id,
                Name = x.Name,
                NameEn = x.NameEn ?? x.Name,
                NameAr = x.NameAr,
                Slug = x.Slug,
                Title = x.Title,
                TitleEn = x.TitleEn ?? x.Title,
                TitleAr = x.TitleAr,
                MetaTitle = x.MetaTitle,
                MetaTitleEn = x.MetaTitleEn ?? x.MetaTitle,
                MetaTitleAr = x.MetaTitleAr,
                MetaDescription = x.MetaDescription,
                MetaDescriptionEn = x.MetaDescriptionEn ?? x.MetaDescription,
                MetaDescriptionAr = x.MetaDescriptionAr,
                IsPublished = x.IsPublished,
                SortOrder = x.SortOrder
            })
            .FirstOrDefaultAsync(ct);

        if (p is null) return (null, []);

        return (p, await SectionsAsync(p.Id, ct));
    }

    public async Task<PageEditDto> SaveAsync(PageEditDto dto, CancellationToken ct = default)
    {
        dto.Slug = NormalizeSlug(dto.Slug);

        if (string.IsNullOrWhiteSpace(dto.NameEn) && string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Page English name is required.");

        if (await db.Pages.AnyAsync(x => x.Slug == dto.Slug && x.Id != dto.Id, ct))
            throw new InvalidOperationException("Slug already exists.");

        Page x;
        if (dto.Id == Guid.Empty)
        {
            x = new Page
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };
            db.Pages.Add(x);
        }
        else
        {
            x = await db.Pages.FindAsync([dto.Id], ct)
                ?? throw new InvalidOperationException("Page not found.");
        }

        x.NameEn = Legacy(dto.NameEn, dto.Name);
        x.NameAr = dto.NameAr;
        x.Name = x.NameEn ?? dto.Name ?? "Untitled Page";

        x.Slug = dto.Slug;
        x.TitleEn = Legacy(dto.TitleEn, dto.Title);
        x.TitleAr = dto.TitleAr;
        x.Title = x.TitleEn;

        x.MetaTitleEn = Legacy(dto.MetaTitleEn, dto.MetaTitle);
        x.MetaTitleAr = dto.MetaTitleAr;
        x.MetaTitle = x.MetaTitleEn;

        x.MetaDescriptionEn = Legacy(dto.MetaDescriptionEn, dto.MetaDescription);
        x.MetaDescriptionAr = dto.MetaDescriptionAr;
        x.MetaDescription = x.MetaDescriptionEn;

        x.IsPublished = dto.IsPublished;
        x.SortOrder = dto.SortOrder;
        x.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        dto.Id = x.Id;
        dto.Name = x.Name;
        dto.Title = x.Title;
        dto.MetaTitle = x.MetaTitle;
        dto.MetaDescription = x.MetaDescription;

        return dto;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var x = await db.Pages.Include(p => p.Sections).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (x is null) return;

        foreach (var s in x.Sections)
        {
            await files.DeleteAsync(s.Image);
            await files.DeleteAsync(s.BackgroundImage);
        }

        db.Pages.Remove(x);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<SectionEditDto>> SectionsAsync(
     Guid pageId,
     CancellationToken ct = default)
    {
        var items = await db.Sections
            .AsNoTracking()
            .Where(x => x.PageId == pageId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new SectionEditDto
            {
                Id = x.Id,
                PageId = x.PageId,
                SectionType = x.SectionType,

                Title = x.Title,
                TitleEn = x.TitleEn ?? x.Title,
                TitleAr = x.TitleAr,

                Subtitle = x.Subtitle,
                SubtitleEn = x.SubtitleEn ?? x.Subtitle,
                SubtitleAr = x.SubtitleAr,

                Description = x.Description,
                DescriptionEn = x.DescriptionEn ?? x.Description,
                DescriptionAr = x.DescriptionAr,

                Image = x.Image,
                BackgroundImage = x.BackgroundImage,
                BackgroundColor = x.BackgroundColor,
                TextColor = x.TextColor,

                SettingsJson = x.SettingsJson,

                SortOrder = x.SortOrder,
                IsVisible = x.IsVisible,

                Animation = x.Animation,
                AnimationDuration = x.AnimationDuration,
                AnimationDelay = x.AnimationDelay
            })
            .ToListAsync(ct);

        foreach (var item in items)
        {
            if (string.Equals(
                    item.SectionType,
                    "About",
                    StringComparison.OrdinalIgnoreCase))
            {
                item.Stats = ReadAboutStats(item.SettingsJson);
            }
        }

        return items;
    }

    public async Task<SectionEditDto?> GetSectionAsync(
      Guid id,
      CancellationToken ct = default)
    {
        var item = await db.Sections
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SectionEditDto
            {
                Id = x.Id,
                PageId = x.PageId,
                SectionType = x.SectionType,

                Title = x.Title,
                TitleEn = x.TitleEn ?? x.Title,
                TitleAr = x.TitleAr,

                Subtitle = x.Subtitle,
                SubtitleEn = x.SubtitleEn ?? x.Subtitle,
                SubtitleAr = x.SubtitleAr,

                Description = x.Description,
                DescriptionEn = x.DescriptionEn ?? x.Description,
                DescriptionAr = x.DescriptionAr,

                Image = x.Image,
                BackgroundImage = x.BackgroundImage,
                BackgroundColor = x.BackgroundColor,
                TextColor = x.TextColor,

                SettingsJson = x.SettingsJson,

                SortOrder = x.SortOrder,
                IsVisible = x.IsVisible,

                Animation = x.Animation,
                AnimationDuration = x.AnimationDuration,
                AnimationDelay = x.AnimationDelay
            })
            .FirstOrDefaultAsync(ct);

        if (item is null)
            return null;

        if (string.Equals(
                item.SectionType,
                "About",
                StringComparison.OrdinalIgnoreCase))
        {
            item.Stats = ReadAboutStats(item.SettingsJson);
        }

        return item;
    }

    public async Task<SectionEditDto> SaveSectionAsync(
        SectionEditDto dto,
        IFormFile? image,
        IFormFile? background,
        CancellationToken ct = default)
    {
        if (!await db.Pages.AnyAsync(p => p.Id == dto.PageId, ct))
            throw new InvalidOperationException("Page not found.");

        Section x;
        if (dto.Id == Guid.Empty)
        {
            x = new Section
            {
                Id = Guid.NewGuid(),
                PageId = dto.PageId
            };
            db.Sections.Add(x);
        }
        else
        {
            x = await db.Sections.FindAsync([dto.Id], ct)
                ?? throw new InvalidOperationException("Section not found.");
        }

        x.SectionType = string.IsNullOrWhiteSpace(dto.SectionType) ? "Text" : dto.SectionType.Trim();

        x.TitleEn = dto.TitleEn ?? dto.Title;
        x.TitleAr = dto.TitleAr;
        x.Title = x.TitleEn;

        x.SubtitleEn = dto.SubtitleEn ?? dto.Subtitle;
        x.SubtitleAr = dto.SubtitleAr;
        x.Subtitle = x.SubtitleEn;

        x.DescriptionEn = dto.DescriptionEn ?? dto.Description;
        x.DescriptionAr = dto.DescriptionAr;
        x.Description = x.DescriptionEn;

        x.BackgroundColor = dto.BackgroundColor;
        x.TextColor = dto.TextColor;
        if (string.Equals(
          x.SectionType,
          "About",
          StringComparison.OrdinalIgnoreCase))
        {
            var stats = dto.Stats
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.ValueEn) ||
                    !string.IsNullOrWhiteSpace(x.ValueAr) ||
                    !string.IsNullOrWhiteSpace(x.LabelEn) ||
                    !string.IsNullOrWhiteSpace(x.LabelAr) ||
                    !string.IsNullOrWhiteSpace(x.Icon))
                .ToList();

            x.SettingsJson = JsonSerializer.Serialize(stats);
        }
        else
        {
            x.SettingsJson = dto.SettingsJson;
        }
        x.SortOrder = dto.SortOrder;
        x.IsVisible = dto.IsVisible;
        x.Animation = dto.Animation;
        x.AnimationDuration = dto.AnimationDuration;
        x.AnimationDelay = dto.AnimationDelay;

        if (image is not null)
        {
            await files.DeleteAsync(x.Image);
            x.Image = await files.SaveAsync(image, "sections", ct);
        }

        if (background is not null)
        {
            await files.DeleteAsync(x.BackgroundImage);
            x.BackgroundImage = await files.SaveAsync(background, "sections", ct);
        }

        await db.SaveChangesAsync(ct);

        dto.Id = x.Id;
        dto.Title = x.Title;
        dto.Subtitle = x.Subtitle;
        dto.Description = x.Description;
        dto.Image = x.Image;
        dto.BackgroundImage = x.BackgroundImage;

        return dto;
    }

    public async Task DeleteSectionAsync(Guid id, CancellationToken ct = default)
    {
        var x = await db.Sections.FindAsync([id], ct);
        if (x is null) return;

        await files.DeleteAsync(x.Image);
        await files.DeleteAsync(x.BackgroundImage);

        db.Sections.Remove(x);
        await db.SaveChangesAsync(ct);
    }
}

public class ThemeService(PortfolioDbContext db) : IThemeService
{
    private static readonly Dictionary<string, string> Defaults = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ThemeMode"] = "system",
        ["SuccessColor"] = "#1FA774",
        ["WarningColor"] = "#D9A441",
        ["DangerColor"] = "#D9534F",
        ["LightBackground"] = "#F7F8FA",
        ["LightSurface"] = "#FFFFFF",
        ["LightSurfaceAlt"] = "#F1F3F6",
        ["LightText"] = "#102033",
        ["LightMutedText"] = "#66758A",
        ["LightBorder"] = "#DDE3EA",
        ["LightHeader"] = "#FFFFFF",
        ["LightFooter"] = "#0D2136",
        ["LightInput"] = "#FFFFFF",
        ["LightShadow"] = "rgba(16,32,51,.12)",
        ["DarkBackground"] = "#061827",
        ["DarkSurface"] = "#0B243B",
        ["DarkSurfaceAlt"] = "#0F2D49",
        ["DarkText"] = "#F7FAFC",
        ["DarkMutedText"] = "#91A1B5",
        ["DarkBorder"] = "rgba(255,255,255,.09)",
        ["DarkHeader"] = "#061827",
        ["DarkFooter"] = "#03121F",
        ["DarkInput"] = "#0B253C",
        ["DarkShadow"] = "rgba(0,0,0,.35)"
    };

    public async Task<ThemeDto> GetAsync(CancellationToken ct = default)
    {
        var x = await db.ThemeSettings.AsNoTracking().FirstOrDefaultAsync(ct)
                ?? new ThemeSetting { Id = Guid.NewGuid() };
        var settings = await db.WebsiteSettings.AsNoTracking()
            .Where(x => x.Key.StartsWith("Theme."))
            .ToDictionaryAsync(x => x.Key[6..], x => x.Value, ct);

        string Get(string key, string fallback) => settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
        int GetInt(string key, int fallback) => int.TryParse(Get(key, fallback.ToString()), out var value) ? value : fallback;
        bool GetBool(string key, bool fallback) => bool.TryParse(Get(key, fallback.ToString()), out var value) ? value : fallback;

        return new ThemeDto
        {
            Id = x.Id,
            PrimaryColor = x.PrimaryColor ?? "#C88A24",
            SecondaryColor = x.SecondaryColor ?? "#08233F",
            AccentColor = x.AccentColor ?? "#D6A84F",
            BodyColor = x.BodyColor ?? "#FFFFFF",
            HeadingColor = x.HeadingColor ?? "#08233F",
            FontFamily = x.FontFamily ?? "Inter",
            HeadingFont = x.HeadingFont ?? "Poppins",
            BorderRadius = x.BorderRadius,
            ButtonRadius = x.ButtonRadius,
            ContainerWidth = x.ContainerWidth,
            EnableAnimations = x.EnableAnimations,
            ThemeMode = Get("ThemeMode", "system"),
            SuccessColor = Get("SuccessColor", Defaults["SuccessColor"]),
            WarningColor = Get("WarningColor", Defaults["WarningColor"]),
            DangerColor = Get("DangerColor", Defaults["DangerColor"]),
            LightBackground = Get("LightBackground", Defaults["LightBackground"]),
            LightSurface = Get("LightSurface", Defaults["LightSurface"]),
            LightSurfaceAlt = Get("LightSurfaceAlt", Defaults["LightSurfaceAlt"]),
            LightText = Get("LightText", Defaults["LightText"]),
            LightMutedText = Get("LightMutedText", Defaults["LightMutedText"]),
            LightBorder = Get("LightBorder", Defaults["LightBorder"]),
            LightHeader = Get("LightHeader", Defaults["LightHeader"]),
            LightFooter = Get("LightFooter", Defaults["LightFooter"]),
            LightInput = Get("LightInput", Defaults["LightInput"]),
            LightShadow = Get("LightShadow", Defaults["LightShadow"]),
            DarkBackground = Get("DarkBackground", Defaults["DarkBackground"]),
            DarkSurface = Get("DarkSurface", Defaults["DarkSurface"]),
            DarkSurfaceAlt = Get("DarkSurfaceAlt", Defaults["DarkSurfaceAlt"]),
            DarkText = Get("DarkText", Defaults["DarkText"]),
            DarkMutedText = Get("DarkMutedText", Defaults["DarkMutedText"]),
            DarkBorder = Get("DarkBorder", Defaults["DarkBorder"]),
            DarkHeader = Get("DarkHeader", Defaults["DarkHeader"]),
            DarkFooter = Get("DarkFooter", Defaults["DarkFooter"]),
            DarkInput = Get("DarkInput", Defaults["DarkInput"]),
            DarkShadow = Get("DarkShadow", Defaults["DarkShadow"])
        };
    }

    public async Task SaveAsync(ThemeDto dto, CancellationToken ct = default)
    {
        var x = await db.ThemeSettings.FirstOrDefaultAsync(ct)
                ?? new ThemeSetting { Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id };

        x.PrimaryColor = dto.PrimaryColor;
        x.SecondaryColor = dto.SecondaryColor;
        x.AccentColor = dto.AccentColor;
        x.BodyColor = dto.BodyColor;
        x.HeadingColor = dto.HeadingColor;
        x.FontFamily = dto.FontFamily;
        x.HeadingFont = dto.HeadingFont;
        x.BorderRadius = dto.BorderRadius;
        x.ButtonRadius = dto.ButtonRadius;
        x.ContainerWidth = dto.ContainerWidth;
        x.EnableAnimations = dto.EnableAnimations;

        if (db.Entry(x).State == EntityState.Detached) db.ThemeSettings.Add(x);

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ThemeMode"] = dto.ThemeMode,
            ["SuccessColor"] = dto.SuccessColor,
            ["WarningColor"] = dto.WarningColor,
            ["DangerColor"] = dto.DangerColor,
            ["LightBackground"] = dto.LightBackground,
            ["LightSurface"] = dto.LightSurface,
            ["LightSurfaceAlt"] = dto.LightSurfaceAlt,
            ["LightText"] = dto.LightText,
            ["LightMutedText"] = dto.LightMutedText,
            ["LightBorder"] = dto.LightBorder,
            ["LightHeader"] = dto.LightHeader,
            ["LightFooter"] = dto.LightFooter,
            ["LightInput"] = dto.LightInput,
            ["LightShadow"] = dto.LightShadow,
            ["DarkBackground"] = dto.DarkBackground,
            ["DarkSurface"] = dto.DarkSurface,
            ["DarkSurfaceAlt"] = dto.DarkSurfaceAlt,
            ["DarkText"] = dto.DarkText,
            ["DarkMutedText"] = dto.DarkMutedText,
            ["DarkBorder"] = dto.DarkBorder,
            ["DarkHeader"] = dto.DarkHeader,
            ["DarkFooter"] = dto.DarkFooter,
            ["DarkInput"] = dto.DarkInput,
            ["DarkShadow"] = dto.DarkShadow
        };

        var existing = await db.WebsiteSettings.Where(s => s.Key.StartsWith("Theme.")).ToListAsync(ct);
        foreach (var pair in values)
        {
            var key = $"Theme.{pair.Key}";
            var setting = existing.FirstOrDefault(s => s.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (setting is null)
                db.WebsiteSettings.Add(new WebsiteSetting { Id = Guid.NewGuid(), Key = key, Value = pair.Value ?? "" });
            else
                setting.Value = pair.Value ?? "";
        }

        await db.SaveChangesAsync(ct);
    }
}

public class WebsiteSettingService(PortfolioDbContext db) : IWebsiteSettingService
{
    public async Task<Dictionary<string, string>> GetAllAsync(CancellationToken ct = default) => await db.WebsiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value, ct);
    public async Task SaveAsync(Dictionary<string, string> values, CancellationToken ct = default) { var existing = await db.WebsiteSettings.ToListAsync(ct); foreach (var pair in values) { var x = existing.FirstOrDefault(x => x.Key == pair.Key); if (x is null) db.WebsiteSettings.Add(new WebsiteSetting { Id = Guid.NewGuid(), Key = pair.Key, Value = pair.Value ?? "" }); else x.Value = pair.Value ?? ""; } await db.SaveChangesAsync(ct); }
}

public class ContactMessageService(PortfolioDbContext db) : IContactMessageService
{
    private static MessageDto Map(ContactMessage x) => new() { Id = x.Id, Name = x.Name, Email = x.Email, Country = x.Country, Phone = x.Phone, Subject = x.Subject, MessageType = x.MessageType, Message = x.Message, CreatedAt = x.CreatedAt, IsRead = x.IsRead, IsReplied = x.IsReplied, IsArchived = x.IsArchived };
    public async Task<List<MessageDto>> ListAsync(bool includeArchived = false, CancellationToken ct = default) { var q = db.ContactMessages.AsNoTracking().AsQueryable(); if (!includeArchived) q = q.Where(x => !x.IsArchived); return (await q.OrderByDescending(x => x.CreatedAt).ToListAsync(ct)).Select(Map).ToList(); }
    public async Task<MessageDto?> GetAsync(Guid id, CancellationToken ct = default) { var x = await db.ContactMessages.FindAsync([id], ct); return x is null ? null : Map(x); }
    private async Task Update(Guid id, Action<ContactMessage> action, CancellationToken ct) { var x = await db.ContactMessages.FindAsync([id], ct); if (x is null) return; action(x); await db.SaveChangesAsync(ct); }
    public Task MarkReadAsync(Guid id, CancellationToken ct = default) => Update(id, x => x.IsRead = !x.IsRead, ct);
    public Task ToggleReplyAsync(Guid id, CancellationToken ct = default) => Update(id, x => x.IsReplied = !x.IsReplied, ct);
    public Task ArchiveAsync(Guid id, CancellationToken ct = default) => Update(id, x => x.IsArchived = true, ct);
    public async Task DeleteAsync(Guid id, CancellationToken ct = default) { var x = await db.ContactMessages.FindAsync([id], ct); if (x is not null) { db.ContactMessages.Remove(x); await db.SaveChangesAsync(ct); } }
    public async Task CreateAsync(MessageDto dto, CancellationToken ct = default) { db.ContactMessages.Add(new ContactMessage { Id = Guid.NewGuid(), Name = dto.Name, Email = dto.Email, Country = dto.Country, Phone = dto.Phone, Subject = dto.Subject, MessageType = dto.MessageType ?? "General Inquiry", Message = dto.Message, CreatedAt = DateTime.UtcNow }); await db.SaveChangesAsync(ct); }
}

public class AnalyticsService(PortfolioDbContext db) : IAnalyticsService
{
    public async Task<AnalyticsDto> GetAsync(int days = 30, CancellationToken ct = default) { var from = DateTime.UtcNow.Date.AddDays(-(days - 1)); var visitors = await db.Visitors.AsNoTracking().CountAsync(x => x.LastVisitAt >= from, ct); var views = await db.PageViews.AsNoTracking().CountAsync(x => x.VisitedAt >= from, ct); var groups = await db.PageViews.AsNoTracking().Where(x => x.VisitedAt >= from).GroupBy(x => x.VisitedAt.Date).Select(g => new { D = g.Key, C = g.Count() }).ToListAsync(ct); var labels = Enumerable.Range(0, days).Select(i => from.AddDays(i)); var trend = labels.Select(d => new TrendPointDto(d.ToString("MM/dd"), groups.FirstOrDefault(x => x.D == d)?.C ?? 0)).ToList(); var popular = await db.PageViews.AsNoTracking().Where(x => x.VisitedAt >= from && x.Url != null).GroupBy(x => x.Url!).OrderByDescending(g => g.Count()).Take(10).Select(g => new { Url = g.Key, Count = g.Count() }).ToListAsync(ct); return new AnalyticsDto { UniqueVisitors = visitors, PageViews = views, Trend = trend, PopularPages = popular.Select(x => (x.Url, x.Count)).ToList() }; }
    public async Task TrackAsync(HttpContext httpContext, Guid? pageId = null, string? url = null, CancellationToken ct = default) { if (httpContext.Request.Path.StartsWithSegments("/Admin")) return; var visitorKey = httpContext.Request.Cookies["portfolio_visitor"] ?? Guid.NewGuid().ToString("N"); if (!httpContext.Response.HasStarted) httpContext.Response.Cookies.Append("portfolio_visitor", visitorKey, new CookieOptions { HttpOnly = true, IsEssential = true, MaxAge = TimeSpan.FromDays(365) }); var visitor = await db.Visitors.FirstOrDefaultAsync(x => x.VisitorKey == visitorKey, ct); if (visitor is null) { visitor = new Visitor { Id = Guid.NewGuid(), VisitorKey = visitorKey, UserAgent = httpContext.Request.Headers.UserAgent.ToString(), Referrer = httpContext.Request.Headers.Referer.ToString(), FirstVisitAt = DateTime.UtcNow, LastVisitAt = DateTime.UtcNow }; db.Visitors.Add(visitor); } else visitor.LastVisitAt = DateTime.UtcNow; db.PageViews.Add(new PageView { Id = Guid.NewGuid(), VisitorId = visitor.Id, PageId = pageId, Url = url ?? httpContext.Request.Path.ToString(), VisitedAt = DateTime.UtcNow }); await db.SaveChangesAsync(ct); }
}

public class AuditLogService(PortfolioDbContext db) : IAuditLogService
{
    public async Task CreateAsync(AuditLogDto dto, CancellationToken ct = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            UserId = dto.UserId,
            UserName = string.IsNullOrWhiteSpace(dto.UserName) ? "Anonymous" : dto.UserName,
            ActionType = dto.ActionType,
            Area = dto.Area,
            Controller = dto.Controller,
            Action = dto.Action,
            EntityId = dto.EntityId,
            Path = dto.Path,
            HttpMethod = dto.HttpMethod,
            StatusCode = dto.StatusCode,
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent,
            Details = dto.Details,
            OccurredAtUtc = dto.OccurredAtUtc == default ? DateTime.UtcNow : dto.OccurredAtUtc
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task<List<AuditLogDto>> ListAsync(
        string? userName = null,
        string? action = null,
        string? ipAddress = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        int take = 200,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 20, 1000);

        var query = db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(userName))
            query = query.Where(x => x.UserName.Contains(userName));

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Controller.Contains(action) || x.Action.Contains(action) || x.Path.Contains(action));

        if (!string.IsNullOrWhiteSpace(ipAddress))
            query = query.Where(x => x.IpAddress != null && x.IpAddress.Contains(ipAddress));

        if (fromUtc.HasValue)
            query = query.Where(x => x.OccurredAtUtc >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(x => x.OccurredAtUtc <= toUtc.Value);

        return await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(take)
            .Select(x => new AuditLogDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserName = x.UserName,
                ActionType = x.ActionType,
                Area = x.Area,
                Controller = x.Controller,
                Action = x.Action,
                EntityId = x.EntityId,
                Path = x.Path,
                HttpMethod = x.HttpMethod,
                StatusCode = x.StatusCode,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                Details = x.Details,
                OccurredAtUtc = x.OccurredAtUtc
            })
            .ToListAsync(ct);
    }
}
