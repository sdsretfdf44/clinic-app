import { getResource } from '@/features/admin/shared';

import type {
  AppointmentsFunnel,
  PatientDemographics,
  RevenueSummary,
  TopDoctorsReport,
  TopServicesReport,
} from './types';

export interface DateRangeParams {
  from?: string;
  to?: string;
}

export const reportsApi = {
  revenueSummary: (params: DateRangeParams) =>
    getResource<RevenueSummary>('/v1/reports/revenue-summary', params),
  appointmentsFunnel: (params: DateRangeParams) =>
    getResource<AppointmentsFunnel>('/v1/reports/appointments-funnel', params),
  topDoctors: (params: DateRangeParams & { top?: number }) =>
    getResource<TopDoctorsReport>('/v1/reports/top-doctors', params),
  topServices: (params: DateRangeParams & { top?: number }) =>
    getResource<TopServicesReport>('/v1/reports/top-services', params),
  patientDemographics: () => getResource<PatientDemographics>('/v1/reports/patient-demographics'),
};
