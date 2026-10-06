import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import { getResource, putResource } from '../shared';

export interface ClinicSetting {
  id: string;
  key: string;
  value: string;
  category: string;
  dataType: 'String' | 'Integer' | 'Decimal' | 'Boolean' | 'Json';
  description: string | null;
  isSecret: boolean;
  updatedOn: string | null;
}

export const upsertClinicSettingSchema = z.object({
  key: z.string().min(1).max(128).regex(/^[a-z][a-z0-9_.]*$/, 'Start with a lowercase letter; use lowercase letters, digits, underscores, or periods.'),
  value: z.string().max(4000),
  category: z.string().min(1).max(64),
  dataType: z.enum(['String', 'Integer', 'Decimal', 'Boolean', 'Json']),
  description: z.string().max(500).optional().or(z.literal('')),
  isSecret: z.boolean(),
});
export type UpsertClinicSettingFormValues = z.infer<typeof upsertClinicSettingSchema>;

interface UpsertPayload {
  key: string;
  value: string;
  category: string;
  dataType: 'String' | 'Integer' | 'Decimal' | 'Boolean' | 'Json';
  description: string | null;
  isSecret: boolean;
}

const keys = { all: ['clinic', 'settings'] as const };

export const clinicSettingsApi = {
  list: (category?: string) => getResource<ClinicSetting[]>('/v1/clinic/settings', category ? { category } : undefined),
  upsert: (payload: UpsertPayload) => putResource<UpsertPayload, ClinicSetting>('/v1/clinic/settings', payload),
};

export function useClinicSettings(category?: string) {
  return useQuery({
    queryKey: [...keys.all, category],
    queryFn: () => clinicSettingsApi.list(category),
    staleTime: 30_000,
  });
}

export function useUpsertClinicSetting() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: clinicSettingsApi.upsert, onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }) });
}
