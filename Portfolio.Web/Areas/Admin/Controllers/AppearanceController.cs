using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.DTOs;
using Portfolio.Business.Interfaces;
using Portfolio.Data.Seed;
using Portfolio.Web.Infrastructure;
namespace Portfolio.Web.Areas.Admin.Controllers;
[Area("Admin"),Authorize]
public class AppearanceController(IThemeService theme) : Controller
{
    [PermissionAuthorize(PermissionConstants.AppearanceView)] public async Task<IActionResult> Index(CancellationToken ct)=>View(await theme.GetAsync(ct));
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.AppearanceEdit)] public async Task<IActionResult> Save(ThemeDto model,CancellationToken ct){await theme.SaveAsync(model,ct);TempData["Success"]="Appearance updated.";return RedirectToAction(nameof(Index));}
}
