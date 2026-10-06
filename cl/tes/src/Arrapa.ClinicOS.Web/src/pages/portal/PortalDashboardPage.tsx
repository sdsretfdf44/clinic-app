import { Link } from 'react-router-dom';
import {
  CalendarDaysIcon,
  ClockIcon,
  BeakerIcon,
  CurrencyDollarIcon,
  PlusCircleIcon,
  ChevronRightIcon,
  ArrowRightIcon,
} from '@heroicons/react/24/outline';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  Card,
  PageHeader,
  SkeletonCard,
  StatCard,
} from '@/components/ui';
import { useCurrentUser } from '@/features/auth/hooks';
import {
  PORTAL_APPT_STATUS_LABEL,
  PORTAL_APPT_STATUS_TONE,
  usePortalDashboard,
} from '@/features/portal';

export default function PortalDashboardPage() {
  const currentUser = useCurrentUser();
  const dashboard = usePortalDashboard();

  if (dashboard.isPending) {
    return (
      <div className="space-y-6">
        <PageHeader eyebrow="Patient portal" title="Welcome" description="Loading your portal…" />
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <SkeletonCard key={i} lines={1} />
          ))}
        </div>
      </div>
    );
  }
  if (dashboard.isError) {
    return (
      <div className="space-y-6">
        <PageHeader eyebrow="Patient portal" title="Welcome" />
        <Alert variant="error">{humanizeError(dashboard.error)}</Alert>
      </div>
    );
  }

  const d = dashboard.data!;
  const user = currentUser.data;

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Patient portal"
        title={`Hello${user ? `, ${user.firstName}` : ''}.`}
        description="View your upcoming appointments, prescriptions, and invoices — or request a new appointment."
        actions={
          <Link to="/portal/appointments/new">
            <Button leftIcon={<PlusCircleIcon className="h-4 w-4" />}>Request appointment</Button>
          </Link>
        }
      />

      {/* KPIs */}
      <section className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Upcoming appointments"
          value={d.upcomingAppointmentCount.toString()}
          icon={<CalendarDaysIcon />}
          tone="brand"
        />
        <StatCard
          label="Pending requests"
          value={d.pendingRequestCount.toString()}
          icon={<ClockIcon />}
          tone={d.pendingRequestCount > 0 ? 'warning' : 'gray'}
        />
        <StatCard
          label="Prescriptions"
          value={d.prescriptionCount.toString()}
          icon={<BeakerIcon />}
          tone="teal"
        />
        <StatCard
          label="Outstanding balance"
          value={`${d.outstandingAmount.toFixed(2)} ${d.currency}`}
          hint={d.outstandingInvoiceCount > 0 ? `${d.outstandingInvoiceCount} invoice(s)` : 'No unpaid invoices'}
          icon={<CurrencyDollarIcon />}
          tone={d.outstandingAmount > 0 ? 'warning' : 'success'}
        />
      </section>

      {/* Next appointment */}
      <Card padding="lg">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-title text-gray-900 dark:text-white">Next appointment</h3>
          <Link
            to="/portal/appointments"
            className="text-caption font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400"
          >
            View all →
          </Link>
        </div>
        {d.nextAppointment ? (
          <Link
            to={`/portal/appointments/${d.nextAppointment.id}`}
            className="group block rounded-xl border border-gray-200/70 dark:border-gray-800 hover:border-brand-300 dark:hover:border-brand-500/40 p-4 transition-colors"
          >
            <div className="flex items-center justify-between gap-4 flex-wrap">
              <div>
                <p className="text-title text-gray-900 dark:text-white">
                  {new Date(d.nextAppointment.scheduledStart).toLocaleString(undefined, {
                    dateStyle: 'full',
                    timeStyle: 'short',
                  })}
                </p>
                <p className="mt-1 text-body text-gray-500 dark:text-gray-400">
                  {d.nextAppointment.doctorName}
                  {d.nextAppointment.doctorSpecialty ? ` · ${d.nextAppointment.doctorSpecialty}` : ''}
                </p>
                {d.nextAppointment.reason && (
                  <p className="mt-1 text-caption text-gray-500 dark:text-gray-400">Reason: {d.nextAppointment.reason}</p>
                )}
              </div>
              <div className="flex items-center gap-3">
                <Badge tone={PORTAL_APPT_STATUS_TONE[d.nextAppointment.status]}>
                  {PORTAL_APPT_STATUS_LABEL[d.nextAppointment.status]}
                </Badge>
                <ChevronRightIcon className="h-4 w-4 text-gray-300 group-hover:text-gray-500" />
              </div>
            </div>
          </Link>
        ) : (
          <div className="rounded-xl border border-dashed border-gray-200 dark:border-gray-800 p-6 text-center">
            <p className="text-body text-gray-600 dark:text-gray-300">You have no upcoming appointments.</p>
            <Link to="/portal/appointments/new" className="mt-2 inline-flex items-center gap-1.5 text-body font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400">
              Request one <ArrowRightIcon className="h-3.5 w-3.5" />
            </Link>
          </div>
        )}
      </Card>

      {/* Quick links */}
      <section className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <QuickLink
          to="/portal/appointments"
          title="Appointments"
          description="View history and manage requests"
          icon={<CalendarDaysIcon />}
        />
        <QuickLink
          to="/portal/prescriptions"
          title="Prescriptions"
          description="Your medication history"
          icon={<BeakerIcon />}
        />
        <QuickLink
          to="/portal/invoices"
          title="Invoices"
          description="Payments and balances"
          icon={<CurrencyDollarIcon />}
        />
      </section>
    </div>
  );
}

function QuickLink({
  to,
  title,
  description,
  icon,
}: {
  to: string;
  title: string;
  description: string;
  icon: React.ReactNode;
}) {
  return (
    <Link
      to={to}
      className="group rounded-xl border border-gray-200/70 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 shadow-theme-xs hover:border-brand-300 dark:hover:border-brand-500/40 transition-colors"
    >
      <div className="flex items-start gap-3">
        <span className="inline-flex h-10 w-10 items-center justify-center rounded-lg bg-brand-50 text-brand-600 dark:bg-brand-500/10 dark:text-brand-300 [&>svg]:h-5 [&>svg]:w-5">
          {icon}
        </span>
        <div className="flex-1 min-w-0">
          <p className="text-body font-semibold text-gray-900 dark:text-white">{title}</p>
          <p className="mt-0.5 text-caption text-gray-500 dark:text-gray-400">{description}</p>
        </div>
        <ArrowRightIcon className="h-4 w-4 text-gray-300 group-hover:text-brand-500 transition-colors" />
      </div>
    </Link>
  );
}
