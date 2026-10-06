import { useMemo, useState } from 'react';
import {
  BanknotesIcon,
  CalendarDaysIcon,
  ChartBarIcon,
  CurrencyDollarIcon,
  DocumentTextIcon,
  UserGroupIcon,
  UsersIcon,
} from '@heroicons/react/24/outline';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Avatar,
  Badge,
  Card,
  PageHeader,
  Select,
  SkeletonCard,
  StatCard,
  Tabs,
  type TabDef,
  TrendChart,
} from '@/components/ui';
import {
  useAppointmentsFunnel,
  usePatientDemographics,
  useRevenueSummary,
  useTopDoctorsReport,
  useTopServicesReport,
} from '@/features/reports';

type TabKey = 'revenue' | 'appointments' | 'doctors' | 'services' | 'patients';
type RangeKey = '7d' | '30d' | '90d' | '365d';

const rangeOptions: { value: RangeKey; label: string; days: number }[] = [
  { value: '7d', label: 'Last 7 days', days: 7 },
  { value: '30d', label: 'Last 30 days', days: 30 },
  { value: '90d', label: 'Last 90 days', days: 90 },
  { value: '365d', label: 'Last 12 months', days: 365 },
];

function useDateRange(range: RangeKey) {
  return useMemo(() => {
    const opt = rangeOptions.find((o) => o.value === range) ?? rangeOptions[1]!;
    const to = new Date();
    to.setUTCHours(0, 0, 0, 0);
    to.setUTCDate(to.getUTCDate() + 1);
    const from = new Date(to);
    from.setUTCDate(from.getUTCDate() - opt.days);
    return { from: from.toISOString(), to: to.toISOString() };
  }, [range]);
}

export default function ReportsPage() {
  const [tab, setTab] = useState<TabKey>('revenue');
  const [range, setRange] = useState<RangeKey>('30d');
  const dates = useDateRange(range);

  const tabs: TabDef<TabKey>[] = [
    { value: 'revenue', label: 'Revenue', icon: <CurrencyDollarIcon className="h-4 w-4" /> },
    { value: 'appointments', label: 'Appointments', icon: <CalendarDaysIcon className="h-4 w-4" /> },
    { value: 'doctors', label: 'Top doctors', icon: <UserGroupIcon className="h-4 w-4" /> },
    { value: 'services', label: 'Top services', icon: <DocumentTextIcon className="h-4 w-4" /> },
    { value: 'patients', label: 'Demographics', icon: <UsersIcon className="h-4 w-4" /> },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Reports"
        title="Analytics & insights"
        description="Revenue, appointments, top performers, and patient demographics."
      />

      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <Tabs<TabKey> tabs={tabs} value={tab} onChange={setTab} variant="pills" />
        {tab !== 'patients' && (
          <div className="w-full sm:w-56">
            <Select
              value={range}
              onChange={(e) => setRange(e.target.value as RangeKey)}
              aria-label="Date range"
            >
              {rangeOptions.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </Select>
          </div>
        )}
      </div>

      {tab === 'revenue' && <RevenueTab from={dates.from} to={dates.to} />}
      {tab === 'appointments' && <AppointmentsTab from={dates.from} to={dates.to} />}
      {tab === 'doctors' && <TopDoctorsTab from={dates.from} to={dates.to} />}
      {tab === 'services' && <TopServicesTab from={dates.from} to={dates.to} />}
      {tab === 'patients' && <PatientDemographicsTab />}
    </div>
  );
}

// -------------------- Revenue tab --------------------
function RevenueTab({ from, to }: { from: string; to: string }) {
  const q = useRevenueSummary({ from, to });
  if (q.isPending) {
    return <SkeletonBlock rows={3} />;
  }
  if (q.isError || !q.data) {
    return <Alert variant="error">{humanizeError(q.error)}</Alert>;
  }
  const d = q.data;
  const money = new Intl.NumberFormat(undefined, { style: 'currency', currency: d.currency, maximumFractionDigits: 0 });
  const chartPoints = d.series.map((s) => ({ date: s.date, total: s.issued, completed: s.collected }));

  return (
    <div className="space-y-6">
      <section className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label="Issued" value={money.format(d.issuedTotal)} icon={<CurrencyDollarIcon />} tone="brand" />
        <StatCard label="Collected" value={money.format(d.collectedTotal)} icon={<BanknotesIcon />} tone="success" />
        <StatCard
          label="Outstanding"
          value={money.format(d.outstandingTotal)}
          icon={<BanknotesIcon />}
          tone={d.outstandingTotal > 0 ? 'warning' : 'gray'}
        />
        <StatCard
          label="Invoices"
          value={d.invoiceCount.toLocaleString()}
          hint={`${d.paidInvoiceCount} paid`}
          icon={<DocumentTextIcon />}
          tone="gray"
        />
      </section>

      <Card padding="lg">
        <div className="mb-4">
          <h3 className="text-title text-gray-900 dark:text-white">Revenue trend</h3>
          <p className="mt-0.5 text-caption text-gray-500 dark:text-gray-400">
            Blue = issued · Teal = collected
          </p>
        </div>
        <TrendChart points={chartPoints} ariaLabel="Revenue over time" height={180} />
      </Card>
    </div>
  );
}

// -------------------- Appointments tab --------------------
function AppointmentsTab({ from, to }: { from: string; to: string }) {
  const q = useAppointmentsFunnel({ from, to });
  if (q.isPending) return <SkeletonBlock rows={2} />;
  if (q.isError || !q.data) return <Alert variant="error">{humanizeError(q.error)}</Alert>;

  const d = q.data;
  const total = d.booked + d.confirmed + d.checkedIn + d.completed + d.cancelled + d.noShow;

  return (
    <div className="space-y-6">
      <section className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
        <StatCard label="Booked" value={d.booked.toString()} tone="gray" />
        <StatCard label="Confirmed" value={d.confirmed.toString()} tone="brand" />
        <StatCard label="Checked in" value={d.checkedIn.toString()} tone="teal" />
        <StatCard label="Completed" value={d.completed.toString()} tone="success" />
        <StatCard label="Cancelled" value={d.cancelled.toString()} tone="warning" />
        <StatCard label="No-show" value={d.noShow.toString()} tone="danger" />
      </section>

      <Card padding="lg">
        <div className="mb-4">
          <h3 className="text-title text-gray-900 dark:text-white">Outcome rates</h3>
          <p className="mt-0.5 text-caption text-gray-500 dark:text-gray-400">
            Based on resolved appointments (completed + cancelled + no-show) in this range.
          </p>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <RateBar label="Completion rate" pct={d.completionRatePct} tone="success" />
          <RateBar label="Cancellation rate" pct={d.cancellationRatePct} tone="warning" />
          <RateBar label="No-show rate" pct={d.noShowRatePct} tone="danger" />
        </div>
        <p className="mt-4 text-caption text-gray-500 dark:text-gray-400">
          Total appointments in range: <span className="tabular-nums font-medium">{total.toLocaleString()}</span>
        </p>
      </Card>
    </div>
  );
}

function RateBar({ label, pct, tone }: { label: string; pct: number; tone: 'success' | 'warning' | 'danger' }) {
  const barColor =
    tone === 'success' ? 'bg-success-500' : tone === 'warning' ? 'bg-warning-500' : 'bg-error-500';
  const width = Math.min(100, Math.max(0, pct));
  return (
    <div>
      <div className="flex items-baseline justify-between">
        <p className="text-caption text-gray-500 dark:text-gray-400">{label}</p>
        <p className="text-title text-gray-900 dark:text-white tabular-nums">{pct.toFixed(1)}%</p>
      </div>
      <div className="mt-2 h-2 rounded-full bg-gray-100 dark:bg-gray-800 overflow-hidden">
        <div className={`${barColor} h-full rounded-full`} style={{ width: `${width}%` }} />
      </div>
    </div>
  );
}

// -------------------- Top doctors tab --------------------
function TopDoctorsTab({ from, to }: { from: string; to: string }) {
  const q = useTopDoctorsReport({ from, to, top: 10 });
  if (q.isPending) return <SkeletonBlock rows={2} />;
  if (q.isError || !q.data) return <Alert variant="error">{humanizeError(q.error)}</Alert>;

  const d = q.data;
  const money = new Intl.NumberFormat(undefined, { style: 'currency', currency: d.currency, maximumFractionDigits: 0 });

  return (
    <Card padding="lg">
      <div className="mb-4">
        <h3 className="text-title text-gray-900 dark:text-white">Top doctors — by revenue</h3>
        <p className="mt-0.5 text-caption text-gray-500 dark:text-gray-400">
          Ranked by invoiced revenue linked to their appointments.
        </p>
      </div>
      {d.rows.length === 0 ? (
        <p className="text-body text-gray-500 dark:text-gray-400">No data for this range.</p>
      ) : (
        <ul className="space-y-3">
          {d.rows.map((row, idx) => (
            <li key={row.doctorId} className="flex items-center gap-3">
              <span className="inline-flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-brand-50 dark:bg-brand-500/10 text-brand-700 dark:text-brand-300 text-caption font-semibold">
                {idx + 1}
              </span>
              <Avatar name={row.doctorName} size="sm" />
              <div className="min-w-0 flex-1">
                <p className="text-body font-medium text-gray-900 dark:text-white truncate">{row.doctorName}</p>
                <p className="text-caption text-gray-500 dark:text-gray-400 truncate">{row.specialty ?? '—'}</p>
              </div>
              <div className="text-right">
                <p className="text-body text-gray-900 dark:text-gray-100 tabular-nums">
                  {row.completed} / {row.appointments}
                </p>
                <p className="text-caption text-gray-500 dark:text-gray-400 tabular-nums">{money.format(row.revenue)}</p>
              </div>
            </li>
          ))}
        </ul>
      )}
    </Card>
  );
}

// -------------------- Top services tab --------------------
function TopServicesTab({ from, to }: { from: string; to: string }) {
  const q = useTopServicesReport({ from, to, top: 10 });
  if (q.isPending) return <SkeletonBlock rows={2} />;
  if (q.isError || !q.data) return <Alert variant="error">{humanizeError(q.error)}</Alert>;

  const d = q.data;
  const money = new Intl.NumberFormat(undefined, { style: 'currency', currency: d.currency, maximumFractionDigits: 0 });

  return (
    <Card padding="lg">
      <div className="mb-4">
        <h3 className="text-title text-gray-900 dark:text-white">Top services — by revenue</h3>
        <p className="mt-0.5 text-caption text-gray-500 dark:text-gray-400">
          Grouped by line item description and service code.
        </p>
      </div>
      {d.rows.length === 0 ? (
        <p className="text-body text-gray-500 dark:text-gray-400">No data for this range.</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-body">
            <thead>
              <tr className="text-caption text-gray-500 dark:text-gray-400 text-left">
                <th className="py-2 pr-4 font-medium">Service</th>
                <th className="py-2 pr-4 font-medium">Code</th>
                <th className="py-2 pr-4 font-medium text-right">Qty</th>
                <th className="py-2 pr-4 font-medium text-right">Lines</th>
                <th className="py-2 font-medium text-right">Revenue</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
              {d.rows.map((r, idx) => (
                <tr key={`${r.description}-${r.serviceCode ?? idx}`}>
                  <td className="py-2 pr-4 text-gray-900 dark:text-white">{r.description}</td>
                  <td className="py-2 pr-4 text-gray-500 dark:text-gray-400">{r.serviceCode ?? '—'}</td>
                  <td className="py-2 pr-4 text-right tabular-nums">{r.quantity}</td>
                  <td className="py-2 pr-4 text-right tabular-nums">{r.lineCount}</td>
                  <td className="py-2 text-right tabular-nums">{money.format(r.revenue)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Card>
  );
}

// -------------------- Patient demographics tab --------------------
function PatientDemographicsTab() {
  const q = usePatientDemographics();
  if (q.isPending) return <SkeletonBlock rows={2} />;
  if (q.isError || !q.data) return <Alert variant="error">{humanizeError(q.error)}</Alert>;
  const d = q.data;

  return (
    <div className="space-y-6">
      <StatCard label="Total patients" value={d.totalPatients.toLocaleString()} icon={<UsersIcon />} tone="brand" />
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <BreakdownCard title="By gender" icon={<UsersIcon className="h-4 w-4" />}>
          {d.gender.map((g) => (
            <BreakdownRow key={g.gender} label={g.gender} value={g.count} total={d.totalPatients} />
          ))}
        </BreakdownCard>
        <BreakdownCard title="By age band" icon={<ChartBarIcon className="h-4 w-4" />}>
          {d.ageBands.map((a) => (
            <BreakdownRow key={a.band} label={a.band} value={a.count} total={d.totalPatients} />
          ))}
        </BreakdownCard>
        <BreakdownCard title="By blood group" icon={<UsersIcon className="h-4 w-4" />}>
          {d.bloodGroups.length === 0 ? (
            <p className="text-body text-gray-500 dark:text-gray-400">Not recorded yet.</p>
          ) : (
            d.bloodGroups.map((b) => (
              <BreakdownRow key={b.bloodGroup} label={b.bloodGroup} value={b.count} total={d.totalPatients} />
            ))
          )}
        </BreakdownCard>
      </div>
    </div>
  );
}

function BreakdownCard({ title, icon, children }: { title: string; icon: React.ReactNode; children: React.ReactNode }) {
  return (
    <Card padding="lg">
      <div className="flex items-center gap-2 mb-3">
        <span className="text-gray-400">{icon}</span>
        <h3 className="text-title text-gray-900 dark:text-white">{title}</h3>
      </div>
      <div className="space-y-2">{children}</div>
    </Card>
  );
}

function BreakdownRow({ label, value, total }: { label: string; value: number; total: number }) {
  const pct = total > 0 ? Math.round((value * 100) / total) : 0;
  return (
    <div>
      <div className="flex items-baseline justify-between">
        <p className="text-body text-gray-700 dark:text-gray-300">{label}</p>
        <p className="text-caption text-gray-500 dark:text-gray-400 tabular-nums">
          <Badge tone="brand">{value}</Badge> <span className="ml-1">{pct}%</span>
        </p>
      </div>
      <div className="mt-1 h-1.5 rounded-full bg-gray-100 dark:bg-gray-800 overflow-hidden">
        <div className="bg-brand-500 h-full rounded-full" style={{ width: `${pct}%` }} />
      </div>
    </div>
  );
}

// -------------------- Skeleton --------------------
function SkeletonBlock({ rows }: { rows: number }) {
  return (
    <div className="space-y-4">
      {Array.from({ length: rows }).map((_, i) => (
        <SkeletonCard key={i} lines={2} />
      ))}
    </div>
  );
}
