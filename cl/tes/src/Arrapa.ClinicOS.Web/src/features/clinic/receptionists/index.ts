import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import { deleteResource, getResource, postResource, putResource } from '../shared';

export interface Receptionist {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber: string | null;
  avatarUrl: string | null;
  title: string | null;
  isActive: boolean;
  sortOrder: number;
  createdOn: string;
}

export const createReceptionistSchema = z.object({
  firstName: z.string().min(1).max(100),
  lastName: z.string().min(1).max(100),
  email: z.string().email(),
  phoneNumber: z.string().max(30).optional().or(z.literal('')),
  title: z.string().max(200).optional().or(z.literal('')),
});
export type CreateReceptionistFormValues = z.infer<typeof createReceptionistSchema>;

export const updateReceptionistSchema = z.object({
  firstName: z.string().min(1).max(100),
  lastName: z.string().min(1).max(100),
  phoneNumber: z.string().max(30).optional().or(z.literal('')),
  avatarUrl: z.string().max(500).url().optional().or(z.literal('')),
  title: z.string().max(200).optional().or(z.literal('')),
  sortOrder: z.coerce.number().int().min(0),
});
export type UpdateReceptionistFormValues = z.infer<typeof updateReceptionistSchema>;

interface CreatePayload {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
  title: string | null;
}
interface UpdatePayload {
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
  avatarUrl: string | null;
  title: string | null;
  sortOrder: number;
}

const keys = { all: ['clinic', 'receptionists'] as const };

export const receptionistsApi = {
  list: (includeInactive = true) =>
    getResource<Receptionist[]>('/v1/clinic/receptionists', { includeInactive }),
  create: (payload: CreatePayload) => postResource<CreatePayload, Receptionist>('/v1/clinic/receptionists', payload),
  update: (id: string, payload: UpdatePayload) =>
    putResource<UpdatePayload, Receptionist>(`/v1/clinic/receptionists/${id}`, payload),
  activate: (id: string) => postResource<unknown, unknown>(`/v1/clinic/receptionists/${id}/activate`, {}),
  deactivate: (id: string) => postResource<unknown, unknown>(`/v1/clinic/receptionists/${id}/deactivate`, {}),
  remove: (id: string) => deleteResource(`/v1/clinic/receptionists/${id}`),
};

export function useReceptionists(includeInactive = true) {
  return useQuery({
    queryKey: [...keys.all, includeInactive],
    queryFn: () => receptionistsApi.list(includeInactive),
    staleTime: 30_000,
  });
}

export function useCreateReceptionist() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: receptionistsApi.create, onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }) });
}

export function useUpdateReceptionist(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdatePayload) => receptionistsApi.update(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useSetReceptionistActive() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      isActive ? receptionistsApi.activate(id) : receptionistsApi.deactivate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useDeleteReceptionist() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: receptionistsApi.remove, onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }) });
}
