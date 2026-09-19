using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using Portfolio.Business.Interfaces;using Portfolio.Business.DTOs;using Portfolio.Data.Seed;using Portfolio.Web.Infrastructure;
namespace Portfolio.Web.Areas.Admin.Controllers;
[Area("Admin"),Authorize,PermissionAuthorize(PermissionConstants.UsersView)] public class UsersController(IUserManagementService users,IRoleManagementService roles):Controller
{
 public async Task<IActionResult> Index(CancellationToken ct)=>View(await users.ListAsync(ct));
 [HttpGet,PermissionAuthorize(PermissionConstants.UsersCreate)] public async Task<IActionResult>Create(CancellationToken ct){ViewBag.Roles=await roles.ListAsync(ct);return View(new UserEditDto());}
 [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.UsersCreate)] public async Task<IActionResult>Create(UserEditDto model,CancellationToken ct){var r=await users.SaveAsync(model,ct);if(!r.Success){ModelState.AddModelError("",r.Error);ViewBag.Roles=await roles.ListAsync(ct);return View(model);}return RedirectToAction(nameof(Index));}
 [HttpGet,PermissionAuthorize(PermissionConstants.UsersEdit)] public async Task<IActionResult>Edit(Guid id,CancellationToken ct){var m=await users.GetAsync(id,ct);if(m is null)return NotFound();ViewBag.Roles=await roles.ListAsync(ct);return View(m);}
 [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.UsersEdit)] public async Task<IActionResult>Edit(UserEditDto model,CancellationToken ct){var r=await users.SaveAsync(model,ct);if(!r.Success){ModelState.AddModelError("",r.Error);ViewBag.Roles=await roles.ListAsync(ct);return View(model);}return RedirectToAction(nameof(Index));}
 [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.UsersDelete)] public async Task<IActionResult>Delete(Guid id,CancellationToken ct){await users.DeleteAsync(id,ct);return RedirectToAction(nameof(Index));}
}
