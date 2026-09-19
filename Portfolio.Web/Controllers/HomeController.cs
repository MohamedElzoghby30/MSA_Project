using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.Interfaces;
using Portfolio.Web.Models;
namespace Portfolio.Web.Controllers;
public class HomeController(IPageService pages,ICompanyService company,IThemeService theme,IProductService products,IServiceService services,IAnalyticsService analytics,IWebsiteSettingService settings) : Controller
{
 public async Task<IActionResult> Index(CancellationToken ct){var (page,sections)=await pages.GetPublishedBySlugAsync("",ct);if(page is null)return NotFound();await analytics.TrackAsync(HttpContext,page.Id,"/",ct);return View("~/Views/Pages/Page.cshtml",new PublicPageViewModel{Page=page,Sections=sections,Company=await company.GetAsync(ct)??new(),Theme=await theme.GetAsync(ct),Products=await products.ListAsync(ct),Services=await services.ListAsync(ct),Settings=await settings.GetAllAsync(ct)});}
 [ResponseCache(Duration=0,Location=ResponseCacheLocation.None,NoStore=true)] public IActionResult Error()=>View(new ErrorViewModel{RequestId=System.Diagnostics.Activity.Current?.Id??HttpContext.TraceIdentifier});
}
