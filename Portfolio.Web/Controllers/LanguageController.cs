using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.Interfaces;
using Portfolio.Web.Localization;

namespace Portfolio.Web.Controllers;

public class LanguageController(IWebsiteSettingService settings) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Set(
        string culture,
        string? returnUrl = null,
        CancellationToken ct = default)
    {
        var values = await settings.GetAllAsync(ct);

        var enabled = true;

        if (values.TryGetValue(
                "PublicLocalizationEnabled",
                out var raw) &&
            bool.TryParse(raw, out var parsed))
        {
            enabled = parsed;
        }

        var isAdmin = returnUrl?.StartsWith(
            "/Admin",
            StringComparison.OrdinalIgnoreCase) == true;

        if (!enabled && !isAdmin)
            culture = "en";

        if (culture is not ("en" or "ar"))
            culture = "en";

        var cookieName = isAdmin
            ? CultureCookieNames.Admin
            : CultureCookieNames.Public;

        Response.Cookies.Append(
            cookieName,
            culture,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                HttpOnly = false,
                SameSite = SameSiteMode.Lax
            });

        // Retire the old shared ASP.NET Core culture cookie so it cannot
        // conflict with the new independent public/admin language preferences.
        Response.Cookies.Delete(
            CookieRequestCultureProvider.DefaultCookieName);

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(
            "Index",
            "Home");
    }
}
