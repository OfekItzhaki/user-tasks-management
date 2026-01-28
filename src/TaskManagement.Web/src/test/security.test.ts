import { describe, it, expect } from 'vitest';
import { encodeHtml, sanitizeInput, isSafeString, sanitizeAndValidate } from '../utils/security';

describe('Security Utilities', () => {
  describe('encodeHtml', () => {
    it('encodes HTML special characters', () => {
      expect(encodeHtml('<script>alert("xss")</script>')).toBe('&lt;script&gt;alert(&quot;xss&quot;)&lt;&#x2F;script&gt;');
      expect(encodeHtml('Hello & World')).toBe('Hello &amp; World');
      expect(encodeHtml("It's working")).toBe('It&#x27;s working');
    });

    it('handles null and undefined', () => {
      expect(encodeHtml(null)).toBe('');
      expect(encodeHtml(undefined)).toBe('');
    });

    it('does not encode safe text', () => {
      expect(encodeHtml('Hello World')).toBe('Hello World');
    });
  });

  describe('sanitizeInput', () => {
    it('trims whitespace and encodes HTML', () => {
      expect(sanitizeInput('  <script>  ')).toBe('&lt;script&gt;');
      expect(sanitizeInput('  Hello World  ')).toBe('Hello World');
    });

    it('handles empty strings', () => {
      expect(sanitizeInput('')).toBe('');
      expect(sanitizeInput('   ')).toBe('');
    });
  });

  describe('isSafeString', () => {
    it('detects script tags', () => {
      expect(isSafeString('<script>alert("xss")</script>')).toBe(false);
      expect(isSafeString('Hello World')).toBe(true);
    });

    it('detects iframe tags', () => {
      expect(isSafeString('<iframe src="evil.com"></iframe>')).toBe(false);
    });

    it('detects javascript: protocol', () => {
      expect(isSafeString('javascript:alert("xss")')).toBe(false);
    });

    it('detects event handlers', () => {
      expect(isSafeString('<div onclick="evil()">')).toBe(false);
      expect(isSafeString('<img onerror="evil()">')).toBe(false);
    });

    it('allows safe HTML-like text', () => {
      expect(isSafeString('Use < and > operators')).toBe(true);
    });
  });

  describe('sanitizeAndValidate', () => {
    it('sanitizes safe input', () => {
      expect(sanitizeAndValidate('Hello World')).toBe('Hello World');
    });

    it('removes dangerous input', () => {
      expect(sanitizeAndValidate('<script>alert("xss")</script>')).toBe('');
    });

    it('handles null and undefined', () => {
      expect(sanitizeAndValidate(null)).toBe('');
      expect(sanitizeAndValidate(undefined)).toBe('');
    });
  });
});
