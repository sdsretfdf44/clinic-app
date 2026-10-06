import { Navigate } from 'react-router-dom';
import {
  BuildingOffice2Icon,
  UserGroupIcon,
  ClockIcon,
  EnvelopeIcon,
} from '@heroicons/react/24/outline';

import { PageHeader, SkeletonCard, StatCard } from '@/components/ui';
import { useCurrentUser } from '@/features/auth/hooks';

export default function DashboardPage() {
  const currentUser = useCurrentUser();

  if (currentUser.isPending) {
    return (
      <div className="space-y-6">
        <PageHeader eyebrow="Dashboard" title="Welcome" description="Loading your workspace…" />
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <SkeletonCard key={i} lines={1} />
          ))}
        </div>
      </div>
    );
  }

  const user = currentUser.data;

  // Land each role on their own workspace overview.
  if (user) {
    const roles = user.roles;
    const isOnly = (role: string) => roles.includes(role) && !roles.some((r) => r !== role);

    if (isOnly('Patient')) return <Navigate to="/portal" replace />;
    if (roles.includes('SuperAdmin')) return <Navigate to="/admin" replace />;
    if (roles.includes('ClinicAdmin')) return <Navigate to="/clinic" replace />;
    if (isOnly('Doctor')) return <Navigate to="/doctor" replace />;
    if (isOnly('Receptionist')) return <Navigate to="/reception" replace />;
  }

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Dashboard"
        title={`Welcome${user ? `, ${user.firstName}` : ''}.`}
        description="Your workspace at a glance."
      />

      <section className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Tenant"
          value={user?.tenantName ?? '—'}
          hint={user?.tenantSlug ? `/${user.tenantSlug}` : undefined}
          icon={<BuildingOffice2Icon />}
          tone="brand"
        />
        <StatCard
          label="Primary role"
          value={user?.primaryRole ?? '—'}
          hint={user?.roles.join(', ')}
          icon={<UserGroupIcon />}
          tone="teal"
        />
        <StatCard
          label="Last sign-in"
          value={user?.lastLoginAt ? new Date(user.lastLoginAt).toLocaleDateString() : 'First sign-in'}
          hint={user?.lastLoginAt ? new Date(user.lastLoginAt).toLocaleTimeString() : undefined}
          icon={<ClockIcon />}
          tone="gray"
        />
        <StatCard
          label="Email"
          value={user?.email ?? '—'}
          icon={<EnvelopeIcon />}
          tone="gray"
        />
      </section>
    </div>
  );
}
