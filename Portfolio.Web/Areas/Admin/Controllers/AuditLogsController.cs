using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.Interfaces;
using Portfolio.Data.Seed;
using Portfolio.Web.Infrastructure;

namespace Portfolio.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
[PermissionAuthorize(PermissionConstants.AuditLogsView)]
public class AuditLogsController(IAuditLogService auditLogs) : Controller
{
    public async Task<IActionResult> Index(
        string? userName,
        string? action,
        string? ipAddress,
        DateTime? from,
        DateTime? to,
        CancellationToken ct)
    {
        var fromUtc = from?.ToUniversalTime();
        var toUtc = to?.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

        var model = await auditLogs.ListAsync(userName, action, ipAddress, fromUtc, toUtc, 300, ct);
        ViewBag.UserName = userName;
        ViewBag.Action = action;
        ViewBag.IpAddress = ipAddress;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");
        return View(model);
    }
}
