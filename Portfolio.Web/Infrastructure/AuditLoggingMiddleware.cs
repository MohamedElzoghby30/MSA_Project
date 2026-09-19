using System.Text.Json;
using System.Security.Claims;
using Portfolio.Business.DTOs;
using Portfolio.Business.Interfaces;

namespace Portfolio.Web.Infrastructure;

public sealed class AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<AuditLoggingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogs)
    {
        if (!context.Request.Path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            try
            {
                await WriteAuditAsync(context, auditLogs, 500, ex.GetType().Name);
            }
            catch (Exception auditException)
            {
                _logger.LogError(auditException, "Unable to persist failure audit log for {Path}", context.Request.Path);
            }
            _logger.LogError(ex, "Unhandled exception while processing admin request {Path}", context.Request.Path);
            throw;
        }

        try
        {
            await WriteAuditAsync(context, auditLogs, context.Response.StatusCode, null);
        }
        catch (Exception auditException)
        {
            _logger.LogError(auditException, "Unable to persist audit log for {Path}", context.Request.Path);
        }
    }

    private static async Task WriteAuditAsync(
        HttpContext context,
        IAuditLogService auditLogs,
        int statusCode,
        string? exceptionType)
    {
        var controller = context.Request.RouteValues.TryGetValue("controller", out var controllerValue)
            ? controllerValue?.ToString() ?? string.Empty
            : string.Empty;

        var action = context.Request.RouteValues.TryGetValue("action", out var actionValue)
            ? actionValue?.ToString() ?? string.Empty
            : string.Empty;

        var entityId = FindEntityId(context);
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? context.User.FindFirst("sub")?.Value;
        var userId = Guid.TryParse(userIdClaim, out var parsedUserId)
            ? parsedUserId
            : (Guid?)null;

        var userName = context.User.Identity?.IsAuthenticated == true
            ? context.User.Identity?.Name
              ?? context.User.FindFirst("email")?.Value
              ?? "Authenticated User"
            : "Anonymous";

        var ip = context.Connection.RemoteIpAddress?.ToString();
        var details = new Dictionary<string, string?>
        {
            ["StatusCode"] = statusCode.ToString(),
            ["Endpoint"] = context.GetEndpoint()?.DisplayName,
            ["EntityId"] = entityId,
            ["Exception"] = exceptionType
        };

        var dto = new AuditLogDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserName = userName ?? "Anonymous",
            ActionType = context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)
                ? "View"
                : context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                    ? "Command"
                    : context.Request.Method,
            Area = "Admin",
            Controller = controller,
            Action = action,
            EntityId = entityId,
            Path = context.Request.Path.Value ?? string.Empty,
            HttpMethod = context.Request.Method,
            StatusCode = statusCode,
            IpAddress = ip,
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            Details = JsonSerializer.Serialize(details),
            OccurredAtUtc = DateTime.UtcNow
        };

        await auditLogs.CreateAsync(dto);
    }

    private static string? FindEntityId(HttpContext context)
    {
        var preferredKeys = new[] { "id", "entityId", "productId", "pageId", "sectionId", "imageId", "userId", "roleId", "companyId" };

        foreach (var key in preferredKeys)
        {
            if (context.Request.RouteValues.TryGetValue(key, out var value) && value is not null)
            {
                var text = value.ToString();
                if (!string.IsNullOrWhiteSpace(text)) return text;
            }
        }

        return null;
    }
}
