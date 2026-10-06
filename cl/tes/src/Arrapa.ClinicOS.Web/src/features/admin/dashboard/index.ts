import { useQuery } from '@tanstack/react-query';

import { getResource } from '../shared';

export interface SuperAdminDashboard {
  tenants: { total: number; active: number; trial: number; suspended: number; newThisMonth: number };
  users: { total: number; active: number; inactive: number; superAdmins: number; clinicAdmins: number };
  revenue: { monthlyRecurring: number; annualRecurring: number; currency: string };
  planRevenue: { planId: string; planName: string; tenantCount: number; monthlyRevenue: number }[];
  recentTenants: { id: string; name: string; slug: string; status: string; createdOn: string }[];
  recentActivity: { id: string; entityType: string; entityId: string; operation: string; performedByEmail: string | null; occurredOn: string }[];
}

export function useAdminDashboard() {
  return useQuery({
    queryKey: ['admin', 'dashboard'],
    queryFn: () => getResource<SuperAdminDashboard>('/v1/admin/dashboard'),
    staleTime: 30_000,
    refetchOnWindowFocus: true,
  });
}
