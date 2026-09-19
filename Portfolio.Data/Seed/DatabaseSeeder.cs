using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;
using Portfolio.Data.Identity;

namespace Portfolio.Data.Seed;

public static class DatabaseSeeder
{
    private const string AdminEmail = "admin@portfolio.com";
    private const string AdminPassword = "Admin@123456";
    private const string ContentVersionKey = "MsaContentSeedVersion";
    private const string ContentVersion = "2";

    public static async Task SeedAsync(
        PortfolioDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedPermissionsAsync(context);
        await SeedRolePermissionsAsync(context, roleManager);
        await SeedSuperAdminAsync(userManager, roleManager);
        await SeedWebsiteDataAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        var roles = new[]
        {
            (RoleConstants.SuperAdmin, "Full access to the entire system."),
            (RoleConstants.Admin, "Administrative access to the website."),
            (RoleConstants.ContentManager, "Manage website content."),
            (RoleConstants.Support, "Manage contact messages.")
        };

        foreach (var (name, description) in roles)
        {
            var existing = await roleManager.FindByNameAsync(name);
            if (existing is not null) continue;

            var role = new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = name,
                NormalizedName = name.ToUpperInvariant(),
                Description = description
            };

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create role '{name}': " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static async Task SeedPermissionsAsync(PortfolioDbContext context)
    {
        var permissions = new[]
        {
            PermissionConstants.DashboardView,
            PermissionConstants.CompanyView, PermissionConstants.CompanyEdit,
            PermissionConstants.ProductsView, PermissionConstants.ProductsCreate, PermissionConstants.ProductsEdit, PermissionConstants.ProductsDelete,
            PermissionConstants.ProductImagesView, PermissionConstants.ProductImagesCreate, PermissionConstants.ProductImagesDelete,
            PermissionConstants.ServicesView, PermissionConstants.ServicesCreate, PermissionConstants.ServicesEdit, PermissionConstants.ServicesDelete,
            PermissionConstants.PagesView, PermissionConstants.PagesCreate, PermissionConstants.PagesEdit, PermissionConstants.PagesDelete,
            PermissionConstants.SectionsView, PermissionConstants.SectionsCreate, PermissionConstants.SectionsEdit, PermissionConstants.SectionsDelete,
            PermissionConstants.AppearanceView, PermissionConstants.AppearanceEdit,
            PermissionConstants.MessagesView, PermissionConstants.MessagesDelete, PermissionConstants.MessagesReply,
            PermissionConstants.AnalyticsView,
            PermissionConstants.UsersView, PermissionConstants.UsersCreate, PermissionConstants.UsersEdit, PermissionConstants.UsersDelete,
            PermissionConstants.RolesView, PermissionConstants.RolesCreate, PermissionConstants.RolesEdit, PermissionConstants.RolesDelete,
            PermissionConstants.AuditLogsView,
            PermissionConstants.SettingsView, PermissionConstants.SettingsEdit,
            PermissionConstants.SettingsLocalization, PermissionConstants.SettingsDarkMode
        };

        foreach (var name in permissions)
        {
            if (!await context.Permissions.AnyAsync(x => x.Name == name))
            {
                context.Permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = name
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolePermissionsAsync(
        PortfolioDbContext context,
        RoleManager<ApplicationRole> roleManager)
    {
        var superAdmin = await roleManager.FindByNameAsync(RoleConstants.SuperAdmin)
                         ?? throw new InvalidOperationException("SuperAdmin role was not found.");

        var permissions = await context.Permissions.AsNoTracking().ToListAsync();
        await EnsureRolePermissionsAsync(context, superAdmin.Id, permissions.Select(x => x.Id));

        var rolePermissionMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [RoleConstants.Admin] = new[]
            {
                PermissionConstants.DashboardView,
                PermissionConstants.CompanyView, PermissionConstants.CompanyEdit,
                PermissionConstants.ProductsView, PermissionConstants.ProductsCreate, PermissionConstants.ProductsEdit, PermissionConstants.ProductsDelete,
                PermissionConstants.ProductImagesView, PermissionConstants.ProductImagesCreate, PermissionConstants.ProductImagesDelete,
                PermissionConstants.ServicesView, PermissionConstants.ServicesCreate, PermissionConstants.ServicesEdit, PermissionConstants.ServicesDelete,
                PermissionConstants.PagesView, PermissionConstants.PagesCreate, PermissionConstants.PagesEdit, PermissionConstants.PagesDelete,
                PermissionConstants.SectionsView, PermissionConstants.SectionsCreate, PermissionConstants.SectionsEdit, PermissionConstants.SectionsDelete,
                PermissionConstants.AppearanceView, PermissionConstants.AppearanceEdit,
                PermissionConstants.MessagesView, PermissionConstants.MessagesDelete, PermissionConstants.MessagesReply,
                PermissionConstants.AnalyticsView,
                PermissionConstants.UsersView, PermissionConstants.UsersCreate, PermissionConstants.UsersEdit, PermissionConstants.UsersDelete,
                PermissionConstants.AuditLogsView,
                PermissionConstants.SettingsView, PermissionConstants.SettingsEdit,
                PermissionConstants.SettingsLocalization, PermissionConstants.SettingsDarkMode
            },
            [RoleConstants.ContentManager] = new[]
            {
                PermissionConstants.DashboardView,
                PermissionConstants.CompanyView, PermissionConstants.CompanyEdit,
                PermissionConstants.ProductsView, PermissionConstants.ProductsCreate, PermissionConstants.ProductsEdit, PermissionConstants.ProductsDelete,
                PermissionConstants.ProductImagesView, PermissionConstants.ProductImagesCreate, PermissionConstants.ProductImagesDelete,
                PermissionConstants.ServicesView, PermissionConstants.ServicesCreate, PermissionConstants.ServicesEdit, PermissionConstants.ServicesDelete,
                PermissionConstants.PagesView, PermissionConstants.PagesCreate, PermissionConstants.PagesEdit, PermissionConstants.PagesDelete,
                PermissionConstants.SectionsView, PermissionConstants.SectionsCreate, PermissionConstants.SectionsEdit, PermissionConstants.SectionsDelete,
                PermissionConstants.AppearanceView, PermissionConstants.AppearanceEdit
            },
            [RoleConstants.Support] = new[]
            {
                PermissionConstants.DashboardView,
                PermissionConstants.MessagesView, PermissionConstants.MessagesDelete, PermissionConstants.MessagesReply
            }
        };

        foreach (var pair in rolePermissionMap)
        {
            var role = await roleManager.FindByNameAsync(pair.Key);
            if (role is null) continue;
            var selected = pair.Value.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var permissionIds = permissions
                .Where(x => selected.Contains(x.Name))
                .Select(x => x.Id)
                .ToList();
            await EnsureRolePermissionsAsync(context, role.Id, permissionIds);
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureRolePermissionsAsync(PortfolioDbContext context, Guid roleId, IEnumerable<Guid> permissionIds)
    {
        var ids = permissionIds.Distinct().ToHashSet();
        var current = await context.RolePermissions
            .Where(x => x.RoleId == roleId)
            .Select(x => x.PermissionId)
            .ToListAsync();

        foreach (var permissionId in ids)
        {
            if (!current.Contains(permissionId))
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });
            }
        }
    }

    private static async Task SeedSuperAdminAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        var user = await userManager.FindByEmailAsync(AdminEmail);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = AdminEmail,
                Email = AdminEmail,
                FullName = "System Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, AdminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to create SuperAdmin: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, RoleConstants.SuperAdmin))
        {
            var result = await userManager.AddToRoleAsync(user, RoleConstants.SuperAdmin);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to assign SuperAdmin role: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static async Task SeedWebsiteDataAsync(PortfolioDbContext context)
    {
        var seeded = await context.WebsiteSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == ContentVersionKey);

        if (seeded?.Value == ContentVersion)
            return;

        var now = DateTime.UtcNow;

        var company = await context.Companies.FirstOrDefaultAsync();
        if (company is null)
        {
            company = new Company { Id = Guid.NewGuid(), CreatedAt = now };
            context.Companies.Add(company);
        }

        company.Name = "MSA GOLD & JEWELLERY TRADING L.L.C";
        company.Description = "MSA GOLD & JEWELLERY TRADING L.L.C is a Dubai-based wholesale precious metals enterprise operating directly from the historic Deira Gold Souq. Founded in 2025, the company combines deep regional bullion market expertise with high-purity refining standards, transparent trade settlement, and reliable wholesale distribution.";
        company.Logo = "/img/brand/msa-logo.svg";
        company.Favicon = "/img/brand/msa-mark.svg";
        company.Email = "msagold0@gmail.com";
        company.Address = "Office 302, Hind Plaza-6A, Gold Souq, Deira, Dubai, UAE";
        company.WorkingHours = "Monday - Saturday, 9:00 AM - 6:00 PM";
        company.UpdatedAt = now;

        await context.SaveChangesAsync();

        var oldPhones = await context.CompanyPhones.Where(x => x.CompanyId == company.Id).ToListAsync();
        context.CompanyPhones.RemoveRange(oldPhones);
        context.CompanyPhones.AddRange(
            new CompanyPhone { Id = Guid.NewGuid(), CompanyId = company.Id, PhoneNumber = "+971 4 345 0109", Label = "Landline", IsWhatsApp = false, IsActive = true, SortOrder = 1 },
            new CompanyPhone { Id = Guid.NewGuid(), CompanyId = company.Id, PhoneNumber = "+971 56 418 4546", Label = "Mobile / WhatsApp", IsWhatsApp = true, IsActive = true, SortOrder = 2 });

        var oldSocials = await context.SocialLinks.Where(x => x.CompanyId == company.Id).ToListAsync();
        context.SocialLinks.RemoveRange(oldSocials);
        context.SocialLinks.AddRange(
            new SocialLink { Id = Guid.NewGuid(), CompanyId = company.Id, Platform = "Instagram", Url = "https://www.instagram.com/", Icon = "instagram", IsActive = true, SortOrder = 1 },
            new SocialLink { Id = Guid.NewGuid(), CompanyId = company.Id, Platform = "WhatsApp", Url = "https://wa.me/971564184546", Icon = "whatsapp", IsActive = true, SortOrder = 2 },
            new SocialLink { Id = Guid.NewGuid(), CompanyId = company.Id, Platform = "LinkedIn", Url = "https://www.linkedin.com/", Icon = "linkedin", IsActive = true, SortOrder = 3 },
            new SocialLink { Id = Guid.NewGuid(), CompanyId = company.Id, Platform = "YouTube", Url = "https://www.youtube.com/", Icon = "youtube", IsActive = true, SortOrder = 4 });

        var theme = await context.ThemeSettings.FirstOrDefaultAsync();
        if (theme is null)
        {
            theme = new ThemeSetting { Id = Guid.NewGuid() };
            context.ThemeSettings.Add(theme);
        }
        theme.PrimaryColor = "#C88A24";
        theme.SecondaryColor = "#061827";
        theme.AccentColor = "#D6A84F";
        theme.BodyColor = "#081A29";
        theme.HeadingColor = "#FFFFFF";
        theme.FontFamily = "Inter";
        theme.HeadingFont = "Poppins";
        theme.BorderRadius = 14;
        theme.ButtonRadius = 8;
        theme.ContainerWidth = 1240;
        theme.EnableAnimations = true;

        UpsertSetting(context, "WebsiteTitle", "MSA GOLD & JEWELLERY TRADING L.L.C");
        UpsertSetting(context, "WebsiteDescription", "Dubai-based wholesale precious metals trading, bullion supply, refining, assaying and trade settlement from Deira Gold Souq.");
        UpsertSetting(context, "DefaultSeoTitle", "MSA Gold & Jewellery Trading L.L.C | Dubai Gold & Bullion");
        UpsertSetting(context, "DefaultSeoDescription", "Wholesale fine gold, commercial trade gold, silver bullion, jewellery metals, refining, assaying and physical trade settlement from Dubai's Deira Gold Souq.");
        UpsertSetting(context, "ContactEmail", "msagold0@gmail.com");
        UpsertSetting(context, "Domain", "www.msagold.ae");
        UpsertSetting(context, "EstablishedYear", "2025");
        UpsertSetting(context, "LocationShort", "Deira Gold Souq, Dubai, UAE");
        UpsertSetting(context, "Tagline", "Trusted & Transparent Precious Metals Trading");
        UpsertSetting(context, ContentVersionKey, ContentVersion);

        var pages = await UpsertPagesAsync(context, now);

        // Replace only the initial/demo catalog created by the previous seed.
        var currentProducts = await context.Products.Include(x => x.Images).ToListAsync();
        var productNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Signature Product", "Premium Package", "Fine Gold Bullion", "Commercial Trade Gold", "Silver Bullion"
        };
        var productRemovals = currentProducts.Where(x => productNames.Contains(x.Name)).ToList();
        foreach (var product in productRemovals)
        {
            foreach (var image in product.Images)
                context.ProductImages.Remove(image);
            context.Products.Remove(product);
        }

        context.Products.AddRange(
            NewProduct("Fine Gold Bullion", "Minted and cast gold bars at 999.9 fine gold investment-grade purity.", "/img/products/fine-gold.svg", 1),
            NewProduct("Commercial Trade Gold", "Standard commercial kilobars and trade units at 995 fine gold purity.", "/img/products/commercial-gold.svg", 2),
            NewProduct("Silver Bullion", "High-purity silver bars, casting grain and industrial silver shot.", "/img/products/silver.svg", 3));

        var currentServices = await context.Services.ToListAsync();
        var oldServiceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Strategy & Consulting", "Digital Experience", "Ongoing Support", "Gold Jewelry Solutions", "Technical Refining & Assaying", "Trade Settlement Desk"
        };
        context.Services.RemoveRange(currentServices.Where(x => oldServiceNames.Contains(x.Title)));
        context.Services.AddRange(
            NewService("Gold Jewelry Solutions", "Wholesale supply, trade, and custom gold metal alloys tailored for jewelry workshops and retailers.", "jewelry", "/img/services/jewelry.svg", 1),
            NewService("Technical Refining & Assaying", "High-purity chemical refining solutions reaching up to 999.9 standards, supported by precise assay testing procedures.", "refining", "/img/services/refining.svg", 2),
            NewService("Trade Settlement Desk", "Direct market execution, physical bullion delivery, and competitive spot pricing handled directly from our Deira Gold Souq office.", "trade", "/img/services/trade.svg", 3));

        await SeedSectionsAsync(context, pages, now);
        await context.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, Page>> UpsertPagesAsync(PortfolioDbContext context, DateTime now)
    {
        var definitions = new[]
        {
            ("Home", "", "Trusted Precious Metals Partner in Dubai", 1),
            ("About Us", "about", "A trusted name in precious metals", 2),
            ("Products", "products", "Our Precious Metals", 3),
            ("Services", "services", "Specialist Trade & Refining Services", 4),
            ("Contact", "contact", "Connect with MSA", 5)
        };

        var result = new Dictionary<string, Page>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, slug, title, sort) in definitions)
        {
            var page = await context.Pages.FirstOrDefaultAsync(x => x.Slug == slug);
            if (page is null)
            {
                page = new Page { Id = Guid.NewGuid(), Slug = slug, CreatedAt = now };
                context.Pages.Add(page);
            }

            page.Name = name;
            page.Title = title;
            page.MetaTitle = name == "Home" ? "MSA Gold & Jewellery Trading L.L.C | Dubai" : $"{title} | MSA Gold";
            page.MetaDescription = "MSA GOLD & JEWELLERY TRADING L.L.C — wholesale precious metals, bullion trading, refining, assaying and settlement from Deira Gold Souq, Dubai.";
            page.IsPublished = true;
            page.SortOrder = sort;
            page.UpdatedAt = now;
            result[slug] = page;
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static async Task SeedSectionsAsync(PortfolioDbContext context, Dictionary<string, Page> pages, DateTime now)
    {
        var pageIds = pages.Values.Select(x => x.Id).ToList();
        var oldSections = await context.Sections.Where(x => pageIds.Contains(x.PageId)).ToListAsync();
        context.Sections.RemoveRange(oldSections);

        var home = pages[""];
        context.Sections.AddRange(
            new Section { Id = Guid.NewGuid(), PageId = home.Id, SectionType = "Hero", Title = "Trusted Precious Metals Partner in Dubai", Subtitle = "MSA GOLD & JEWELLERY TRADING L.L.C", Description = "High-purity gold and silver bullion, wholesale trading, refining solutions and jewellery metal supply — directly from the heart of Deira Gold Souq.", Image = "/img/home/hero-gold.svg", BackgroundImage = "/img/home/hero-gold.svg", SortOrder = 1, IsVisible = true, Animation = "fade-up", AnimationDuration = 850 },
            new Section { Id = Guid.NewGuid(), PageId = home.Id, SectionType = "TrustStrip", Title = "Built on trust. Driven by purity.", Description = "High Purity 999.9 Gold | Trusted & Transparent Trading | Direct from Deira Gold Souq | Competitive Market Pricing", SortOrder = 2, IsVisible = true, Animation = "fade-up", AnimationDuration = 700 },
            new Section { Id = Guid.NewGuid(), PageId = home.Id, SectionType = "Products", Title = "Our Core Offerings", Subtitle = "Premium bullion for global markets", Description = "Investment-grade bullion products designed for wholesalers, retailers and professional market participants.", SortOrder = 3, IsVisible = true, Animation = "zoom-in", AnimationDuration = 800 },
            new Section { Id = Guid.NewGuid(), PageId = home.Id, SectionType = "About", Title = "A Trusted Name in Precious Metals", Subtitle = "About MSA", Description = "Founded in 2025, MSA combines deep regional bullion market expertise with high-purity refining standards, transparent trade settlement and reliable wholesale distribution from Dubai's historic Gold Souq.", Image = "/img/home/gold-souq.svg", SortOrder = 4, IsVisible = true, Animation = "fade-up", AnimationDuration = 800 },
            new Section { Id = Guid.NewGuid(), PageId = home.Id, SectionType = "Services", Title = "What We Offer", Subtitle = "Specialist capabilities", Description = "From jewellery metal solutions to technical refining and trade settlement, our team supports the full precious metals supply chain.", SortOrder = 5, IsVisible = true, Animation = "fade-up", AnimationDuration = 800 },
            new Section { Id = Guid.NewGuid(), PageId = home.Id, SectionType = "Stats", Title = "The MSA Difference", Subtitle = "Our foundations", Description = "2025 — Established | Dubai, UAE — Gold Souq | 999.9 — Fine Gold Purity | Worldwide — Trusted Distribution", SortOrder = 6, IsVisible = true, Animation = "scale", AnimationDuration = 700 },
            new Section { Id = Guid.NewGuid(), PageId = home.Id, SectionType = "CTA", Title = "Ready to trade with confidence?", Subtitle = "Let's talk precious metals", Description = "Secure your investment and supply requirements with a trusted partner in the heart of Dubai.", SortOrder = 7, IsVisible = true, Animation = "fade-up", AnimationDuration = 700 },
            new Section { Id = Guid.NewGuid(), PageId = home.Id, SectionType = "Contact", Title = "Get in Touch", Subtitle = "Talk to our team", Description = "Send us an inquiry, request, complaint or partnership message and our team will follow up.", SortOrder = 8, IsVisible = true, Animation = "fade-up", AnimationDuration = 800 }
        );

        var about = pages["about"];
        context.Sections.AddRange(
            new Section { Id = Guid.NewGuid(), PageId = about.Id, SectionType = "Hero", Title = "A Trusted Name in Precious Metals", Subtitle = "Since 2025 · Dubai, UAE", Description = "Wholesale precious metals expertise grounded in transparency, reliability and high-purity standards.", Image = "/img/home/gold-souq.svg", BackgroundImage = "/img/home/gold-souq.svg", SortOrder = 1, IsVisible = true, Animation = "fade-up", AnimationDuration = 850 },
            new Section { Id = Guid.NewGuid(), PageId = about.Id, SectionType = "About", Title = "Operating from the Heart of Deira Gold Souq", Subtitle = "Company Overview", Description = "MSA GOLD & JEWELLERY TRADING L.L.C is a Dubai-based wholesale precious metals enterprise operating directly from the historic Deira Gold Souq. Our model combines regional market knowledge with disciplined refining, assay testing, physical delivery and transparent trade settlement.", Image = "/img/home/gold-souq.svg", SortOrder = 2, IsVisible = true, Animation = "fade-left", AnimationDuration = 800 },
            new Section { Id = Guid.NewGuid(), PageId = about.Id, SectionType = "Stats", Title = "Experience the MSA Standard", Subtitle = "Our foundations", Description = "Established 2025 | Dubai Gold Souq | Fine Gold up to 999.9 | Reliable Wholesale Distribution", SortOrder = 3, IsVisible = true, Animation = "zoom-in", AnimationDuration = 750 },
            new Section { Id = Guid.NewGuid(), PageId = about.Id, SectionType = "CTA", Title = "Build your next trade with MSA", Subtitle = "Get in touch", Description = "Discuss bullion supply, refining requirements or physical settlement with our team.", SortOrder = 4, IsVisible = true, Animation = "fade-up", AnimationDuration = 700 }
        );

        var products = pages["products"];
        context.Sections.AddRange(
            new Section { Id = Guid.NewGuid(), PageId = products.Id, SectionType = "Hero", Title = "Precious Metals for Professional Markets", Subtitle = "Gold & Silver Bullion", Description = "Reliable wholesale bullion supply across fine gold, commercial trade gold and high-purity silver products.", Image = "/img/products/fine-gold.svg", BackgroundImage = "/img/home/hero-gold.svg", SortOrder = 1, IsVisible = true, Animation = "fade-up", AnimationDuration = 850 },
            new Section { Id = Guid.NewGuid(), PageId = products.Id, SectionType = "Products", Title = "Our Bullion Range", Subtitle = "Wholesale products", Description = "Select a product to discuss availability, delivery and current trade requirements.", SortOrder = 2, IsVisible = true, Animation = "zoom-in", AnimationDuration = 800 },
            new Section { Id = Guid.NewGuid(), PageId = products.Id, SectionType = "CTA", Title = "Need a specific bullion requirement?", Subtitle = "Request a quotation", Description = "Contact MSA for product availability, quantities and settlement options.", SortOrder = 3, IsVisible = true, Animation = "fade-up", AnimationDuration = 700 }
        );

        var services = pages["services"];
        context.Sections.AddRange(
            new Section { Id = Guid.NewGuid(), PageId = services.Id, SectionType = "Hero", Title = "Specialist Trade & Refining Services", Subtitle = "Built for the precious metals supply chain", Description = "Professional jewellery metal solutions, technical refining and assaying, and direct trade settlement from Dubai.", Image = "/img/services/refining.svg", BackgroundImage = "/img/home/hero-gold.svg", SortOrder = 1, IsVisible = true, Animation = "fade-up", AnimationDuration = 850 },
            new Section { Id = Guid.NewGuid(), PageId = services.Id, SectionType = "Services", Title = "Our Services", Subtitle = "Where expertise meets execution", Description = "Focused capabilities for workshops, retailers, wholesalers and professional trading partners.", SortOrder = 2, IsVisible = true, Animation = "fade-up", AnimationDuration = 800 },
            new Section { Id = Guid.NewGuid(), PageId = services.Id, SectionType = "CTA", Title = "Speak with our trading desk", Subtitle = "Start a conversation", Description = "Let's discuss your supply, refining or settlement requirements.", SortOrder = 3, IsVisible = true, Animation = "fade-up", AnimationDuration = 700 }
        );

        var contact = pages["contact"];
        context.Sections.AddRange(
            new Section { Id = Guid.NewGuid(), PageId = contact.Id, SectionType = "Hero", Title = "Get in Touch", Subtitle = "Deira Gold Souq · Dubai", Description = "For inquiries, partnerships, requests or complaints, reach out to our team.", Image = "/img/home/gold-souq.svg", BackgroundImage = "/img/home/gold-souq.svg", SortOrder = 1, IsVisible = true, Animation = "fade-up", AnimationDuration = 850 },
            new Section { Id = Guid.NewGuid(), PageId = contact.Id, SectionType = "Contact", Title = "Let's Connect", Subtitle = "Talk to MSA", Description = "Tell us what you need and our team will respond as soon as possible.", SortOrder = 2, IsVisible = true, Animation = "fade-up", AnimationDuration = 800 },
            new Section { Id = Guid.NewGuid(), PageId = contact.Id, SectionType = "CTA", Title = "Ready to trade with confidence?", Subtitle = "MSA GOLD & JEWELLERY TRADING L.L.C", Description = "A trusted partner for precious metals in the heart of Dubai.", SortOrder = 3, IsVisible = true, Animation = "fade-up", AnimationDuration = 700 }
        );
    }

    private static Product NewProduct(string name, string description, string image, int sortOrder) =>
        new()
        {
            Id = Guid.NewGuid(), Name = name, Description = description, Price = 0, Currency = "AED", IsActive = true,
            SortOrder = sortOrder, CreatedAt = DateTime.UtcNow,
            Images = { new ProductImage { Id = Guid.NewGuid(), ImagePath = image, IsMain = true, SortOrder = 1 } }
        };

    private static Service NewService(string title, string description, string icon, string image, int sortOrder) =>
        new()
        {
            Id = Guid.NewGuid(), Title = title, Description = description, Icon = icon, Image = image, IsActive = true,
            SortOrder = sortOrder, CreatedAt = DateTime.UtcNow
        };

    private static void UpsertSetting(PortfolioDbContext context, string key, string value)
    {
        var setting = context.WebsiteSettings.Local.FirstOrDefault(x => x.Key == key);
        if (setting is null)
        {
            setting = context.WebsiteSettings.Local.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        if (setting is null)
        {
            setting = context.WebsiteSettings.FirstOrDefault(x => x.Key == key);
        }

        if (setting is null)
        {
            context.WebsiteSettings.Add(new WebsiteSetting { Id = Guid.NewGuid(), Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
        }
    }
}
