import { useQuery } from '@tanstack/react-query';

import { getResource } from '@/features/admin/shared';

export type DoctorAppointmentStatus =
  | 'Booked'
  | 'Confirmed'
  | 'CheckedIn'
  | 'InConsultation'
  | 'Completed'
  | 'Cancelled'
  | 'NoShow';

export interface DoctorDashboard {
  doctor: {
    id: string;
    fullName: string;
    specialty: string | null;
    departmentId: string | null;
    departmentName: string | null;
    currency: string;
  };
  today: {
    total: number;
    remaining: number;
    inConsultation: number;
    completed: number;
    noShow: number;
  };
  thisWeek: {
    booked: number;
    completed: number;
    cancelled: number;
    noShow: number;
    consultationRevenue: number;
  };
  thisMonth: {
    booked: number;
    completed: number;
    cancelled: number;
    noShow: number;
    consultationRevenue: number;
  };
  lifetime: {
    uniquePatients: number;
    totalCompleted: number;
  };
  nextAppointments: {
    id: string;
    appointmentNumber: string;
    patientId: string;
    patientName: string;
    patientMrn: string;
    scheduledStart: string;
    scheduledEnd: string;
    status: DoctorAppointmentStatus;
    reason: string | null;
  }[];
  appointmentTrend: { date: string; total: number; completed: number }[];
}

export function useDoctorDashboard() {
  return useQuery({
    queryKey: ['doctor', 'dashboard'],
    queryFn: () => getResource<DoctorDashboard>('/v1/doctor/dashboard'),
    staleTime: 30_000,
    refetchOnWindowFocus: true,
  });
}
