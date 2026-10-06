import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import { getResource, putResource } from '../shared';

export interface ClinicProfile {
  id: string;
  name: string;
  slug: string;
  status: string;
  contactEmail: string | null;
  contactPhone: string | null;
  logoUrl: string | null;
  timeZone: string;
  locale: string;
  currency: string;
  defaultConsultationFee: number | null;
  subscriptionPlanName: string | null;
  subscriptionExpiresAt: string | null;
}

export const updateClinicProfileSchema = z.object({
  name: z.string().min(1).max(200),
  contactEmail: z.string().email().optional().or(z.literal('')),
  contactPhone: z.string().max(30).optional().or(z.literal('')),
  logoUrl: z.string().max(500).url().optional().or(z.literal('')),
  timeZone: z.string().min(1).max(100),
  locale: z.string().min(2).max(10),
  currency: z.string().min(3).max(10),
  defaultConsultationFee: z.coerce.number().min(0).optional().or(z.literal('')),
});
export type UpdateClinicProfileFormValues = z.infer<typeof updateClinicProfileSchema>;

interface UpdatePayload {
  name: string;
  contactEmail: string | null;
  contactPhone: string | null;
  logoUrl: string | null;
  timeZone: string;
  locale: string;
  currency: string;
  defaultConsultationFee: number | null;
}

const keys = { detail: ['clinic', 'profile'] as const };

export const profileApi = {
  get: () => getResource<ClinicProfile>('/v1/clinic/profile'),
  update: (payload: UpdatePayload) => putResource<UpdatePayload, ClinicProfile>('/v1/clinic/profile', payload),
};

export function useClinicProfile() {
  return useQuery({ queryKey: keys.detail, queryFn: profileApi.get, staleTime: 30_000 });
}

export function useUpdateClinicProfile() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: profileApi.update,
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.detail }),
  });
}
