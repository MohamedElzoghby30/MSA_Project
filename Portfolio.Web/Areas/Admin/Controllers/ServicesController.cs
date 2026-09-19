using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.Interfaces;
using Portfolio.Data.Seed;
using Portfolio.Web.Areas.Admin.Models;
using Portfolio.Web.Infrastructure;
namespace Portfolio.Web.Areas.Admin.Controllers;
[Area("Admin"), Authorize]
public class ServicesController(IServiceService services) : Controller
{
    [PermissionAuthorize(PermissionConstants.ServicesView)] public async Task<IActionResult> Index(CancellationToken ct)=>View(await services.ListAsync(ct));
    [HttpGet,PermissionAuthorize(PermissionConstants.ServicesCreate)] public IActionResult Create()=>View(new ServiceFormModel());
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.ServicesCreate)] public async Task<IActionResult>Create(ServiceFormModel model,CancellationToken ct){if(!ModelState.IsValid)return View(model);await services.SaveAsync(model,model.ImageFile,ct);return RedirectToAction(nameof(Index));}
    [HttpGet,PermissionAuthorize(PermissionConstants.ServicesEdit)] public async Task<IActionResult>Edit(Guid id,CancellationToken ct){var x=await services.GetAsync(id,ct);return x is null?NotFound():View(x);}
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.ServicesEdit)] public async Task<IActionResult>Edit(ServiceFormModel model,CancellationToken ct){if(!ModelState.IsValid)return View(model);await services.SaveAsync(model,model.ImageFile,ct);return RedirectToAction(nameof(Index));}
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.ServicesDelete)] public async Task<IActionResult>Delete(Guid id,CancellationToken ct){await services.DeleteAsync(id,ct);return RedirectToAction(nameof(Index));}
}
