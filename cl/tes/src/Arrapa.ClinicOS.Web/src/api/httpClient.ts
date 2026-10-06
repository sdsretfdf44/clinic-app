import axios, {
  AxiosError,
  AxiosHeaders,
  type AxiosInstance,
  type AxiosRequestConfig,
  type InternalAxiosRequestConfig,
} from 'axios';

import { env } from '@/app/env';
import { useAuthStore } from '@/store/authStore';
import type { ApiResponse, ApiError } from '@/types/api';

type RetryableRequestConfig = InternalAxiosRequestConfig & { _retry?: boolean };

export const httpClient: AxiosInstance = axios.create({
  baseURL: env.apiBaseUrl,
  timeout: 30_000,
  withCredentials: false,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
});

httpClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const state = useAuthStore.getState();
  const token = state.accessToken;
  const tenantId = state.tenantId;

  if (!(config.headers instanceof AxiosHeaders)) {
    config.headers = new AxiosHeaders(config.headers);
  }
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`);
  }
  if (tenantId) {
    config.headers.set('X-Tenant-Id', tenantId);
  }
  return config;
});

/**
 * Single-flight refresh: at most one refresh request is in flight at a time. Concurrent 401s wait for the same
 * refresh promise, then replay their original request with the new access token. On refresh failure, all pending
 * requests are rejected and the session is cleared.
 */
let refreshPromise: Promise<string | null> | null = null;

async function refreshAccessToken(): Promise<string | null> {
  const state = useAuthStore.getState();
  const refreshToken = state.refreshToken;
  if (!refreshToken) {
    return null;
  }

  const response = await axios.post<ApiResponse<{ accessToken: string; refreshToken: string }>>(
    '/v1/auth/refresh',
    { refreshToken },
    { baseURL: env.apiBaseUrl, timeout: 15_000 },
  );

  const data = response.data.data;
  if (!response.data.success || !data) {
    throw new Error(response.data.error?.message ?? 'Refresh failed.');
  }
  useAuthStore.getState().updateTokens(data.accessToken, data.refreshToken);
  return data.accessToken;
}

httpClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<ApiResponse<unknown>>) => {
    const original = error.config as RetryableRequestConfig | undefined;

    if (!original || error.response?.status !== 401 || original._retry) {
      return Promise.reject(error);
    }

    // Do not attempt a refresh on the refresh endpoint itself.
    if (typeof original.url === 'string' && original.url.includes('/v1/auth/refresh')) {
      useAuthStore.getState().signOut();
      return Promise.reject(error);
    }

    original._retry = true;

    if (!refreshPromise) {
      refreshPromise = refreshAccessToken()
        .catch((refreshError) => {
          useAuthStore.getState().signOut();
          throw refreshError;
        })
        .finally(() => {
          refreshPromise = null;
        });
    }

    try {
      const newToken = await refreshPromise;
      if (!newToken) {
        useAuthStore.getState().signOut();
        return Promise.reject(error);
      }
      if (!(original.headers instanceof AxiosHeaders)) {
        original.headers = new AxiosHeaders(original.headers);
      }
      original.headers.set('Authorization', `Bearer ${newToken}`);
      return httpClient.request(original as AxiosRequestConfig);
    } catch (refreshError) {
      return Promise.reject(refreshError as Error);
    }
  },
);

export function extractApiError(error: unknown): ApiError | null {
  if (axios.isAxiosError<ApiResponse<unknown>>(error) && error.response?.data?.error) {
    return error.response.data.error;
  }
  return null;
}

export function humanizeError(error: unknown, fallback = 'Something went wrong. Please try again.'): string {
  const apiError = extractApiError(error);
  if (apiError) return apiError.message;
  if (error instanceof Error) return error.message;
  return fallback;
}
