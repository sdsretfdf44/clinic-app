import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';

import { getPaginated, postResource } from '../shared';

export interface ClinicUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phoneNumber: string | null;
  primaryRole: string;
  roles: string[];
  isActive: boolean;
  isLockedOut: boolean;
  lastLoginAt: string | null;
  createdOn: string;
}

export const inviteUserSchema = z.object({
  firstName: z.string().min(1).max(100),
  lastName: z.string().min(1).max(100),
  email: z.string().email(),
  primaryRole: z.enum(['ClinicAdmin', 'Doctor', 'Receptionist', 'Patient']),
});
export type InviteUserFormValues = z.infer<typeof inviteUserSchema>;

interface InvitePayload {
  firstName: string;
  lastName: string;
  email: string;
  primaryRole: string;
}

interface ListParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  role?: string;
  isActive?: boolean;
}

const keys = { all: ['clinic', 'users'] as const, list: (p: unknown) => ['clinic', 'users', 'list', p] as const };

export const clinicUsersApi = {
  list: (params: ListParams) => getPaginated<ClinicUser>('/v1/clinic/users', params),
  invite: (payload: InvitePayload) => postResource<InvitePayload, ClinicUser>('/v1/clinic/users', payload),
  changeRole: (id: string, primaryRole: string) =>
    postResource<{ primaryRole: string }, unknown>(`/v1/clinic/users/${id}/role`, { primaryRole }),
  activate: (id: string) => postResource<unknown, unknown>(`/v1/clinic/users/${id}/activate`, {}),
  deactivate: (id: string) => postResource<unknown, unknown>(`/v1/clinic/users/${id}/deactivate`, {}),
};

export function useClinicUsers(params: ListParams) {
  return useQuery({ queryKey: keys.list(params), queryFn: () => clinicUsersApi.list(params), staleTime: 15_000 });
}

export function useInviteClinicUser() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: clinicUsersApi.invite, onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }) });
}

export function useChangeUserRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, primaryRole }: { id: string; primaryRole: string }) => clinicUsersApi.changeRole(id, primaryRole),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useSetClinicUserActive() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      isActive ? clinicUsersApi.activate(id) : clinicUsersApi.deactivate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}
