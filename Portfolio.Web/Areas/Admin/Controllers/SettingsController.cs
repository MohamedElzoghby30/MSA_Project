using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.Interfaces;
using Portfolio.Web.Localization;
using Portfolio.Data.Seed;
using Portfolio.Web.Infrastructure;

namespace Portfolio.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
[PermissionAuthorize(PermissionConstants.SettingsView)]
public class SettingsController(
    IWebsiteSettingService settings,
    IPermissionService permissions) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewBag.CanEditGeneral = await permissions.HasAsync(User, PermissionConstants.SettingsEdit, ct);
        ViewBag.CanEditLocalization = await permissions.HasAsync(User, PermissionConstants.SettingsLocalization, ct);
        ViewBag.CanEditDarkMode = await permissions.HasAsync(User, PermissionConstants.SettingsDarkMode, ct);

        var values = await settings.GetAllAsync(ct);
        return View(values);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(PermissionConstants.SettingsEdit)]
    public async Task<IActionResult> Save(
        Dictionary<string, string> values,
        CancellationToken ct)
    {
        await settings.SaveAsync(values, ct);
        TempData["Success"] = "Settings updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(PermissionConstants.SettingsLocalization)]
    public async Task<IActionResult> SaveLocalization(
        bool enabled,
        string? defaultLanguage,
        CancellationToken ct)
    {
        defaultLanguage = defaultLanguage is "ar" ? "ar" : "en";

        await settings.SaveAsync(new Dictionary<string, string>
        {
            ["PublicLocalizationEnabled"] = enabled.ToString(),
            ["PublicDefaultLanguage"] = defaultLanguage
        }, ct);

        // Clear only the public-language preference for this browser.
        // The Admin language remains independent. This makes a newly selected
        // public default effective immediately on the next public request.
        Response.Cookies.Delete(CultureCookieNames.Public);
        Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);

        TempData["Success"] = enabled
            ? "Website translation is enabled."
            : "Website translation is disabled.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(PermissionConstants.SettingsDarkMode)]
    public async Task<IActionResult> SaveDarkMode(
        bool enabled,
        CancellationToken ct)
    {
        await settings.SaveAsync(new Dictionary<string, string>
        {
            ["PublicDarkModeEnabled"] = enabled.ToString()
        }, ct);

        TempData["Success"] = enabled
            ? "Website dark mode is enabled."
            : "Website dark mode is disabled.";

        return RedirectToAction(nameof(Index));
    }
}
