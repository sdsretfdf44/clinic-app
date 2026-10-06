import type { PaginatedList } from '@/types/api';
import { deleteResource, getPaginated, getResource, postResource, putResource } from '../shared';
import type { TenantDetail, TenantSummary } from './types';

interface ListParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  status?: string;
  sortBy?: string;
  sortDirection?: 'Ascending' | 'Descending';
}

export interface CreateTenantPayload {
  name: string;
  slug: string;
  contactEmail: string | null;
  contactPhone: string | null;
  timeZone: string;
  locale: string;
  currency: string;
  subscriptionPlanId: string | null;
  adminFirstName: string;
  adminLastName: string;
  adminEmail: string;
}

export interface UpdateTenantPayload {
  name: string;
  contactEmail: string | null;
  contactPhone: string | null;
  logoUrl: string | null;
  timeZone: string;
  locale: string;
  currency: string;
}

export interface AssignPlanPayload {
  subscriptionPlanId: string;
  expiresAt: string | null;
}

export const tenantsApi = {
  list: (params: ListParams) => getPaginated<TenantSummary>('/v1/admin/tenants', params) as Promise<PaginatedList<TenantSummary>>,
  get: (id: string) => getResource<TenantDetail>(`/v1/admin/tenants/${id}`),
  create: (payload: CreateTenantPayload) => postResource<CreateTenantPayload, TenantDetail>('/v1/admin/tenants', payload),
  update: (id: string, payload: UpdateTenantPayload) => putResource<UpdateTenantPayload, TenantDetail>(`/v1/admin/tenants/${id}`, payload),
  suspend: (id: string, reason: string) => postResource<{ reason: string }, unknown>(`/v1/admin/tenants/${id}/suspend`, { reason }),
  activate: (id: string) => postResource<unknown, unknown>(`/v1/admin/tenants/${id}/activate`, {}),
  remove: (id: string) => deleteResource(`/v1/admin/tenants/${id}`),
  assignPlan: (id: string, payload: AssignPlanPayload) => postResource<AssignPlanPayload, unknown>(`/v1/admin/tenants/${id}/plan`, payload),
};
