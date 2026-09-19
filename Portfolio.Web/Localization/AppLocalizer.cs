using Microsoft.AspNetCore.Hosting;
using System.Globalization;
using System.Text.Json;

namespace Portfolio.Web.Localization;

public sealed class AppLocalizer
{
    private readonly Dictionary<string, string> arabic;

    public AppLocalizer(IWebHostEnvironment environment)
    {
        var path = Path.Combine(environment.ContentRootPath, "Localization", "ui.ar.json");
        arabic = new(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path)) return;

        try
        {
            var json = File.ReadAllText(path);
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (values is not null)
            {
                foreach (var pair in values)
                    arabic[pair.Key] = pair.Value;
            }
        }
        catch
        {
            // Keep English fallback if the translation file is unavailable.
        }
    }

    public string Get(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text ?? string.Empty;
        if (IsArabic && arabic.TryGetValue(text.Trim(), out var value)) return value;
        return text;
    }

    public bool IsArabic => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase);
    public string Direction => IsArabic ? "rtl" : "ltr";
    public string LanguageCode => IsArabic ? "ar" : "en";
}
