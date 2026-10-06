import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import { deleteResource, getResource, postResource, putResource } from '../shared';

export interface FeatureFlag {
  id: string;
  key: string;
  name: string;
  description: string | null;
  defaultEnabled: boolean;
  isGlobal: boolean;
  overrideCount: number;
}

export const upsertFlagSchema = z.object({
  key: z.string().min(1).max(128).regex(/^[a-z0-9_]+$/, 'Lowercase, digits, underscores only.'),
  name: z.string().min(1).max(200),
  description: z.string().max(1000).optional().or(z.literal('')),
  defaultEnabled: z.boolean(),
  isGlobal: z.boolean(),
});
export type UpsertFlagFormValues = z.infer<typeof upsertFlagSchema>;

interface UpsertFlagPayload {
  key: string;
  name: string;
  description: string | null;
  defaultEnabled: boolean;
  isGlobal: boolean;
}

const keys = { all: ['admin', 'flags'] as const };

export const flagsApi = {
  list: () => getResource<FeatureFlag[]>('/v1/admin/feature-flags'),
  create: (payload: UpsertFlagPayload) => postResource<UpsertFlagPayload, FeatureFlag>('/v1/admin/feature-flags', payload),
  update: (id: string, payload: UpsertFlagPayload) =>
    putResource<UpsertFlagPayload, FeatureFlag>(`/v1/admin/feature-flags/${id}`, payload),
  setOverride: (id: string, tenantId: string, isEnabled: boolean) =>
    postResource<{ tenantId: string; isEnabled: boolean }, unknown>(`/v1/admin/feature-flags/${id}/override`, { tenantId, isEnabled }),
  clearOverride: (id: string, tenantId: string) =>
    deleteResource(`/v1/admin/feature-flags/${id}/override/${tenantId}`),
};

export function useFlagsList() {
  return useQuery({ queryKey: keys.all, queryFn: flagsApi.list, staleTime: 30_000 });
}

export function useUpsertFlag(id?: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpsertFlagPayload) =>
      id ? flagsApi.update(id, payload) : flagsApi.create(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}
