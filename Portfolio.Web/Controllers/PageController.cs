using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.Interfaces;
using Portfolio.Web.Models;
namespace Portfolio.Web.Controllers;
public class PageController(IPageService pages,ICompanyService company,IThemeService theme,IProductService products,IServiceService services,IAnalyticsService analytics,IWebsiteSettingService settings) : Controller
{
  public async Task<IActionResult> Index(string slug,CancellationToken ct){var (page,sections)=await pages.GetPublishedBySlugAsync(slug,ct);if(page is null)return NotFound();await analytics.TrackAsync(HttpContext,page.Id,Request.Path,ct);return View("~/Views/Pages/Page.cshtml",new PublicPageViewModel{Page=page,Sections=sections,Company=await company.GetAsync(ct)??new(),Theme=await theme.GetAsync(ct),Products=await products.ListAsync(ct),Services=await services.ListAsync(ct),Settings=await settings.GetAllAsync(ct)});}
}
