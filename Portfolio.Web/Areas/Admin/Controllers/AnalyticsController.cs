using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.Interfaces;
using Portfolio.Data.Seed;
using Portfolio.Web.Infrastructure;
namespace Portfolio.Web.Areas.Admin.Controllers;
[Area("Admin"),Authorize,PermissionAuthorize(PermissionConstants.AnalyticsView)]
public class AnalyticsController(IAnalyticsService analytics) : Controller
{ public async Task<IActionResult> Index(int days=30,CancellationToken ct=default)=>View(await analytics.GetAsync(days,ct)); }
