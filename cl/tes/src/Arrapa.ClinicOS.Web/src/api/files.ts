import type { ApiResponse } from '@/types/api';

import { httpClient } from './httpClient';

export interface UploadFileResponse {
  key: string;
  contentType: string;
  sizeBytes: number;
  publicUrl: string | null;
}

/**
 * POSTs a single file to `/v1/files/upload` as multipart/form-data. `category` becomes a
 * path segment inside the tenant scope so uploads are organized (e.g. avatars, prescriptions).
 */
export async function uploadFile(file: File, category: string): Promise<UploadFileResponse> {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('category', category);

  const response = await httpClient.post<ApiResponse<UploadFileResponse>>('/v1/files/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });

  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.error?.message ?? 'Upload failed.');
  }

  return response.data.data;
}

export function fileDownloadUrl(key: string): string {
  return `/v1/files/${key}`;
}
