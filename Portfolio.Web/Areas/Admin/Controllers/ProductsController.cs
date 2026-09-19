using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.DTOs;
using Portfolio.Business.Interfaces;
using Portfolio.Data.Seed;
using Portfolio.Web.Areas.Admin.Models;
using Portfolio.Web.Infrastructure;

namespace Portfolio.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class ProductsController(IProductService products) : Controller
{
    [PermissionAuthorize(PermissionConstants.ProductsView)]
    public async Task<IActionResult> Index(CancellationToken ct) => View(await products.ListAsync(ct));

    [HttpGet, PermissionAuthorize(PermissionConstants.ProductsCreate)]
    public IActionResult Create() => View(new ProductFormModel());

    [HttpPost, ValidateAntiForgeryToken, PermissionAuthorize(PermissionConstants.ProductsCreate)]
    public async Task<IActionResult> Create(ProductFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        await products.SaveAsync(model, ct);
        TempData["Success"] = "Product created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, PermissionAuthorize(PermissionConstants.ProductsEdit)]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var model = await products.GetAsync(id, ct);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, PermissionAuthorize(PermissionConstants.ProductsEdit)]
    public async Task<IActionResult> Edit(ProductFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        await products.SaveAsync(model, ct);
        TempData["Success"] = "Product updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, PermissionAuthorize(PermissionConstants.ProductsDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await products.DeleteAsync(id, ct);
        TempData["Success"] = "Product deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, PermissionAuthorize(PermissionConstants.ProductImagesView)]
    public async Task<IActionResult> Images(Guid id, CancellationToken ct)
    {
        var model = await products.GetImagesAsync(id, ct);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, PermissionAuthorize(PermissionConstants.ProductImagesCreate)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken ct)
    {
        var result = await products.AddImageAsync(id, file, ct);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Image uploaded successfully." : result.Error;
        return RedirectToAction(nameof(Images), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, PermissionAuthorize(PermissionConstants.ProductImagesDelete)]
    public async Task<IActionResult> DeleteImage(Guid imageId, Guid productId, CancellationToken ct)
    {
        await products.DeleteImageAsync(imageId, ct);
        TempData["Success"] = "Image deleted successfully.";
        return RedirectToAction(nameof(Images), new { id = productId });
    }

    [HttpPost, ValidateAntiForgeryToken, PermissionAuthorize(PermissionConstants.ProductImagesCreate)]
    public async Task<IActionResult> SetMainImage(Guid imageId, Guid productId, CancellationToken ct)
    {
        await products.SetMainImageAsync(imageId, ct);
        TempData["Success"] = "Main image updated.";
        return RedirectToAction(nameof(Images), new { id = productId });
    }
}
