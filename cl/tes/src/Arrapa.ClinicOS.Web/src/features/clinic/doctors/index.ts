import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import { deleteResource, getPaginated, getResource, postResource, putResource } from '../shared';

export interface DoctorSummary {
  id: string;
  userId: string;
  fullName: string;
  email: string;
  phoneNumber: string | null;
  avatarUrl: string | null;
  specialty: string | null;
  departmentId: string | null;
  departmentName: string | null;
  effectiveConsultationFee: number | null;
  currency: string;
  isActive: boolean;
  isAcceptingNewPatients: boolean;
  createdOn: string;
}

export interface DoctorDetail {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber: string | null;
  avatarUrl: string | null;
  specialty: string | null;
  licenseNumber: string | null;
  registrationNumber: string | null;
  departmentId: string | null;
  departmentName: string | null;
  consultationFee: number | null;
  effectiveConsultationFee: number | null;
  currency: string;
  bio: string | null;
  isAcceptingNewPatients: boolean;
  isActive: boolean;
  sortOrder: number;
  createdOn: string;
  updatedOn: string | null;
}

export const createDoctorSchema = z.object({
  firstName: z.string().min(1).max(100),
  lastName: z.string().min(1).max(100),
  email: z.string().min(1).email(),
  phoneNumber: z.string().max(30).optional().or(z.literal('')),
  specialty: z.string().max(200).optional().or(z.literal('')),
  licenseNumber: z.string().max(64).optional().or(z.literal('')),
  registrationNumber: z.string().max(64).optional().or(z.literal('')),
  departmentId: z.string().uuid().optional().or(z.literal('')),
  consultationFee: z.coerce.number().min(0).optional().or(z.literal('')),
  bio: z.string().max(1000).optional().or(z.literal('')),
  isAcceptingNewPatients: z.boolean(),
});
export type CreateDoctorFormValues = z.infer<typeof createDoctorSchema>;

export const updateDoctorSchema = z.object({
  firstName: z.string().min(1).max(100),
  lastName: z.string().min(1).max(100),
  phoneNumber: z.string().max(30).optional().or(z.literal('')),
  avatarUrl: z.string().max(500).url().optional().or(z.literal('')),
  specialty: z.string().max(200).optional().or(z.literal('')),
  licenseNumber: z.string().max(64).optional().or(z.literal('')),
  registrationNumber: z.string().max(64).optional().or(z.literal('')),
  departmentId: z.string().uuid().optional().or(z.literal('')),
  consultationFee: z.coerce.number().min(0).optional().or(z.literal('')),
  bio: z.string().max(1000).optional().or(z.literal('')),
  isAcceptingNewPatients: z.boolean(),
  sortOrder: z.coerce.number().int().min(0),
});
export type UpdateDoctorFormValues = z.infer<typeof updateDoctorSchema>;

interface CreatePayload {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
  specialty: string | null;
  licenseNumber: string | null;
  registrationNumber: string | null;
  departmentId: string | null;
  consultationFee: number | null;
  bio: string | null;
  isAcceptingNewPatients: boolean;
}

interface UpdatePayload extends Omit<CreatePayload, 'email'> {
  avatarUrl: string | null;
  sortOrder: number;
}

interface ListParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  departmentId?: string;
  isActive?: boolean;
}

const keys = {
  all: ['clinic', 'doctors'] as const,
  list: (params: unknown) => ['clinic', 'doctors', 'list', params] as const,
  detail: (id: string) => ['clinic', 'doctors', 'detail', id] as const,
};

export const doctorsApi = {
  list: (params: ListParams) => getPaginated<DoctorSummary>('/v1/clinic/doctors', params),
  get: (id: string) => getResource<DoctorDetail>(`/v1/clinic/doctors/${id}`),
  create: (payload: CreatePayload) => postResource<CreatePayload, DoctorDetail>('/v1/clinic/doctors', payload),
  update: (id: string, payload: UpdatePayload) =>
    putResource<UpdatePayload, DoctorDetail>(`/v1/clinic/doctors/${id}`, payload),
  activate: (id: string) => postResource<unknown, unknown>(`/v1/clinic/doctors/${id}/activate`, {}),
  deactivate: (id: string) => postResource<unknown, unknown>(`/v1/clinic/doctors/${id}/deactivate`, {}),
  remove: (id: string) => deleteResource(`/v1/clinic/doctors/${id}`),
};

export function useDoctorsList(params: ListParams) {
  return useQuery({ queryKey: keys.list(params), queryFn: () => doctorsApi.list(params), staleTime: 15_000 });
}

export function useDoctor(id: string | undefined) {
  return useQuery({
    queryKey: id ? keys.detail(id) : keys.detail('none'),
    queryFn: () => doctorsApi.get(id!),
    enabled: !!id,
  });
}

export function useCreateDoctor() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: doctorsApi.create, onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }) });
}

export function useUpdateDoctor(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdatePayload) => doctorsApi.update(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useSetDoctorActive() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      isActive ? doctorsApi.activate(id) : doctorsApi.deactivate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useDeleteDoctor() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: doctorsApi.remove,
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}
