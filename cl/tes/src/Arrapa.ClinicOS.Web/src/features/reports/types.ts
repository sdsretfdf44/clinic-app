export interface RevenueSeriesPoint {
  date: string;
  issued: number;
  collected: number;
}

export interface RevenueSummary {
  from: string;
  to: string;
  currency: string;
  issuedTotal: number;
  collectedTotal: number;
  outstandingTotal: number;
  invoiceCount: number;
  paidInvoiceCount: number;
  series: RevenueSeriesPoint[];
}

export interface AppointmentsFunnel {
  from: string;
  to: string;
  booked: number;
  confirmed: number;
  checkedIn: number;
  completed: number;
  cancelled: number;
  noShow: number;
  completionRatePct: number;
  noShowRatePct: number;
  cancellationRatePct: number;
}

export interface TopDoctorReportRow {
  doctorId: string;
  doctorName: string;
  specialty: string | null;
  appointments: number;
  completed: number;
  revenue: number;
}

export interface TopDoctorsReport {
  from: string;
  to: string;
  currency: string;
  rows: TopDoctorReportRow[];
}

export interface TopServiceReportRow {
  description: string;
  serviceCode: string | null;
  lineCount: number;
  quantity: number;
  revenue: number;
}

export interface TopServicesReport {
  from: string;
  to: string;
  currency: string;
  rows: TopServiceReportRow[];
}

export interface GenderBreakdownRow {
  gender: string;
  count: number;
}

export interface AgeBandRow {
  band: string;
  count: number;
}

export interface BloodGroupRow {
  bloodGroup: string;
  count: number;
}

export interface PatientDemographics {
  totalPatients: number;
  gender: GenderBreakdownRow[];
  ageBands: AgeBandRow[];
  bloodGroups: BloodGroupRow[];
}
