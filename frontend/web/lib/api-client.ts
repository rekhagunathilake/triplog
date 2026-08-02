import { ApiError } from './api-error';

const DEV_USER_ID = process.env.NEXT_PUBLIC_DEV_USER_ID!;

interface ApiRequestOptions extends RequestInit {
  baseUrl: string;
  path: string;
  json?: unknown;
}

export async function apiRequest<T = void>(options: ApiRequestOptions): Promise<T> {
  const { baseUrl, path, json, headers, method, ...rest } = options;

  const response = await fetch(`${baseUrl}${path}`, {
    method: method || (json ? 'POST' : 'GET'),
    headers: {
      'X-User-Id': DEV_USER_ID,
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