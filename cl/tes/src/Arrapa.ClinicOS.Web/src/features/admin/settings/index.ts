import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import { getResource, putResource } from '../shared';

export interface GlobalSetting {
  id: string;
  key: string;
  value: string;
  category: string;
  dataType: 'String' | 'Integer' | 'Decimal' | 'Boolean' | 'Json';
  description: string | null;
  isSecret: boolean;
  updatedOn: string | null;
}

export const upsertSettingSchema = z.object({
  key: z.string().min(1).max(128).regex(/^[a-z][a-z0-9_.]*$/, 'Start with a lowercase letter; use lowercase letters, digits, underscores, or periods.'),
  value: z.string().max(4000),
  category: z.string().min(1).max(64),
  dataType: z.enum(['String', 'Integer', 'Decimal', 'Boolean', 'Json']),
  description: z.string().max(500).optional().or(z.literal('')),
  isSecret: z.boolean(),
});
export type UpsertSettingFormValues = z.infer<typeof upsertSettingSchema>;

interface UpsertSettingPayload {
  key: string;
  value: string;
  category: string;
  dataType: 'String' | 'Integer' | 'Decimal' | 'Boolean' | 'Json';
  description: string | null;
  isSecret: boolean;
}

const keys = { all: ['admin', 'settings'] as const };

export const settingsApi = {
  list: (category?: string) => getResource<GlobalSetting[]>('/v1/admin/settings', category ? { category } : undefined),
  upsert: (payload: UpsertSettingPayload) => putResource<UpsertSettingPayload, GlobalSetting>('/v1/admin/settings', payload),
};

export function useSettingsList(category?: string) {
  return useQuery({
    queryKey: [...keys.all, category],
    queryFn: () => settingsApi.list(category),
    staleTime: 30_000,
  });
}

export function useUpsertSetting() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: settingsApi.upsert,
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}
