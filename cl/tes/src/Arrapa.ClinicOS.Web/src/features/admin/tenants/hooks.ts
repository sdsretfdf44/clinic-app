import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { tenantsApi, type CreateTenantPayload, type UpdateTenantPayload, type AssignPlanPayload } from './api';

const keys = {
  all: ['admin', 'tenants'] as const,
  list: (params: unknown) => ['admin', 'tenants', 'list', params] as const,
  detail: (id: string) => ['admin', 'tenants', 'detail', id] as const,
};

export function useTenantsList(params: Parameters<typeof tenantsApi.list>[0]) {
  return useQuery({
    queryKey: keys.list(params),
    queryFn: () => tenantsApi.list(params),
    staleTime: 15_000,
  });
}

export function useTenant(id: string | undefined) {
  return useQuery({
    queryKey: id ? keys.detail(id) : keys.detail('none'),
    queryFn: () => tenantsApi.get(id!),
    enabled: !!id,
  });
}

export function useCreateTenant() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateTenantPayload) => tenantsApi.create(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useUpdateTenant(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateTenantPayload) => tenantsApi.update(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.detail(id) });
      qc.invalidateQueries({ queryKey: keys.all });
    },
  });
}

export function useSuspendTenant(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (reason: string) => tenantsApi.suspend(id, reason),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.detail(id) });
      qc.invalidateQueries({ queryKey: keys.all });
    },
  });
}

export function useActivateTenant(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () => tenantsApi.activate(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.detail(id) });
      qc.invalidateQueries({ queryKey: keys.all });
    },
  });
}

export function useDeleteTenant() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => tenantsApi.remove(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useAssignPlan(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: AssignPlanPayload) => tenantsApi.assignPlan(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.detail(id) });
      qc.invalidateQueries({ queryKey: keys.all });
    },
  });
}
