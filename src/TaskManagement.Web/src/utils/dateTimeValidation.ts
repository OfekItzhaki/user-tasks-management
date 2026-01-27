/**
 * Validation helpers for date and time fields.
 * Useful for task due dates and other date/time inputs.
 */

export interface ValidationResult {
  valid: boolean;
  error?: string;
}

const YYYY_MM_DD = /^\d{4}-\d{2}-\d{2}$/;
const HH_MM = /^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/;

/**
 * Validate date (YYYY-MM-DD). Empty is valid (optional field).
 * Past dates are allowed (e.g. overdue tasks).
 */
export function validateDate(value: string): ValidationResult {
  const trimmed = value.trim();
  if (!trimmed) return { valid: true };

  if (!YYYY_MM_DD.test(trimmed)) {
    return {
      valid: false,
      error: 'Invalid date format. Use YYYY-MM-DD.',
    };
  }

  const date = new Date(trimmed);
  if (Number.isNaN(date.getTime())) {
    return { valid: false, error: 'Invalid date.' };
  }

  const [y, m, d] = trimmed.split('-').map(Number);
  if (date.getUTCFullYear() !== y || date.getUTCMonth() !== m - 1 || date.getUTCDate() !== d) {
    return { valid: false, error: 'Invalid date.' };
  }

  return { valid: true };
}

/**
 * Validate time (HH:mm, 24h).
 */
export function validateTime(value: string): ValidationResult {
  const t = (value || '').trim();
  if (!t) {
    return { valid: false, error: 'Time is required.' };
  }
  if (!HH_MM.test(t)) {
    return { valid: false, error: 'Invalid time. Use HH:mm (e.g. 09:00, 14:30).' };
  }
  return { valid: true };
}

/**
 * Validate date that must be in the future (for reminders, etc.)
 */
export function validateFutureDate(value: string): ValidationResult {
  const trimmed = value.trim();
  if (!trimmed) {
    return { valid: false, error: 'Date is required.' };
  }

  if (!YYYY_MM_DD.test(trimmed)) {
    return {
      valid: false,
      error: 'Invalid date format. Use YYYY-MM-DD.',
    };
  }

  const date = new Date(trimmed);
  if (Number.isNaN(date.getTime())) {
    return { valid: false, error: 'Invalid date.' };
  }

  const [y, m, d] = trimmed.split('-').map(Number);
  if (date.getUTCFullYear() !== y || date.getUTCMonth() !== m - 1 || date.getUTCDate() !== d) {
    return { valid: false, error: 'Invalid date.' };
  }

  const today = new Date();
  today.setHours(0, 0, 0, 0);
  date.setHours(0, 0, 0, 0);
  if (date < today) {
    return { valid: false, error: 'Date cannot be in the past.' };
  }

  return { valid: true };
}

/**
 * Normalize time to HH:mm, or undefined if invalid.
 */
export function normalizeTime(value: string): string | undefined {
  const t = (value || '').trim();
  if (!HH_MM.test(t)) return undefined;
  const [h, m] = t.split(':').map(Number);
  return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`;
}
