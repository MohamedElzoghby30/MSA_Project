using Portfolio.Business.DTOs;

namespace Portfolio.Web.Models;

public class PublicPageViewModel
{
    public PageEditDto Page { get; set; } = new();
    public List<SectionEditDto> Sections { get; set; } = [];
    public CompanyDto Company { get; set; } = new();
    public ThemeDto Theme { get; set; } = new();
    public List<ProductListDto> Products { get; set; } = [];
    public List<ServiceDto> Services { get; set; } = [];
    public Dictionary<string, string> Settings { get; set; } = new();
}

public class ContactFormModel
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(150)]
    public string Name { get; set; } = "";

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    [System.ComponentModel.DataAnnotations.StringLength(200)]
    public string Email { get; set; } = "";

    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string? Country { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(50)]
    public string? Phone { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(250)]
    public string? Subject { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(50)]
    public string MessageType { get; set; } = "General Inquiry";

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(5000)]
    public string Message { get; set; } = "";
}
