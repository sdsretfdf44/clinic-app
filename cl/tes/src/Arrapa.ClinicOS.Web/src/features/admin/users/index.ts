import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { getPaginated, postResource } from '../shared';

export interface AdminUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phoneNumber: string | null;
  tenantId: string;
  tenantName: string;
  tenantSlug: string;
  primaryRole: string;
  roles: string[];
  isActive: boolean;
  isLockedOut: boolean;
  lastLoginAt: string | null;
  createdOn: string;
}

interface ListParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  tenantId?: string;
  role?: string;
  isActive?: boolean;
}

const keys = {
  all: ['admin', 'users'] as const,
  list: (params: unknown) => ['admin', 'users', 'list', params] as const,
};

export const usersApi = {
  list: (params: ListParams) => getPaginated<AdminUser>('/v1/admin/users', params),
  activate: (id: string) => postResource<unknown, unknown>(`/v1/admin/users/${id}/activate`, {}),
  deactivate: (id: string) => postResource<unknown, unknown>(`/v1/admin/users/${id}/deactivate`, {}),
};

export function useUsersList(params: ListParams) {
  return useQuery({
    queryKey: keys.list(params),
    queryFn: () => usersApi.list(params),
    staleTime: 15_000,
  });
}

export function useActivateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => usersApi.activate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useDeactivateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => usersApi.deactivate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}
