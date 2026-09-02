import { ApiError } from './api-error';

interface ApiRequestOptions extends RequestInit {
  baseUrl: string;
  path: string;
  json?: unknown;
  token?: string;
}

export async function apiRequest<T = void>(options: ApiRequestOptions): Promise<T> {
  const { baseUrl, path, json, headers, method, token, ...rest } = options;

  const response = await fetch(`${baseUrl}${path}`, {
    method: method || (json ? 'POST' : 'GET'),
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(json ? { 'Content-Type': 'application/json' } : {}),
      ...headers,
    },
    body: json ? JSON.stringify(json) : undefined,
    cache: 'no-store',
    ...rest,
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => ({}));
    throw new ApiError(response.status, problem);
  }

  if (response.status === 204) return undefined as T;
  return response.json();
}