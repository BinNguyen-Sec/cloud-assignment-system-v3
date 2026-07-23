import { describe, expect, it } from 'vitest';
import { buildApiUrl, normalizeApiBaseUrl } from './apiUrl';

describe('normalizeApiBaseUrl', () => {
  it('uses the default base URL when empty', () => {
    expect(normalizeApiBaseUrl()).toBe('/api/v1');
  });

  it('removes trailing slashes', () => {
    expect(normalizeApiBaseUrl('https://api.example.com/api/v1///')).toBe(
      'https://api.example.com/api/v1',
    );
  });
});

describe('buildApiUrl', () => {
  it('joins base URL and path exactly once', () => {
    expect(buildApiUrl('system/info', '/api/v1')).toBe('/api/v1/system/info');
  });
});
