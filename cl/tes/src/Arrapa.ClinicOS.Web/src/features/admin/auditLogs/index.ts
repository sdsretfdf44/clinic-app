import { useQuery } from '@tanstack/react-query';

import { getPaginated, getResource } from '../shared';

export interface AuditLog {
  id: string;
  tenantId: string;
  tenantName: string | null;
  entityType: string;
  entityId: string;
  operation: string;
  performedByUserId: string | null;
  performedByEmail: string | null;
  ipAddress: string | null;
  correlationId: string | null;
  occurredOn: string;
}

export interface AuditLogDetail extends AuditLog {
  userAgent: string | null;
  changesJson: string | null;
}

interface ListParams {
  pageNumber: number;
  pageSize: number;
  tenantId?: string;
  entityType?: string;
  operation?: string;
  from?: string;
  to?: string;
  search?: string;
}

export const auditLogsApi = {
  list: (params: ListParams) => getPaginated<AuditLog>('/v1/admin/audit-logs', params),
  get: (id: string) => getResource<AuditLogDetail>(`/v1/admin/audit-logs/${id}`),
};

export function useAuditLogsList(params: ListParams) {
  return useQuery({
    queryKey: ['admin', 'audit-logs', 'list', params],
    queryFn: () => auditLogsApi.list(params),
    staleTime: 15_000,
  });
}

export function useAuditLog(id: string | undefined) {
  return useQuery({
    queryKey: ['admin', 'audit-logs', 'detail', id ?? 'none'],
    queryFn: () => auditLogsApi.get(id!),
    enabled: !!id,
  });
}
