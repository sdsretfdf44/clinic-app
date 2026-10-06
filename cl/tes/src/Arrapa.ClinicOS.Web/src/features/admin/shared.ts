import { httpClient } from '@/api/httpClient';
import type { ApiResponse, PaginatedList } from '@/types/api';

export function unwrap<T>(response: ApiResponse<T>): T {
  if (!response.success || response.data === null || response.data === undefined) {
    throw new Error(response.error?.message ?? response.message ?? 'Request failed.');
  }
  return response.data;
}

export async function getPaginated<T>(url: string, params?: object): Promise<PaginatedList<T>> {
  const { data } = await httpClient.get<ApiResponse<PaginatedList<T>>>(url, { params });
  return unwrap(data);
}

export async function getResource<T>(url: string, params?: object): Promise<T> {
  const { data } = await httpClient.get<ApiResponse<T>>(url, { params });
  return unwrap(data);
}

export async function postResource<TIn, TOut = void>(url: string, body: TIn): Promise<TOut> {
  const { data } = await httpClient.post<ApiResponse<TOut>>(url, body);
  return unwrap(data);
}

export async function putResource<TIn, TOut = void>(url: string, body: TIn): Promise<TOut> {
  const { data } = await httpClient.put<ApiResponse<TOut>>(url, body);
  return unwrap(data);
}

export async function deleteResource(url: string): Promise<void> {
  await httpClient.delete<ApiResponse<unknown>>(url);
}
