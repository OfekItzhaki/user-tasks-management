/**
 * Error handling utilities for .NET API responses.
 * Handles .NET validation error format: { errors: { "Field": ["Error message"] }, title, status }
 */

/**
 * Check if an error message is technical/internal (should not be shown to users)
 */
function isTechnicalError(message: string): boolean {
  const technicalPatterns = [
    /^ReferenceError:/i,
    /^TypeError:/i,
    /^SyntaxError:/i,
    /is not a function/i,
    /is undefined/i,
    /Cannot read property/i,
    /Cannot access/i,
    /Property.*doesn't exist/i,
    /Property.*does not exist/i,
  ];
  
  return technicalPatterns.some(pattern => pattern.test(message));
}

/**
 * Extract error message from .NET API error format
 */
export function extractErrorMessage(error: unknown, defaultMessage: string): string {
  if (!error) return defaultMessage;
  
  try {
    let message: string | null = null;
    
    const dotNetError = error as {
      errors?: Record<string, string[]>;
      title?: string;
      status?: number;
    } | undefined;
    
    if (dotNetError?.errors) {
      const errorMessages = Object.values(dotNetError.errors).flat();
      message = errorMessages.join(', ');
    } else if (dotNetError?.title) {
      message = dotNetError.title;
    } else if (error instanceof Error && error.message) {
      message = error.message;
    } else if (typeof error === 'string') {
      message = error;
    } else if (error && typeof error === 'object' && 'toString' in error) {
      const errorString = String(error);
      if (errorString !== '[object Object]') {
        message = errorString;
      }
    }
    
    if (message && !isTechnicalError(message)) {
      return message;
    }
    
    return defaultMessage;
  } catch {
    // If anything fails, return default message
    return defaultMessage;
  }
}

/**
 * Check if error is an authentication error (401)
 */
export function isAuthError(error: unknown): boolean {
  if (!error) return false;
  
  try {
    const e = error as { status?: number; statusCode?: number; response?: { status?: number } } | null | undefined;
    const statusCode = e?.status ?? e?.statusCode ?? e?.response?.status;
    const message = extractErrorMessage(error, '').toLowerCase();
    
    return statusCode === 401 || message.includes('unauthorized');
  } catch {
    return false;
  }
}

/**
 * Check if error is a timeout error
 */
export function isTimeoutError(error: unknown): boolean {
  if (!error) return false;
  
  try {
    const message = extractErrorMessage(error, '').toLowerCase();
    const apiError = error as { code?: string };
    const code = apiError?.code;
    
    return (
      message.includes('too long') ||
      message.includes('timeout') ||
      code === 'ECONNABORTED'
    );
  } catch {
    return false;
  }
}

/**
 * Show error toast with consistent formatting
 * TODO: Install react-hot-toast or your preferred toast library and uncomment toast.error()
 */
export function showErrorToast(
  error: unknown,
  defaultMessage: string,
): void {
  const message = extractErrorMessage(error, defaultMessage);
  console.error('Error:', message);
  // toast.error(message);
}

/**
 * Handle API errors with automatic auth error detection
 */
export function handleApiError(
  error: unknown,
  defaultMessage: string,
  onAuthError?: () => void,
): void {
  if (isAuthError(error)) {
    if (onAuthError) {
      onAuthError();
    }
    return;
  }
  
  if (isTimeoutError(error)) {
    const timeoutMessage = defaultMessage.includes('timeout') 
      ? defaultMessage 
      : 'The request is taking too long. Please try again later.';
    showErrorToast(error, timeoutMessage);
    return;
  }
  
  showErrorToast(error, defaultMessage);
}

/**
 * Get user-friendly error message for common scenarios
 */
export function getFriendlyErrorMessage(error: unknown, operation: string): string {
  if (isTimeoutError(error)) {
    return `${operation} is taking too long. Please try again later.`;
  }
  
  if (isAuthError(error)) {
    return 'Your session has expired. Please log in again.';
  }
  
  return extractErrorMessage(error, `Unable to ${operation.toLowerCase()}. Please try again.`);
}
