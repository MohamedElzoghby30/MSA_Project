using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.Interfaces;
using Portfolio.Data.Seed;
using Portfolio.Web.Infrastructure;
namespace Portfolio.Web.Areas.Admin.Controllers;
[Area("Admin"),Authorize,PermissionAuthorize(PermissionConstants.MessagesView)]
public class MessagesController(IContactMessageService messages) : Controller
{
    public async Task<IActionResult> Index(bool archived=false,CancellationToken ct=default)=>View(await messages.ListAsync(archived,ct));
    public async Task<IActionResult> Details(Guid id,CancellationToken ct){var x=await messages.GetAsync(id,ct);return x is null?NotFound():View(x);}
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.MessagesView)] public async Task<IActionResult> ToggleRead(Guid id,CancellationToken ct){await messages.MarkReadAsync(id,ct);return RedirectToAction(nameof(Index));}
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.MessagesReply)] public async Task<IActionResult> ToggleReplied(Guid id,CancellationToken ct){await messages.ToggleReplyAsync(id,ct);return RedirectToAction(nameof(Index));}
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.MessagesView)] public async Task<IActionResult> Archive(Guid id,CancellationToken ct){await messages.ArchiveAsync(id,ct);return RedirectToAction(nameof(Index));}
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.MessagesDelete)] public async Task<IActionResult> Delete(Guid id,CancellationToken ct){await messages.DeleteAsync(id,ct);return RedirectToAction(nameof(Index));}
}
