using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.Interfaces;
using Portfolio.Business.DTOs;
using Portfolio.Data.Seed;
using Portfolio.Web.Areas.Admin.Models;
using Portfolio.Web.Infrastructure;
namespace Portfolio.Web.Areas.Admin.Controllers;
[Area("Admin"), Authorize]
public class PagesController(IPageService pages) : Controller
{
    [PermissionAuthorize(PermissionConstants.PagesView)] public async Task<IActionResult> Index(CancellationToken ct)=>View(await pages.ListAsync(ct));
    [HttpGet,PermissionAuthorize(PermissionConstants.PagesCreate)] public IActionResult Create()=>View(new PageFormModel());
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.PagesCreate)] public async Task<IActionResult>Create(PageFormModel model,CancellationToken ct){try{if(!ModelState.IsValid)return View(model);await pages.SaveAsync(model,ct);return RedirectToAction(nameof(Index));}catch(Exception ex){ModelState.AddModelError("",ex.Message);return View(model);}}
    [HttpGet,PermissionAuthorize(PermissionConstants.PagesEdit)] public async Task<IActionResult>Edit(Guid id,CancellationToken ct){var x=await pages.GetAsync(id,ct);return x is null?NotFound():View(x);}
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.PagesEdit)] public async Task<IActionResult>Edit(PageFormModel model,CancellationToken ct){try{if(!ModelState.IsValid)return View(model);await pages.SaveAsync(model,ct);return RedirectToAction(nameof(Index));}catch(Exception ex){ModelState.AddModelError("",ex.Message);return View(model);}}
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.PagesDelete)] public async Task<IActionResult>Delete(Guid id,CancellationToken ct){await pages.DeleteAsync(id,ct);return RedirectToAction(nameof(Index));}
    [HttpGet,PermissionAuthorize(PermissionConstants.SectionsView)] public async Task<IActionResult> Sections(Guid pageId,CancellationToken ct){ViewBag.Page=await pages.GetAsync(pageId,ct);return View(await pages.SectionsAsync(pageId,ct));}
    [HttpGet,PermissionAuthorize(PermissionConstants.SectionsCreate)] public async Task<IActionResult>CreateSection(Guid pageId,CancellationToken ct)=>View(new SectionFormModel{PageId=pageId,SortOrder=(await pages.SectionsAsync(pageId,ct)).Count+1});
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.SectionsCreate)] public async Task<IActionResult>CreateSection(SectionFormModel model,CancellationToken ct){try{if(!ModelState.IsValid)return View(model);await pages.SaveSectionAsync(model,model.ImageFile,model.BackgroundImageFile,ct);return RedirectToAction(nameof(Sections),new{pageId=model.PageId});}catch(Exception ex){ModelState.AddModelError("",ex.Message);return View(model);}}
    [HttpGet,PermissionAuthorize(PermissionConstants.SectionsEdit)] public async Task<IActionResult>EditSection(Guid id,CancellationToken ct)
    {
        var x = await pages.GetSectionAsync(id, ct);
        if (x is null) return NotFound();

        var model = new SectionFormModel
        {
            Id = x.Id,
            PageId = x.PageId,
            SectionType = x.SectionType,
            Title = x.Title,
            TitleEn = x.TitleEn,
            TitleAr = x.TitleAr,
            Subtitle = x.Subtitle,
            SubtitleEn = x.SubtitleEn,
            SubtitleAr = x.SubtitleAr,
            Description = x.Description,
            DescriptionEn = x.DescriptionEn,
            DescriptionAr = x.DescriptionAr,
            Image = x.Image,
            BackgroundImage = x.BackgroundImage,
            BackgroundColor = x.BackgroundColor,
            TextColor = x.TextColor,
            SettingsJson = x.SettingsJson,
            SortOrder = x.SortOrder,
            IsVisible = x.IsVisible,
            Animation = x.Animation,
            AnimationDuration = x.AnimationDuration,
            AnimationDelay = x.AnimationDelay
        };

        return View(model);
    }
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.SectionsEdit)] public async Task<IActionResult>EditSection(SectionFormModel model,CancellationToken ct){try{await pages.SaveSectionAsync(model,model.ImageFile,model.BackgroundImageFile,ct);return RedirectToAction(nameof(Sections),new{pageId=model.PageId});}catch(Exception ex){ModelState.AddModelError("",ex.Message);return View(model);}}
    [HttpPost,ValidateAntiForgeryToken,PermissionAuthorize(PermissionConstants.SectionsDelete)] public async Task<IActionResult>DeleteSection(Guid id,Guid pageId,CancellationToken ct){await pages.DeleteSectionAsync(id,ct);return RedirectToAction(nameof(Sections),new{pageId});}
}
