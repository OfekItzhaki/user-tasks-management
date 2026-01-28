/**
 * Security utilities for XSS protection and HTML encoding
 */

/**
 * Encodes HTML special characters to prevent XSS attacks
 * @param text - The text to encode
 * @returns HTML-encoded string
 */
export function encodeHtml(text: string | null | undefined): string {
  if (!text) return '';
  
  const map: Record<string, string> = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#x27;',
    '/': '&#x2F;',
  };
  
  return text.replace(/[&<>"'/]/g, (char) => map[char] || char);
}

/**
 * Sanitizes user input by encoding HTML and trimming whitespace
 * @param input - The input to sanitize
 * @returns Sanitized string
 */
export function sanitizeInput(input: string | null | undefined): string {
  if (!input) return '';
  return encodeHtml(input.trim());
}

/**
 * Validates that a string doesn't contain potentially dangerous HTML/script tags
 * @param input - The input to validate
 * @returns true if safe, false if contains dangerous content
 */
export function isSafeString(input: string | null | undefined): boolean {
  if (!input) return true;
  
  const dangerousPatterns = [
    /<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi,
    /<iframe\b[^<]*(?:(?!<\/iframe>)<[^<]*)*<\/iframe>/gi,
    /javascript:/gi,
    /on\w+\s*=/gi, // Event handlers like onclick=
  ];
  
  return !dangerousPatterns.some(pattern => pattern.test(input));
}

/**
 * Sanitizes and validates user input
 * @param input - The input to sanitize and validate
 * @returns Sanitized string if safe, empty string if dangerous
 */
export function sanitizeAndValidate(input: string | null | undefined): string {
  if (!input) return '';
  
  // Check if input is safe first (before encoding)
  if (!isSafeString(input)) {
    console.warn('Potentially dangerous input detected and removed');
    return '';
  }
  
  // If safe, sanitize (encode HTML)
  return sanitizeInput(input);
}
