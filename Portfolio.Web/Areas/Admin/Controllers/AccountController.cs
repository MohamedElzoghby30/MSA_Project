using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Business.DTOs;
using Portfolio.Business.Interfaces;
using Portfolio.Data.Identity;
using Portfolio.Web.Areas.Admin.Models;

namespace Portfolio.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class AccountController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IAuditLogService auditLogs,
    IConfiguration configuration) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.FindByEmailAsync(model.Email);

        if (user is null || !user.IsActive)
        {
            await WriteLoginAuditAsync(null, "Login Failed", 401, model.Email);

            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");

            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            await WriteLoginAuditAsync(user, "Login", 200, null);

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(
                "Index",
                "Dashboard",
                new { area = "Admin" });
        }

        if (result.IsLockedOut)
        {
            await WriteLoginAuditAsync(user, "Login Locked", 423, null);

            ModelState.AddModelError(
                string.Empty,
                "Your account has been temporarily locked due to multiple failed login attempts.");
        }
        else
        {
            await WriteLoginAuditAsync(user, "Login Failed", 401, null);

            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");
        }

        return View(model);
    }

    // =========================================================
    // EMERGENCY PASSWORD RESET
    // =========================================================

    [AllowAnonymous]
    [HttpGet]
    public IActionResult EmergencyReset()
    {
        return View(new EmergencyResetViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyReset(
        EmergencyResetViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var masterPassword =
            configuration["EmergencyReset:MasterPassword"];

        // Check emergency key
        if (string.IsNullOrWhiteSpace(masterPassword) ||
            !string.Equals(
                model.EmergencyKey,
                masterPassword,
                StringComparison.Ordinal))
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid emergency key.");

            return View(model);
        }

        // Find user
        var user =
            await userManager.FindByEmailAsync(model.Email);

        if (user is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "No user was found with this email address.");

            return View(model);
        }

        // Generate reset token
        var token =
            await userManager.GeneratePasswordResetTokenAsync(user);

        // Reset password
        var resetResult =
            await userManager.ResetPasswordAsync(
                user,
                token,
                model.NewPassword);

        if (!resetResult.Succeeded)
        {
            foreach (var error in resetResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        // Unlock account
        await userManager.SetLockoutEndDateAsync(
            user,
            null);

        // Reset failed login attempts
        await userManager.ResetAccessFailedCountAsync(
            user);

        await WriteLoginAuditAsync(
            user,
            "Emergency Password Reset",
            200,
            null);

        TempData["Success"] =
            "Password reset successfully. You can now sign in with the new password.";

        return RedirectToAction(
            nameof(Login));
    }

    // =========================================================
    // LOGOUT
    // =========================================================

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var user = await userManager.GetUserAsync(User);

        await WriteLoginAuditAsync(
            user,
            "Logout",
            200,
            null);

        await signInManager.SignOutAsync();

        return RedirectToAction(
            "Login",
            "Account",
            new { area = "Admin" });
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();

    private async Task WriteLoginAuditAsync(
        ApplicationUser? user,
        string action,
        int statusCode,
        string? attemptedEmail)
    {
        await auditLogs.CreateAsync(new AuditLogDto
        {
            UserId = user?.Id,
            UserName = user?.Email ?? attemptedEmail ?? "Anonymous",
            ActionType = "Authentication",
            Area = "Admin",
            Controller = "Account",
            Action = action,
            Path = Request.Path.Value ?? string.Empty,
            HttpMethod = Request.Method,
            StatusCode = statusCode,
            IpAddress =
                HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent =
                Request.Headers.UserAgent.ToString(),
            OccurredAtUtc = DateTime.UtcNow
        });
    }
}