using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Portfolio.Business.DTOs;

namespace Portfolio.Web.Areas.Admin.Models;

public class CompanyFormModel : CompanyDto
{
    public IFormFile? LogoFile { get; set; }
    public IFormFile? FaviconFile { get; set; }
}
public class ProductFormModel : ProductEditDto { }
public class ServiceFormModel : ServiceDto { public IFormFile? ImageFile { get; set; } }
public class PageFormModel : PageEditDto { }
public class SectionFormModel : SectionEditDto { public IFormFile? ImageFile { get; set; } public IFormFile? BackgroundImageFile { get; set; } }
public class ContactFormModel
{
    [Required, StringLength(150)] public string Name { get; set; } = "";
    [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = "";
    [StringLength(100)] public string? Country { get; set; }
    [StringLength(50)] public string? Phone { get; set; }
    [StringLength(250)] public string? Subject { get; set; }
    public string MessageType { get; set; } = "General Inquiry";
    [Required, StringLength(5000)] public string Message { get; set; } = "";
}
