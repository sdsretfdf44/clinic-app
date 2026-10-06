import { Link } from 'react-router-dom';
import {
  CalendarDaysIcon,
  ClockIcon,
  UsersIcon,
  CurrencyDollarIcon,
  ArrowTrendingUpIcon,
  BanknotesIcon,
  BuildingOffice2Icon,
  UserGroupIcon,
  ChartBarIcon,
} from '@heroicons/react/24/outline';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Avatar,
  Badge,
  Card,
  PageHeader,
  SkeletonCard,
  StatCard,
  TrendChart,
} from '@/components/ui';
import { useClinicDashboard } from '@/features/clinic/dashboard';

export default function ClinicDashboardPage() {
  const dash = useClinicDashboard();

  if (dash.isPending) {
    return (
      <div className="space-y-6">
        <PageHeader eyebrow="Clinic admin" title="Clinic overview" description="Loading operational KPIs…" />
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
          {Array.from({ length: 6 }).map((_, i) => (
            <SkeletonCard key={i} lines={1} />
          ))}
        </div>
      </div>
    );
  }
  if (dash.isError || !dash.data) {
    return (
      <div className="space-y-6">
        <PageHeader eyebrow="Clinic admin" title="Clinic overview" />
        <Alert variant="error">{humanizeError(dash.error)}</Alert>
      </div>
    );
  }
  const d = dash.data;
  const currency = new Intl.NumberFormat(undefined, { style: 'currency', currency: d.currency, maximumFractionDigits: 0 });

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Clinic admin"
        title="Clinic overview"
        description="Live operational KPIs across appointments, revenue, patients, and staff."
      />

      {d.appointments.pendingRequests > 0 && (
        <Alert
          variant="info"
          action={
            <Link
              to="/appointments?status=Requested"
              className="text-body font-medium text-brand-700 hover:text-brand-800 dark:text-brand-300 whitespace-nowrap"
            >
              Review →
            </Link>
          }
        >
          {d.appointments.pendingRequests} pending appointment{' '}
          {d.appointments.pendingRequests === 1 ? 'request' : 'requests'} awaiting approval.
        </Alert>
      )}

      {/* Appointment KPIs */}
      <section>
        <h2 className="u-label mb-3">Appointments</h2>
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
          <StatCard label="Today" value={d.appointments.today.toString()} icon={<CalendarDaysIcon />} tone="brand" />
          <StatCard label="Tomorrow" value={d.appointments.tomorrow.toString()} icon={<CalendarDaysIcon />} tone="teal" />
          <StatCard label="This week" value={d.appointments.thisWeek.toString()} icon={<CalendarDaysIcon />} tone="gray" />
          <StatCard label="This month" value={d.appointments.thisMonth.toString()} icon={<CalendarDaysIcon />} tone="gray" />
          <StatCard
            label="Pending req."
            value={d.appointments.pendingRequests.toString()}
            icon={<ClockIcon />}
            tone={d.appointments.pendingRequests > 0 ? 'warning' : 'gray'}
          />
          <StatCard
            label="No-show 30d"
            value={`${d.appointments.noShowRatePct.toFixed(1)}%`}
            icon={<ArrowTrendingUpIcon />}
            tone={d.appointments.noShowRatePct > 10 ? 'danger' : 'gray'}
          />
        </div>
      </section>

      {/* Trend chart */}
      <Card padding="lg">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h3 className="text-title text-gray-900 dark:text-white">Appointments — last 30 days</h3>
            <p className="mt-0.5 text-caption text-gray-500 dark:text-gray-400">Scheduled vs. completed</p>
          </div>
          <Link
            to="/appointments"
            className="text-caption font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400"
          >
            Open schedule →
          </Link>
        </div>
        <TrendChart points={d.appointmentTrend} ariaLabel="Appointments last 30 days" />
      </Card>

      {/* Revenue + patients */}
      <section className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <StatCard
          label="Issued this month"
          value={currency.format(d.revenue.issuedThisMonth)}
          hint={`Collected ${currency.format(d.revenue.collectedThisMonth)}`}
          icon={<CurrencyDollarIcon />}
          tone="brand"
        />
        <StatCard
          label="Outstanding balance"
          value={currency.format(d.revenue.outstandingTotal)}
          hint={`${d.revenue.outstandingInvoiceCount} unpaid ${d.revenue.outstandingInvoiceCount === 1 ? 'invoice' : 'invoices'}`}
          icon={<BanknotesIcon />}
          tone={d.revenue.outstandingTotal > 0 ? 'warning' : 'success'}
        />
        <StatCard
          label="Active patients"
          value={d.patients.totalActive.toLocaleString()}
          hint={`${d.patients.newThisMonth} new this month`}
          icon={<UsersIcon />}
          tone="teal"
        />
      </section>

      {/* Top doctors + departments */}
      <section className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <Card padding="lg">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h3 className="text-title text-gray-900 dark:text-white">Top doctors this month</h3>
              <p className="mt-0.5 text-caption text-gray-500 dark:text-gray-400">By completed consultations</p>
            </div>
            <Link
              to="/clinic/doctors"
              className="text-caption font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400"
            >
              All doctors →
            </Link>
          </div>
          {d.topDoctors.length === 0 ? (
            <p className="text-body text-gray-500">No completed consultations yet this month.</p>
          ) : (
            <ul className="space-y-3">
              {d.topDoctors.map((doc, idx) => (
                <li key={doc.doctorId} className="flex items-center gap-3">
                  <span className="inline-flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-brand-50 dark:bg-brand-500/10 text-brand-700 dark:text-brand-300 text-caption font-semibold">
                    {idx + 1}
                  </span>
                  <Avatar name={doc.doctorName} size="sm" />
                  <div className="min-w-0 flex-1">
                    <p className="text-body font-medium text-gray-900 dark:text-white truncate">{doc.doctorName}</p>
                    <p className="text-caption text-gray-500 dark:text-gray-400 truncate">{doc.specialty ?? '—'}</p>
                  </div>
                  <div className="text-right">
                    <p className="text-body text-gray-900 dark:text-gray-100 tabular-nums">{doc.completedThisMonth} visits</p>
                    <p className="text-caption text-gray-500 dark:text-gray-400 tabular-nums">{currency.format(doc.revenueThisMonth)}</p>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </Card>

        <Card padding="lg">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h3 className="text-title text-gray-900 dark:text-white">Departments</h3>
              <p className="mt-0.5 text-caption text-gray-500 dark:text-gray-400">Consultation fees</p>
            </div>
            <Link
              to="/clinic/departments"
              className="text-caption font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400"
            >
              Manage →
            </Link>
          </div>
          {d.departmentBreakdown.length === 0 ? (
            <p className="text-body text-gray-500">
              No departments yet.{' '}
              <Link to="/clinic/departments" className="u-link">
                Create one →
              </Link>
            </p>
          ) : (
            <ul className="space-y-3">
              {d.departmentBreakdown.map((dep) => (
                <li key={dep.id} className="flex items-center justify-between gap-3">
                  <div className="flex items-center gap-3 min-w-0">
                    <span className="inline-flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-300">
                      <BuildingOffice2Icon className="h-4 w-4" />
                    </span>
                    <div className="min-w-0">
                      <p className="text-body font-medium text-gray-900 dark:text-white truncate">{dep.name}</p>
                      <p className="text-caption text-gray-500 dark:text-gray-400">
                        {dep.doctorCount} {dep.doctorCount === 1 ? 'doctor' : 'doctors'}
                      </p>
                    </div>
                  </div>
                  <Badge tone="brand">
                    {dep.defaultConsultationFee != null ? currency.format(dep.defaultConsultationFee) : '—'}
                  </Badge>
                </li>
              ))}
            </ul>
          )}
        </Card>
      </section>

      {/* Staff */}
      <section>
        <h2 className="u-label mb-3">Staff</h2>
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
          <StatCard label="Doctors" value={d.staff.doctors.toString()} icon={<UserGroupIcon />} tone="brand" />
          <StatCard label="Receptionists" value={d.staff.receptionists.toString()} icon={<UsersIcon />} tone="teal" />
          <StatCard label="Clinic admins" value={d.staff.clinicAdmins.toString()} icon={<ChartBarIcon />} tone="gray" />
          <StatCard label="Total users" value={d.staff.totalUsers.toString()} hint={`${d.staff.activeUsers} active`} icon={<UsersIcon />} tone="gray" />
        </div>
      </section>
    </div>
  );
}
