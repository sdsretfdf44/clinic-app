import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import { deleteResource, getResource, postResource, putResource } from '../shared';

export interface Department {
  id: string;
  name: string;
  description: string | null;
  defaultConsultationFee: number | null;
  isActive: boolean;
  sortOrder: number;
  doctorCount: number;
  createdOn: string;
}

export const upsertDepartmentSchema = z.object({
  name: z.string().min(1).max(200),
  description: z.string().max(500).optional().or(z.literal('')),
  defaultConsultationFee: z.coerce.number().min(0).optional().or(z.literal('')),
  isActive: z.boolean(),
  sortOrder: z.coerce.number().int().min(0),
});
export type UpsertDepartmentFormValues = z.infer<typeof upsertDepartmentSchema>;

interface UpsertPayload {
  name: string;
  description: string | null;
  defaultConsultationFee: number | null;
  isActive: boolean;
  sortOrder: number;
}

const keys = {
  all: ['clinic', 'departments'] as const,
  list: (includeInactive: boolean) => ['clinic', 'departments', 'list', includeInactive] as const,
};

export const departmentsApi = {
  list: (includeInactive = true) =>
    getResource<Department[]>('/v1/clinic/departments', { includeInactive }),
  create: (payload: UpsertPayload) => postResource<UpsertPayload, Department>('/v1/clinic/departments', payload),
  update: (id: string, payload: UpsertPayload) =>
    putResource<UpsertPayload, Department>(`/v1/clinic/departments/${id}`, payload),
  remove: (id: string) => deleteResource(`/v1/clinic/departments/${id}`),
};

export function useDepartments(includeInactive = true) {
  return useQuery({
    queryKey: keys.list(includeInactive),
    queryFn: () => departmentsApi.list(includeInactive),
    staleTime: 30_000,
  });
}

export function useUpsertDepartment(id?: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpsertPayload) => (id ? departmentsApi.update(id, payload) : departmentsApi.create(payload)),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useDeleteDepartment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: departmentsApi.remove,
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}
