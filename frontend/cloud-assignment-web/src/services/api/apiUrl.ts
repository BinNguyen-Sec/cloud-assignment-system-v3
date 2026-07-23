const DEFAULT_API_BASE_URL = '/api/v1';

export function normalizeApiBaseUrl(value?: string): string {
  const trimmed = value?.trim();
  if (!trimmed) {
    return DEFAULT_API_BASE_URL;
  }

  return trimmed.replace(/\/+$/, '');
}

export function buildApiUrl(path: string, baseUrl = normalizeApiBaseUrl(import.meta.env.VITE_API_BASE_URL)) {
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${baseUrl}${normalizedPath}`;
}
