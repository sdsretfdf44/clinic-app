import { Route, Routes } from 'react-router-dom';

import { DashboardLayout } from '@/layouts/DashboardLayout';
import DashboardPage from '@/pages/DashboardPage';
import MarketingHome from '@/pages/MarketingHome';
import NotFoundPage from '@/pages/NotFoundPage';
import AdminDashboardPage from '@/pages/admin/AdminDashboardPage';
import AuditLogsPage from '@/pages/admin/AuditLogsPage';
import FeatureFlagsPage from '@/pages/admin/FeatureFlagsPage';
import GlobalSettingsPage from '@/pages/admin/GlobalSettingsPage';
import SubscriptionPlansPage from '@/pages/admin/SubscriptionPlansPage';
import UsersListPage from '@/pages/admin/UsersListPage';
import NewTenantPage from '@/pages/admin/tenants/NewTenantPage';
import TenantDetailPage from '@/pages/admin/tenants/TenantDetailPage';
import TenantsListPage from '@/pages/admin/tenants/TenantsListPage';
import ForgotPasswordPage from '@/pages/auth/ForgotPasswordPage';
import RegisterPage from '@/pages/auth/RegisterPage';
import ResetPasswordPage from '@/pages/auth/ResetPasswordPage';
import SignInPage from '@/pages/auth/SignInPage';
import VerifyEmailPage from '@/pages/auth/VerifyEmailPage';
import ClinicDashboardPage from '@/pages/clinic/ClinicDashboardPage';
import ClinicProfilePage from '@/pages/clinic/ClinicProfilePage';
import ClinicSettingsPage from '@/pages/clinic/ClinicSettingsPage';
import ClinicUsersPage from '@/pages/clinic/ClinicUsersPage';
import DepartmentsPage from '@/pages/clinic/DepartmentsPage';
import DoctorsPage from '@/pages/clinic/DoctorsPage';
import ReceptionistsPage from '@/pages/clinic/ReceptionistsPage';
import WorkingHoursPage from '@/pages/clinic/WorkingHoursPage';
import AppointmentDetailPage from '@/pages/appointments/AppointmentDetailPage';
import AppointmentsListPage from '@/pages/appointments/AppointmentsListPage';
import NewAppointmentPage from '@/pages/appointments/NewAppointmentPage';
import ConsultationPage from '@/pages/doctor/ConsultationPage';
import DoctorDashboardPage from '@/pages/doctor/DoctorDashboardPage';
import TodayQueuePage from '@/pages/doctor/TodayQueuePage';
import InvoiceDetailPage from '@/pages/billing/InvoiceDetailPage';
import InvoicesListPage from '@/pages/billing/InvoicesListPage';
import NewInvoicePage from '@/pages/billing/NewInvoicePage';
import PortalAppointmentDetailPage from '@/pages/portal/PortalAppointmentDetailPage';
import PortalAppointmentsListPage from '@/pages/portal/PortalAppointmentsListPage';
import PortalDashboardPage from '@/pages/portal/PortalDashboardPage';
import PortalInvoiceDetailPage from '@/pages/portal/PortalInvoiceDetailPage';
import PortalInvoicesPage from '@/pages/portal/PortalInvoicesPage';
import PortalPrescriptionDetailPage from '@/pages/portal/PortalPrescriptionDetailPage';
import PortalPrescriptionsPage from '@/pages/portal/PortalPrescriptionsPage';
import PortalProfilePage from '@/pages/portal/PortalProfilePage';
import RequestAppointmentPage from '@/pages/portal/RequestAppointmentPage';
import ReceptionPatientSearchPage from '@/pages/reception/PatientSearchPage';
import ReceptionDashboardPage from '@/pages/reception/ReceptionDashboardPage';
import WalkInRegistrationPage from '@/pages/reception/WalkInRegistrationPage';
import NewPatientPage from '@/pages/patients/NewPatientPage';
import PatientDetailPage from '@/pages/patients/PatientDetailPage';
import PatientsListPage from '@/pages/patients/PatientsListPage';
import ChangePasswordPage from '@/pages/profile/ChangePasswordPage';
import ProfilePage from '@/pages/profile/ProfilePage';
import TwoFactorPage from '@/pages/profile/TwoFactorPage';
import NotificationsLogPage from '@/pages/notifications/NotificationsLogPage';
import ReportsPage from '@/pages/reports/ReportsPage';
import { ProtectedRoute } from '@/routes/ProtectedRoute';
import { RoleRoute } from '@/routes/RoleRoute';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<MarketingHome />} />
      <Route path="/sign-in" element={<SignInPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/verify-email" element={<VerifyEmailPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<DashboardLayout />}>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/change-password" element={<ChangePasswordPage />} />
          <Route path="/two-factor" element={<TwoFactorPage />} />

          <Route element={<RoleRoute allow={['SuperAdmin']} />}>
            <Route path="/admin" element={<AdminDashboardPage />} />
            <Route path="/admin/tenants" element={<TenantsListPage />} />
            <Route path="/admin/tenants/new" element={<NewTenantPage />} />
            <Route path="/admin/tenants/:id" element={<TenantDetailPage />} />
            <Route path="/admin/plans" element={<SubscriptionPlansPage />} />
            <Route path="/admin/users" element={<UsersListPage />} />
            <Route path="/admin/feature-flags" element={<FeatureFlagsPage />} />
            <Route path="/admin/settings" element={<GlobalSettingsPage />} />
            <Route path="/admin/audit-logs" element={<AuditLogsPage />} />
          </Route>

          <Route element={<RoleRoute allow={['ClinicAdmin', 'SuperAdmin']} />}>
            <Route path="/clinic" element={<ClinicDashboardPage />} />
            <Route path="/clinic/profile" element={<ClinicProfilePage />} />
            <Route path="/clinic/departments" element={<DepartmentsPage />} />
            <Route path="/clinic/doctors" element={<DoctorsPage />} />
            <Route path="/clinic/receptionists" element={<ReceptionistsPage />} />
            <Route path="/clinic/users" element={<ClinicUsersPage />} />
            <Route path="/clinic/working-hours" element={<WorkingHoursPage />} />
            <Route path="/clinic/settings" element={<ClinicSettingsPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/notifications" element={<NotificationsLogPage />} />
          </Route>

          <Route element={<RoleRoute allow={['ClinicAdmin', 'Doctor', 'Receptionist', 'SuperAdmin']} />}>
            <Route path="/patients" element={<PatientsListPage />} />
            <Route path="/patients/new" element={<NewPatientPage />} />
            <Route path="/patients/:id" element={<PatientDetailPage />} />

            <Route path="/appointments" element={<AppointmentsListPage />} />
            <Route path="/appointments/new" element={<NewAppointmentPage />} />
            <Route path="/appointments/:id" element={<AppointmentDetailPage />} />
          </Route>

          <Route element={<RoleRoute allow={['Doctor', 'SuperAdmin']} />}>
            <Route path="/doctor" element={<DoctorDashboardPage />} />
            <Route path="/doctor/queue" element={<TodayQueuePage />} />
            <Route path="/doctor/consultation/:appointmentId" element={<ConsultationPage />} />
          </Route>

          <Route element={<RoleRoute allow={['Receptionist', 'ClinicAdmin', 'SuperAdmin']} />}>
            <Route path="/reception" element={<ReceptionDashboardPage />} />
            <Route path="/reception/walk-in" element={<WalkInRegistrationPage />} />
            <Route path="/reception/search" element={<ReceptionPatientSearchPage />} />

            <Route path="/billing" element={<InvoicesListPage />} />
            <Route path="/billing/new" element={<NewInvoicePage />} />
            <Route path="/billing/:id" element={<InvoiceDetailPage />} />
          </Route>

          <Route element={<RoleRoute allow={['Patient']} />}>
            <Route path="/portal" element={<PortalDashboardPage />} />
            <Route path="/portal/profile" element={<PortalProfilePage />} />
            <Route path="/portal/appointments" element={<PortalAppointmentsListPage />} />
            <Route path="/portal/appointments/new" element={<RequestAppointmentPage />} />
            <Route path="/portal/appointments/:id" element={<PortalAppointmentDetailPage />} />
            <Route path="/portal/prescriptions" element={<PortalPrescriptionsPage />} />
            <Route path="/portal/prescriptions/:id" element={<PortalPrescriptionDetailPage />} />
            <Route path="/portal/invoices" element={<PortalInvoicesPage />} />
            <Route path="/portal/invoices/:id" element={<PortalInvoiceDetailPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
