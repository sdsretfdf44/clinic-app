import { Link } from 'react-router-dom';
import {
  BuildingOffice2Icon,
  CheckCircleIcon,
  NoSymbolIcon,
  UsersIcon,
  ArrowUpRightIcon,
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
} from '@/components/ui';
import { useAdminDashboard } from '@/features/admin/dashboard';

const statusTone: Record<string, 'green' | 'amber' | 'red' | 'gray'> = {
  Active: 'green',
  Trial: 'amber',
  Suspended: 'red',
};

const currencyFormatter = (currency: string) =>
  new Intl.NumberFormat(undefined, { style: 'currency', currency, maximumFractionDigits: 0 });

export default function AdminDashboardPage() {
  const dashboard = useAdminDashboard();

  if (dashboard.isPending) {
    return (
      <div className="space-y-6">
        <PageHeader eyebrow="Super admin" title="Platform overview" description="Loading platform KPIs…" />
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <SkeletonCard key={i} lines={2} />
          ))}
        </div>
      </div>
    );
  }

  if (dashboard.isError || !dashboard.data) {
    return (
      <div className="space-y-6">
        <PageHeader eyebrow="Super admin" title="Platform overview" />
        <Alert variant="error">{humanizeError(dashboard.error)}</Alert>
      </div>
    );
  }

  const data = dashboard.data;
  const formatCurrency = currencyFormatter(data.revenue.currency);
  const maxPlanRevenue = Math.max(1, ...data.planRevenue.map((p) => p.monthlyRevenue));

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Super admin"
        title="Platform overview"
        description="Live KPIs for tenants, users, and platform revenue."
      />

      {/* KPIs */}
      <section className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Total tenants"
          value={data.tenants.total.toLocaleString()}
          hint={`+${data.tenants.newThisMonth} this month`}
          icon={<BuildingOffice2Icon />}
          tone="brand"
        />
        <StatCard
          label="Active tenants"
          value={data.tenants.active.toLocaleString()}
          hint={`${data.tenants.trial} on trial`}
          icon={<CheckCircleIcon />}
          tone="success"
        />
        <StatCard
          label="Suspended"
          value={data.tenants.suspended.toLocaleString()}
          hint={data.tenants.suspended > 0 ? 'Requires attention' : 'All clear'}
          icon={<NoSymbolIcon />}
          tone={data.tenants.suspended > 0 ? 'danger' : 'gray'}
        />
        <StatCard
          label="Users"
          value={data.users.total.toLocaleString()}
          hint={`${data.users.active} active`}
          icon={<UsersIcon />}
          tone="teal"
        />
      </section>

      {/* Revenue */}
      <section className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <Card padding="lg" className="lg:col-span-1">
          <p className="u-label mb-2">Recurring revenue</p>
          <p className="text-display-lg text-gray-900 dark:text-white tabular-nums">
            {formatCurrency.format(data.revenue.monthlyRecurring)}
          </p>
          <p className="mt-1 text-caption text-gray-500 dark:text-gray-400">Monthly (MRR)</p>
          <div className="mt-6 pt-5 border-t border-gray-100 dark:border-gray-800">
            <p className="u-label mb-2">Annualized</p>
            <p className="text-title-lg text-gray-800 dark:text-gray-100 tabular-nums">
              {formatCurrency.format(data.revenue.annualRecurring)}
            </p>
          </div>
        </Card>

        <Card padding="lg" className="lg:col-span-2">
          <div className="flex items-center justify-between mb-5">
            <p className="u-label">Revenue by plan</p>
            <Link to="/admin/plans" className="text-caption font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400">
              Manage plans →
            </Link>
          </div>
          {data.planRevenue.length === 0 ? (
            <p className="text-body text-gray-500">No active paid tenants yet.</p>
          ) : (
            <ul className="space-y-4">
              {data.planRevenue.map((plan) => {
                const pct = (plan.monthlyRevenue / maxPlanRevenue) * 100;
                return (
                  <li key={plan.planId} className="space-y-2">
                    <div className="flex items-center justify-between text-body">
                      <span className="text-gray-800 dark:text-gray-100 font-medium truncate">{plan.planName}</span>
                      <span className="text-gray-500 dark:text-gray-400 tabular-nums">
                        {formatCurrency.format(plan.monthlyRevenue)}
                        <span className="ml-2 text-caption text-gray-400">· {plan.tenantCount} clinics</span>
                      </span>
                    </div>
                    <div className="h-2 rounded-full bg-gray-100 dark:bg-gray-800 overflow-hidden">
                      <div
                        className="h-full rounded-full bg-gradient-to-r from-brand-500 to-brand-600 transition-all duration-300"
                        style={{ width: `${Math.min(100, pct)}%` }}
                      />
                    </div>
                  </li>
                );
              })}
            </ul>
          )}
        </Card>
      </section>

      {/* Recent tenants + activity */}
      <section className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <Card padding="lg">
          <div className="flex items-center justify-between mb-5">
            <p className="u-label">Recent tenants</p>
            <Link to="/admin/tenants" className="text-caption font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400">
              View all →
            </Link>
          </div>
          {data.recentTenants.length === 0 ? (
            <p className="text-body text-gray-500">No tenants yet.</p>
          ) : (
            <ul className="divide-y divide-gray-100 dark:divide-gray-800 -mx-2">
              {data.recentTenants.map((tenant) => (
                <li key={tenant.id}>
                  <Link
                    to={`/admin/tenants/${tenant.id}`}
                    className="flex items-center gap-3 py-3 px-2 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
                  >
                    <Avatar name={tenant.name} size="sm" />
                    <div className="min-w-0 flex-1">
                      <p className="text-body font-medium text-gray-900 dark:text-white truncate">
                        {tenant.name}
                      </p>
                      <p className="text-caption text-gray-500 dark:text-gray-400 font-mono">
                        /{tenant.slug}
                      </p>
                    </div>
                    <Badge tone={statusTone[tenant.status] ?? 'gray'} variant="dot">
                      {tenant.status}
                    </Badge>
                    <ArrowUpRightIcon className="h-3.5 w-3.5 text-gray-300 group-hover:text-gray-500" />
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </Card>

        <Card padding="lg">
          <div className="flex items-center justify-between mb-5">
            <p className="u-label">Recent activity</p>
            <Link to="/admin/audit-logs" className="text-caption font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400">
              Audit log →
            </Link>
          </div>
          {data.recentActivity.length === 0 ? (
            <p className="text-body text-gray-500">No activity yet.</p>
          ) : (
            <ul className="space-y-3">
              {data.recentActivity.map((log) => (
                <li key={log.id} className="flex items-start gap-3">
                  <span className="mt-1.5 inline-block h-1.5 w-1.5 rounded-full bg-brand-500 shrink-0" />
                  <div className="min-w-0 flex-1">
                    <p className="text-body text-gray-800 dark:text-gray-100">
                      <span className="font-medium">{log.operation}</span>{' '}
                      <span className="text-gray-500 dark:text-gray-400">{log.entityType}</span>
                    </p>
                    <p className="text-caption text-gray-500 dark:text-gray-400">
                      {log.performedByEmail ?? 'system'} · {new Date(log.occurredOn).toLocaleString()}
                    </p>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </Card>
      </section>
    </div>
  );
}
