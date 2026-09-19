using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.Interfaces;
using Portfolio.Data.Seed;
using Portfolio.Web.Infrastructure;
namespace Portfolio.Web.Areas.Admin.Controllers;
[Area("Admin"), Authorize, PermissionAuthorize(PermissionConstants.DashboardView)]
public class DashboardController(IDashboardService dashboard) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) => View(await dashboard.GetAsync(30, ct));
}
