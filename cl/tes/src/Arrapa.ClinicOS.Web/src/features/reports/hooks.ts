import { useQuery } from '@tanstack/react-query';

import { reportsApi, type DateRangeParams } from './api';

const STALE = 60_000;

export function useRevenueSummary(params: DateRangeParams) {
  return useQuery({
    queryKey: ['reports', 'revenue-summary', params],
    queryFn: () => reportsApi.revenueSummary(params),
    staleTime: STALE,
  });
}

export function useAppointmentsFunnel(params: DateRangeParams) {
  return useQuery({
    queryKey: ['reports', 'appointments-funnel', params],
    queryFn: () => reportsApi.appointmentsFunnel(params),
    staleTime: STALE,
  });
}

export function useTopDoctorsReport(params: DateRangeParams & { top?: number }) {
  return useQuery({
    queryKey: ['reports', 'top-doctors', params],
    queryFn: () => reportsApi.topDoctors(params),
    staleTime: STALE,
  });
}

export function useTopServicesReport(params: DateRangeParams & { top?: number }) {
  return useQuery({
    queryKey: ['reports', 'top-services', params],
    queryFn: () => reportsApi.topServices(params),
    staleTime: STALE,
  });
}

export function usePatientDemographics() {
  return useQuery({
    queryKey: ['reports', 'patient-demographics'],
    queryFn: () => reportsApi.patientDemographics(),
    staleTime: STALE,
  });
}
