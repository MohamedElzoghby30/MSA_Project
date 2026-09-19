using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.DTOs;
using Portfolio.Business.Interfaces;
using Portfolio.Web.Models;

namespace Portfolio.Web.Controllers;

[Route("contact")]
public class ContactController(IContactMessageService messages) : Controller
{
    private static readonly HashSet<string> AllowedMessageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "General Inquiry",
        "Price Request",
        "Partnership",
        "Complaint",
        "Support",
        "Other"
    };

    [HttpPost("send")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(ContactFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["ContactError"] = "Please check the form fields and try again.";
            return Redirect("/contact#contact");
        }

        if (!AllowedMessageTypes.Contains(model.MessageType))
        {
            TempData["ContactError"] = "Please select a valid message type.";
            return Redirect("/contact#contact");
        }

        await messages.CreateAsync(new MessageDto
        {
            Name = model.Name.Trim(),
            Email = model.Email.Trim(),
            Country = model.Country?.Trim(),
            Phone = model.Phone?.Trim(),
            Subject = model.Subject?.Trim(),
            MessageType = model.MessageType,
            Message = model.Message.Trim()
        }, ct);

        TempData["ContactSuccess"] = "Thanks. Your message has been sent successfully.";
        return Redirect("/contact#contact");
    }
}
