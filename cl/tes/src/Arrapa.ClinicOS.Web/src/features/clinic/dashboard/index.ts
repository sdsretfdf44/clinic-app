import { useQuery } from '@tanstack/react-query';

import { getResource } from '../shared';

export interface ClinicDashboard {
  staff: {
    totalUsers: number;
    activeUsers: number;
    doctors: number;
    receptionists: number;
    clinicAdmins: number;
  };
  departments: { total: number; active: number };
  appointments: {
    today: number;
    tomorrow: number;
    thisWeek: number;
    thisMonth: number;
    pendingRequests: number;
    noShowRatePct: number;
  };
  revenue: {
    issuedThisMonth: number;
    collectedThisMonth: number;
    outstandingTotal: number;
    outstandingInvoiceCount: number;
  };
  patients: {
    totalActive: number;
    newThisMonth: number;
  };
  currency: string;
  defaultConsultationFee: number | null;
  departmentBreakdown: { id: string; name: string; doctorCount: number; defaultConsultationFee: number | null }[];
  appointmentTrend: { date: string; total: number; completed: number }[];
  topDoctors: {
    doctorId: string;
    doctorName: string;
    specialty: string | null;
    completedThisMonth: number;
    revenueThisMonth: number;
  }[];
}

export function useClinicDashboard() {
  return useQuery({
    queryKey: ['clinic', 'dashboard'],
    queryFn: () => getResource<ClinicDashboard>('/v1/clinic/dashboard'),
    staleTime: 30_000,
    refetchOnWindowFocus: true,
  });
}
