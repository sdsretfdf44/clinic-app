import { Link } from 'react-router-dom';
import {
  CalendarDaysIcon,
  ClockIcon,
  CheckCircleIcon,
  XCircleIcon,
  UsersIcon,
  ChartBarIcon,
  ArrowUpRightIcon,
  ChevronRightIcon,
  HeartIcon,
} from '@heroicons/react/24/outline';

import { extractApiError, humanizeError } from '@/api/httpClient';
import {
  Alert,
  Avatar,
  Badge,
  Button,
  Card,
  EmptyState,
  PageHeader,
  SkeletonCard,
  StatCard,
  TrendChart,
} from '@/components/ui';
import { useDoctorDashboard } from '@/features/doctor/dashboard';

import { STATUS_LABEL, STATUS_TONE } from '../appointments/statusMeta';

export default function DoctorDashboardPage() {
  const dash = useDoctorDashboard();

  if (dash.isPending) {
    return (
      <div className="space-y-6">
        <PageHeader eyebrow="Doctor" title="Overview" description="Loading your dashboard…" />
        <div className="grid grid-cols-2 sm:grid-cols-5 gap-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <SkeletonCard key={i} lines={1} />
          ))}
        </div>
      </div>
    );
  }
  if (dash.isError || !dash.data) {
    const apiError = extractApiError(dash.error);
    if (apiError?.code === 'doctor.no_record') {
      return (
        <div className="space-y-6">
          <PageHeader
            eyebrow="Doctor"
            title="Overview"
            description="This workspace is for accounts linked to a doctor profile."
          />
          <EmptyState
            icon={<HeartIcon />}
            title="No doctor profile linked to your account"
            description="This dashboard shows a clinician's appointments, revenue, and patient history. Add a doctor profile from Clinic → Doctors, or sign in as a doctor account to view it."
            action={
              <Button variant="primary" onClick={() => window.location.assign('/clinic/doctors')}>
                Open doctors
              </Button>
            }
            secondaryAction={
              <Button variant="secondary" onClick={() => window.location.assign('/dashboard')}>
                Back to dashboard
              </Button>
            }
          />
        </div>
      );
    }
    return (
      <div className="space-y-6">
        <PageHeader eyebrow="Doctor" title="Overview" />
        <Alert variant="error">{humanizeError(dash.error)}</Alert>
      </div>
    );
  }
  const d = dash.data;
  const currency = new Intl.NumberFormat(undefined, { style: 'currency', currency: d.doctor.currency, maximumFractionDigits: 0 });

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Doctor"
        title={`Welcome, ${d.doctor.fullName}.`}
        description={`${d.doctor.specialty ?? 'General practice'}${d.doctor.departmentName ? ` · ${d.doctor.departmentName}` : ''}`}
      />

      {/* Today */}
      <section>
        <div className="flex items-center justify-between mb-3">
          <h2 className="u-label">Today</h2>
          <Link
            to="/doctor/queue"
            className="text-caption font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400 inline-flex items-center gap-1"
          >
            Open today's queue <ArrowUpRightIcon className="h-3 w-3" />
          </Link>
        </div>
        <div className="grid grid-cols-2 sm:grid-cols-5 gap-4">
          <StatCard label="Total" value={d.today.total.toString()} icon={<CalendarDaysIcon />} tone="gray" />
          <StatCard label="Remaining" value={d.today.remaining.toString()} icon={<ClockIcon />} tone="brand" />
          <StatCard label="In consultation" value={d.today.inConsultation.toString()} icon={<ChartBarIcon />} tone="warning" />
          <StatCard label="Completed" value={d.today.completed.toString()} icon={<CheckCircleIcon />} tone="success" />
          <StatCard label="No-show" value={d.today.noShow.toString()} icon={<XCircleIcon />} tone={d.today.noShow > 0 ? 'danger' : 'gray'} />
        </div>
      </section>

      {/* Trend */}
      <Card padding="lg">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h3 className="text-title text-gray-900 dark:text-white">Your appointments — last 30 days</h3>
            <p className="mt-0.5 text-caption text-gray-500 dark:text-gray-400">Scheduled vs. completed</p>
          </div>
        </div>
        <TrendChart points={d.appointmentTrend} ariaLabel="Doctor appointments last 30 days" />
      </Card>

      {/* This week / month */}
      <section className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <RangeCard label="This week" range={d.thisWeek} currency={currency} />
        <RangeCard label="This month" range={d.thisMonth} currency={currency} />
      </section>

      {/* Lifetime + upcoming */}
      <section className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <Card padding="lg">
          <p className="u-label mb-2">Lifetime</p>
          <p className="text-display text-gray-900 dark:text-white tabular-nums">
            {d.lifetime.totalCompleted.toLocaleString()}
          </p>
          <p className="text-caption text-gray-500 dark:text-gray-400">Consultations completed</p>
          <div className="mt-5 pt-4 border-t border-gray-100 dark:border-gray-800 flex items-center gap-3">
            <span className="inline-flex h-9 w-9 items-center justify-center rounded-lg bg-teal-50 text-teal-600 dark:bg-teal-500/10 dark:text-teal-300">
              <UsersIcon className="h-4 w-4" />
            </span>
            <div>
              <p className="text-title text-gray-900 dark:text-white tabular-nums">
                {d.lifetime.uniquePatients.toLocaleString()}
              </p>
              <p className="text-caption text-gray-500 dark:text-gray-400">Unique patients seen</p>
            </div>
          </div>
        </Card>

        <Card padding="lg" className="lg:col-span-2">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h3 className="text-title text-gray-900 dark:text-white">Next up</h3>
              <p className="mt-0.5 text-caption text-gray-500 dark:text-gray-400">Your next 5 scheduled appointments</p>
            </div>
          </div>
          {d.nextAppointments.length === 0 ? (
            <p className="text-body text-gray-500">No upcoming appointments.</p>
          ) : (
            <ul className="divide-y divide-gray-100 dark:divide-gray-800 -mx-2">
              {d.nextAppointments.map((a) => (
                <li key={a.id}>
                  <Link
                    to={`/appointments/${a.id}`}
                    className="group flex items-center gap-3 py-3 px-2 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
                  >
                    <Avatar name={a.patientName} size="sm" />
                    <div className="min-w-0 flex-1">
                      <p className="text-body font-medium text-gray-900 dark:text-white truncate">{a.patientName}</p>
                      <p className="text-caption text-gray-500 dark:text-gray-400 font-mono">{a.patientMrn}</p>
                      {a.reason && <p className="text-caption text-gray-500 dark:text-gray-400 truncate mt-0.5">{a.reason}</p>}
                    </div>
                    <div className="text-right shrink-0">
                      <p className="text-body text-gray-900 dark:text-gray-100 tabular-nums">
                        {new Date(a.scheduledStart).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })}
                      </p>
                      <div className="mt-1 flex items-center justify-end gap-2">
                        <Badge tone={STATUS_TONE[a.status]}>{STATUS_LABEL[a.status]}</Badge>
                      </div>
                    </div>
                    <ChevronRightIcon className="h-4 w-4 text-gray-300 group-hover:text-gray-500" />
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </Card>
      </section>
    </div>
  );
}

function RangeCard({
  label,
  range,
  currency,
}: {
  label: string;
  range: { booked: number; completed: number; cancelled: number; noShow: number; consultationRevenue: number };
  currency: Intl.NumberFormat;
}) {
  const completionRate = range.booked > 0 ? Math.round((range.completed / range.booked) * 100) : 0;
  return (
    <Card padding="lg">
      <div className="flex items-baseline justify-between mb-4">
        <h3 className="text-title text-gray-900 dark:text-white">{label}</h3>
        <span className="text-caption text-gray-500 dark:text-gray-400 tabular-nums">
          {completionRate}% completion
        </span>
      </div>
      <div className="grid grid-cols-2 gap-4">
        <MiniStat label="Booked" value={range.booked} tone="gray" />
        <MiniStat label="Completed" value={range.completed} tone="success" />
        <MiniStat label="Cancelled" value={range.cancelled} tone="gray" />
        <MiniStat label="No-show" value={range.noShow} tone={range.noShow > 0 ? 'danger' : 'gray'} />
      </div>
      <div className="mt-5 pt-4 border-t border-gray-100 dark:border-gray-800 flex items-center justify-between">
        <p className="u-label">Consultation revenue</p>
        <p className="text-title text-gray-900 dark:text-white tabular-nums">
          {currency.format(range.consultationRevenue)}
        </p>
      </div>
    </Card>
  );
}

function MiniStat({ label, value, tone }: { label: string; value: number; tone: 'gray' | 'success' | 'danger' }) {
  const cls =
    tone === 'success' ? 'text-success-700 dark:text-success-500' :
    tone === 'danger' ? 'text-danger-700 dark:text-danger-500' :
    'text-gray-900 dark:text-white';
  return (
    <div>
      <p className={`text-title-lg tabular-nums ${cls}`}>{value}</p>
      <p className="text-caption text-gray-500 dark:text-gray-400">{label}</p>
    </div>
  );
}
