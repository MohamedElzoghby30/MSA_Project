using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using Portfolio.Business.DTOs;using Portfolio.Business.Interfaces;using Portfolio.Data.Seed;using Portfolio.Web.Infrastructure;
namespace Portfolio.Web.Areas.Admin.Controllers;
[Area("Admin"),Authorize,PermissionAuthorize(PermissionConstants.RolesView)] public class RolesController(IRoleManagementService roles):Controller
{
 public async Task<IActionResult> Index(CancellationToken ct)=>View(await roles.ListAsync(ct));
 [HttpGet,PermissionAuthorize(PermissionConstants.RolesCreate)] public async Task<IActionResult>Create(CancellationToken ct){ViewBag.Permissions=await roles.PermissionsAsync(ct);return View(new RoleEditDto());}
 [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.RolesCreate)] public async Task<IActionResult>Create(RoleEditDto model,CancellationToken ct){var r=await roles.SaveAsync(model,ct);if(!r.Success){ModelState.AddModelError("",r.Error);ViewBag.Permissions=await roles.PermissionsAsync(ct);return View(model);}return RedirectToAction(nameof(Index));}
 [HttpGet,PermissionAuthorize(PermissionConstants.RolesEdit)] public async Task<IActionResult>Edit(Guid id,CancellationToken ct){var m=await roles.GetAsync(id,ct);if(m is null)return NotFound();ViewBag.Permissions=await roles.PermissionsAsync(ct);return View(m);}
 [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.RolesEdit)] public async Task<IActionResult>Edit(RoleEditDto model,CancellationToken ct){var r=await roles.SaveAsync(model,ct);if(!r.Success){ModelState.AddModelError("",r.Error);ViewBag.Permissions=await roles.PermissionsAsync(ct);return View(model);}return RedirectToAction(nameof(Index));}
 [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.RolesDelete)] public async Task<IActionResult>Delete(Guid id,CancellationToken ct){await roles.DeleteAsync(id,ct);return RedirectToAction(nameof(Index));}
}
