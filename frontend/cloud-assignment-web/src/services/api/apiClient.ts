import { buildApiUrl } from './apiUrl';
import type { ProblemDetails } from '../../types/api';

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly problem: ProblemDetails,
  ) {
    super(problem.detail ?? problem.title ?? 'API request failed.');
    this.name = 'ApiError';
  }
}

interface ApiRequestOptions extends Omit<RequestInit, 'body'> {
  body?: unknown;
  accessToken?: string;
}

export async function apiRequest<T>(
  path: string,
  options: ApiRequestOptions = {},
): Promise<T> {
  const { accessToken, body, ...requestInit } = options;
  const headers = new Headers(requestInit.headers);
  headers.set('Accept', 'application/json');

  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`);
  }

  const isFormData = body instanceof FormData;
  if (body !== undefined && !isFormData) {
    headers.set('Content-Type', 'application/json');
  }

  const response = await fetch(buildApiUrl(path), {
    ...requestInit,
    credentials: 'include',
    headers,
    body: body === undefined ? undefined : isFormData ? body : JSON.stringify(body),
  });

  if (!response.ok) {
    throw new ApiError(response.status, await readProblemDetails(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}


export async function apiDownload(
  path: string,
  accessToken: string,
  options: RequestInit = {},
): Promise<{ blob: Blob; fileName: string }> {
  const headers = new Headers(options.headers);
  headers.set('Authorization', `Bearer ${accessToken}`);
  const response = await fetch(buildApiUrl(path), {
    ...options,
    credentials: 'include',
    headers,
  });

  if (!response.ok) {
    throw new ApiError(response.status, await readProblemDetails(response));
  }

  const disposition = response.headers.get('Content-Disposition') ?? '';
  const match = /filename\*?=(?:UTF-8''|\")?([^\";]+)/i.exec(disposition);
  const fileName = match ? decodeURIComponent(match[1].replace(/\"/g, '').trim()) : 'download.xlsx';
  return { blob: await response.blob(), fileName };
}

async function readProblemDetails(response: Response): Promise<ProblemDetails> {
  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    return {
      title: 'API request failed',
      status: response.status,
      detail: response.statusText,
      errorCode: 'SYSTEM_INVALID_ERROR_RESPONSE',
    };
  }
}
