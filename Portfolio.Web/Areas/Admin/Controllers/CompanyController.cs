using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.DTOs;
using Portfolio.Business.Interfaces;
using Portfolio.Data.Seed;
using Portfolio.Web.Areas.Admin.Models;
using Portfolio.Web.Infrastructure;
namespace Portfolio.Web.Areas.Admin.Controllers;
[Area("Admin"), Authorize]
public class CompanyController(ICompanyService company) : Controller
{
    [PermissionAuthorize(PermissionConstants.CompanyView)] public async Task<IActionResult> Index(CancellationToken ct) => View(await company.GetAsync(ct));
    [HttpPost, ValidateAntiForgeryToken, PermissionAuthorize(PermissionConstants.CompanyEdit)]
    public async Task<IActionResult> Save(CompanyFormModel model, CancellationToken ct) { if (!ModelState.IsValid) return View("Index", model); await company.SaveAsync(model, model.LogoFile, model.FaviconFile, ct); TempData["Success"] = "Company information saved."; return RedirectToAction(nameof(Index)); }
    [HttpPost, ValidateAntiForgeryToken, PermissionAuthorize(PermissionConstants.CompanyEdit)] public async Task<IActionResult> SavePhone(Guid companyId, CompanyPhoneDto dto, CancellationToken ct)
    { 
        await company.SavePhoneAsync(companyId,dto,ct); return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, PermissionAuthorize(PermissionConstants.CompanyEdit)] public async Task<IActionResult> DeletePhone(Guid id, CancellationToken ct) { await company.DeletePhoneAsync(id,ct); return RedirectToAction(nameof(Index)); }
    [HttpPost, ValidateAntiForgeryToken, PermissionAuthorize(PermissionConstants.CompanyEdit)] public async Task<IActionResult> SaveSocial(Guid companyId, SocialLinkDto dto, CancellationToken ct) { await company.SaveSocialAsync(companyId,dto,ct); return RedirectToAction(nameof(Index)); }
    [HttpPost, ValidateAntiForgeryToken, PermissionAuthorize(PermissionConstants.CompanyEdit)] public async Task<IActionResult> DeleteSocial(Guid id, CancellationToken ct) { await company.DeleteSocialAsync(id,ct); return RedirectToAction(nameof(Index)); }
}
