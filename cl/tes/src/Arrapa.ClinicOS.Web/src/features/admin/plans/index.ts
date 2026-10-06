import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import { getResource, postResource, putResource } from '../shared';

export interface SubscriptionPlan {
  id: string;
  code: string;
  name: string;
  description: string | null;
  interval: 'Monthly' | 'Annual';
  price: number;
  currency: string;
  maxUsers: number | null;
  maxPatients: number | null;
  maxAppointmentsPerMonth: number | null;
  features: string[];
  isActive: boolean;
  isDefault: boolean;
  sortOrder: number;
  tenantCount: number;
}

export const upsertPlanSchema = z.object({
  code: z.string().min(1).max(64).regex(/^[a-z0-9_-]+$/, 'Lowercase, digits, underscores, hyphens only.'),
  name: z.string().min(1).max(200),
  description: z.string().max(1000).optional().or(z.literal('')),
  interval: z.enum(['Monthly', 'Annual']),
  price: z.coerce.number().min(0),
  currency: z.string().min(3).max(10),
  maxUsers: z.coerce.number().int().min(0).optional().or(z.literal('')),
  maxPatients: z.coerce.number().int().min(0).optional().or(z.literal('')),
  maxAppointmentsPerMonth: z.coerce.number().int().min(0).optional().or(z.literal('')),
  features: z.array(z.string()),
  isActive: z.boolean(),
  isDefault: z.boolean(),
  sortOrder: z.coerce.number().int().min(0),
});
export type UpsertPlanFormValues = z.infer<typeof upsertPlanSchema>;

export interface UpsertPlanPayload {
  code: string;
  name: string;
  description: string | null;
  interval: 'Monthly' | 'Annual';
  price: number;
  currency: string;
  maxUsers: number | null;
  maxPatients: number | null;
  maxAppointmentsPerMonth: number | null;
  features: string[];
  isActive: boolean;
  isDefault: boolean;
  sortOrder: number;
}

const keys = {
  all: ['admin', 'plans'] as const,
  list: (includeInactive: boolean) => ['admin', 'plans', 'list', includeInactive] as const,
};

export const plansApi = {
  list: (includeInactive: boolean) =>
    getResource<SubscriptionPlan[]>('/v1/admin/plans', { includeInactive }),
  create: (payload: UpsertPlanPayload) => postResource<UpsertPlanPayload, SubscriptionPlan>('/v1/admin/plans', payload),
  update: (id: string, payload: UpsertPlanPayload) =>
    putResource<UpsertPlanPayload, SubscriptionPlan>(`/v1/admin/plans/${id}`, payload),
};

export function usePlansList(includeInactive = false) {
  return useQuery({
    queryKey: keys.list(includeInactive),
    queryFn: () => plansApi.list(includeInactive),
    staleTime: 30_000,
  });
}

export function useCreatePlan() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: plansApi.create,
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useUpdatePlan(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpsertPlanPayload) => plansApi.update(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}
