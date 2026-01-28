using System.Text.RegularExpressions;

namespace TaskManagement.Application.Common;

/// <summary>
/// Utility class for sanitizing user inputs to prevent XSS and injection attacks
/// </summary>
public static class InputSanitizer
{
    private static readonly Regex DangerousHtmlPattern = new(
        @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>|<iframe\b[^<]*(?:(?!<\/iframe>)<[^<]*)*<\/iframe>|javascript:|on\w+\s*=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Sanitizes a string by removing potentially dangerous HTML/script content
    /// </summary>
    /// <param name="input">The input string to sanitize</param>
    /// <returns>Sanitized string with dangerous content removed</returns>
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove dangerous HTML/script patterns
        var sanitized = DangerousHtmlPattern.Replace(input, string.Empty);
        
        // Trim whitespace
        return sanitized.Trim();
    }

    /// <summary>
    /// Validates that a string doesn't contain dangerous content
    /// </summary>
    /// <param name="input">The input string to validate</param>
    /// <returns>True if safe, false if contains dangerous content</returns>
    public static bool IsSafe(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        return !DangerousHtmlPattern.IsMatch(input);
    }

    /// <summary>
    /// Sanitizes and validates input, returning empty string if dangerous
    /// </summary>
    /// <param name="input">The input string to sanitize and validate</param>
    /// <returns>Sanitized string if safe, empty string if dangerous</returns>
    public static string SanitizeAndValidate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        if (!IsSafe(input))
        {
            // Log warning in production (would need ILogger injected)
            return string.Empty;
        }

        return Sanitize(input);
    }
}
