using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data.Entities;
using Portfolio.Data.Identity;

namespace Portfolio.Data.Context
{
    public class PortfolioDbContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public PortfolioDbContext(
            DbContextOptions<PortfolioDbContext> options)
            : base(options)
        {
        }


        // =========================
        // Company
        // =========================

        public DbSet<Company> Companies => Set<Company>();

        public DbSet<CompanyPhone> CompanyPhones => Set<CompanyPhone>();

        public DbSet<SocialLink> SocialLinks => Set<SocialLink>();


        // =========================
        // Products
        // =========================

        public DbSet<Product> Products => Set<Product>();

        public DbSet<ProductImage> ProductImages => Set<ProductImage>();


        // =========================
        // Services
        // =========================

        public DbSet<Service> Services => Set<Service>();


        // =========================
        // Website Builder
        // =========================

        public DbSet<Page> Pages => Set<Page>();

        public DbSet<Section> Sections => Set<Section>();

        public DbSet<WebsiteSetting> WebsiteSettings => Set<WebsiteSetting>();

        public DbSet<ThemeSetting> ThemeSettings => Set<ThemeSetting>();


        // =========================
        // Contact
        // =========================

        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();


        // =========================
        // Analytics
        // =========================

        public DbSet<Visitor> Visitors => Set<Visitor>();

        public DbSet<PageView> PageViews => Set<PageView>();


        // =========================
        // Permissions
        // =========================

        public DbSet<Permission> Permissions => Set<Permission>();

        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // =====================================================
            // Company
            // =====================================================

            builder.Entity<Company>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Description)
                    .HasMaxLength(2000);

                entity.Property(x => x.Logo)
                    .HasMaxLength(500);

                entity.Property(x => x.Favicon)
                    .HasMaxLength(500);

                entity.Property(x => x.Email)
                    .HasMaxLength(200);

                entity.Property(x => x.Address)
                    .HasMaxLength(500);

                entity.Property(x => x.WorkingHours)
                    .HasMaxLength(500);
            });


            // =====================================================
            // Company Phones
            // =====================================================

            builder.Entity<CompanyPhone>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Label)
                    .HasMaxLength(100);

                entity.HasOne(x => x.Company)
                    .WithMany(x => x.Phones)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.CompanyId);
            });


            // =====================================================
            // Social Links
            // =====================================================

            builder.Entity<SocialLink>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Platform)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Url)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.Icon)
                    .HasMaxLength(100);

                entity.HasOne(x => x.Company)
                    .WithMany(x => x.SocialLinks)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.CompanyId);
            });


            // =====================================================
            // Products
            // =====================================================

            builder.Entity<Product>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Description)
                    .HasMaxLength(2000);

                entity.Property(x => x.Price)
                    .HasPrecision(18, 2);

                entity.Property(x => x.Currency)
                    .IsRequired()
                    .HasMaxLength(10);
            });


            // =====================================================
            // Product Images
            // =====================================================

            builder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ImagePath)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(x => x.Product)
                    .WithMany(x => x.Images)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.ProductId);
            });


            // =====================================================
            // Services
            // =====================================================

            builder.Entity<Service>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Description)
                    .HasMaxLength(3000);

                entity.Property(x => x.Icon)
                    .HasMaxLength(100);

                entity.Property(x => x.Image)
                    .HasMaxLength(500);
            });


            // =====================================================
            // Pages
            // =====================================================

            builder.Entity<Page>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(x => x.NameEn).HasMaxLength(100);
                entity.Property(x => x.NameAr).HasMaxLength(100);

                entity.Property(x => x.Slug)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Title)
                    .HasMaxLength(200);
                entity.Property(x => x.TitleEn).HasMaxLength(200);
                entity.Property(x => x.TitleAr).HasMaxLength(200);

                entity.Property(x => x.MetaTitle)
                    .HasMaxLength(200);
                entity.Property(x => x.MetaTitleEn).HasMaxLength(200);
                entity.Property(x => x.MetaTitleAr).HasMaxLength(200);

                entity.Property(x => x.MetaDescription)
                    .HasMaxLength(500);
                entity.Property(x => x.MetaDescriptionEn).HasMaxLength(500);
                entity.Property(x => x.MetaDescriptionAr).HasMaxLength(500);

                entity.HasIndex(x => x.Slug)
                    .IsUnique();
            });


            // =====================================================
            // Sections
            // =====================================================

            builder.Entity<Section>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.SectionType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Title)
                    .HasMaxLength(300);
                entity.Property(x => x.TitleEn).HasMaxLength(300);
                entity.Property(x => x.TitleAr).HasMaxLength(300);

                entity.Property(x => x.Subtitle)
                    .HasMaxLength(500);
                entity.Property(x => x.SubtitleEn).HasMaxLength(500);
                entity.Property(x => x.SubtitleAr).HasMaxLength(500);

                entity.Property(x => x.Description)
                    .HasMaxLength(5000);
                entity.Property(x => x.DescriptionEn).HasMaxLength(5000);
                entity.Property(x => x.DescriptionAr).HasMaxLength(5000);

                entity.Property(x => x.Image)
                    .HasMaxLength(500);

                entity.Property(x => x.BackgroundImage)
                    .HasMaxLength(500);

                entity.Property(x => x.BackgroundColor)
                    .HasMaxLength(50);

                entity.Property(x => x.TextColor)
                    .HasMaxLength(50);

                entity.Property(x => x.SettingsJson)
                    .HasMaxLength(10000);

                entity.Property(x => x.Animation)
                    .HasMaxLength(100);

                entity.HasOne(x => x.Page)
                    .WithMany(x => x.Sections)
                    .HasForeignKey(x => x.PageId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new
                {
                    x.PageId,
                    x.SortOrder
                });
            });


            // =====================================================
            // Website Settings
            // =====================================================

            builder.Entity<WebsiteSetting>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Key)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Value)
                    .IsRequired()
                    .HasMaxLength(5000);

                entity.HasIndex(x => x.Key)
                    .IsUnique();
            });


            // =====================================================
            // Theme Settings
            // =====================================================

            builder.Entity<ThemeSetting>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.PrimaryColor)
                    .HasMaxLength(20);

                entity.Property(x => x.SecondaryColor)
                    .HasMaxLength(20);

                entity.Property(x => x.AccentColor)
                    .HasMaxLength(20);

                entity.Property(x => x.BodyColor)
                    .HasMaxLength(20);

                entity.Property(x => x.HeadingColor)
                    .HasMaxLength(20);

                entity.Property(x => x.FontFamily)
                    .HasMaxLength(100);

                entity.Property(x => x.HeadingFont)
                    .HasMaxLength(100);
            });


            // =====================================================
            // Contact Messages
            // =====================================================

            builder.Entity<ContactMessage>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Country)
                    .HasMaxLength(100);

                entity.Property(x => x.Phone)
                    .HasMaxLength(50);

                entity.Property(x => x.Subject)
                    .HasMaxLength(250);

                entity.Property(x => x.MessageType)
                    .HasMaxLength(50);

                entity.Property(x => x.Message)
                    .IsRequired()
                    .HasMaxLength(5000);

                entity.HasIndex(x => x.CreatedAt);

                entity.HasIndex(x => x.IsRead);
            });


            // =====================================================
            // Visitors
            // =====================================================

            builder.Entity<Visitor>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.VisitorKey)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.UserAgent)
                    .HasMaxLength(1000);

                entity.Property(x => x.Referrer)
                    .HasMaxLength(1000);

                entity.HasIndex(x => x.VisitorKey)
                    .IsUnique();
            });


            // =====================================================
            // Page Views
            // =====================================================

            builder.Entity<PageView>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Url)
                    .HasMaxLength(500);

                entity.HasOne(x => x.Visitor)
                    .WithMany(x => x.PageViews)
                    .HasForeignKey(x => x.VisitorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Page)
                    .WithMany()
                    .HasForeignKey(x => x.PageId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(x => x.VisitedAt);

                entity.HasIndex(x => x.PageId);

                entity.HasIndex(x => x.VisitorId);
            });


            // =====================================================
            // Audit Logs
            // =====================================================

            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UserName).IsRequired().HasMaxLength(256);
                entity.Property(x => x.ActionType).IsRequired().HasMaxLength(50);
                entity.Property(x => x.Area).IsRequired().HasMaxLength(100);
                entity.Property(x => x.Controller).IsRequired().HasMaxLength(150);
                entity.Property(x => x.Action).IsRequired().HasMaxLength(150);
                entity.Property(x => x.EntityId).HasMaxLength(100);
                entity.Property(x => x.Path).IsRequired().HasMaxLength(500);
                entity.Property(x => x.HttpMethod).IsRequired().HasMaxLength(20);
                entity.Property(x => x.IpAddress).HasMaxLength(100);
                entity.Property(x => x.UserAgent).HasMaxLength(1000);
                entity.Property(x => x.Details).HasMaxLength(2000);
                entity.HasIndex(x => x.OccurredAtUtc);
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.IpAddress);
                entity.HasIndex(x => new { x.Controller, x.Action });
            });

            // =====================================================
            // Permissions
            // =====================================================

            builder.Entity<Permission>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.HasIndex(x => x.Name)
                    .IsUnique();
            });


            // =====================================================
            // Role Permissions
            // =====================================================

            builder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(x => new
                {
                    x.RoleId,
                    x.PermissionId
                });

                entity.HasOne(x => x.Role)
                    .WithMany()
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Permission)
                    .WithMany(x => x.RolePermissions)
                    .HasForeignKey(x => x.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}