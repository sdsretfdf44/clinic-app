import { useEffect, useMemo, useState, type ComponentType, type SVGProps } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import {
  Squares2X2Icon,
  UserCircleIcon,
  KeyIcon,
  BuildingOffice2Icon,
  BuildingStorefrontIcon,
  UserGroupIcon,
  IdentificationIcon,
  ClockIcon,
  AdjustmentsHorizontalIcon,
  UsersIcon,
  UserPlusIcon,
  CalendarDaysIcon,
  PlusCircleIcon,
  HeartIcon,
  DocumentTextIcon,
  MagnifyingGlassIcon,
  CurrencyDollarIcon,
  ShieldCheckIcon,
  FlagIcon,
  Cog6ToothIcon,
  ClipboardDocumentListIcon,
  ChartBarIcon,
  ChatBubbleLeftRightIcon,
  RectangleGroupIcon,
  BeakerIcon,
  UserIcon,
  ChevronDownIcon,
} from '@heroicons/react/24/outline';

import { useAuthStore } from '@/store/authStore';
import { cn } from '@/utils/cn';

type IconType = ComponentType<SVGProps<SVGSVGElement>>;

interface NavItem {
  label: string;
  to: string;
  icon: IconType;
  exact?: boolean;
  badge?: string;
}

interface NavGroup {
  label: string;
  icon: IconType;
  items: NavItem[];
}

const personalGroup: NavGroup = {
  label: 'Workspace',
  icon: Squares2X2Icon,
  items: [
    { label: 'Dashboard', to: '/dashboard', icon: Squares2X2Icon, exact: true },
    { label: 'Profile', to: '/profile', icon: UserCircleIcon },
    { label: 'Change password', to: '/change-password', icon: KeyIcon },
  ],
};

const portalGroup: NavGroup = {
  label: 'Patient portal',
  icon: RectangleGroupIcon,
  items: [
    { label: 'Overview', to: '/portal', icon: RectangleGroupIcon, exact: true },
    { label: 'My appointments', to: '/portal/appointments', icon: CalendarDaysIcon, exact: true },
    { label: 'Request appointment', to: '/portal/appointments/new', icon: PlusCircleIcon },
    { label: 'Prescriptions', to: '/portal/prescriptions', icon: BeakerIcon, exact: true },
    { label: 'Invoices', to: '/portal/invoices', icon: CurrencyDollarIcon, exact: true },
    { label: 'My profile', to: '/portal/profile', icon: UserIcon },
  ],
};

const doctorGroup: NavGroup = {
  label: 'Doctor',
  icon: HeartIcon,
  items: [
    { label: 'Overview', to: '/doctor', icon: RectangleGroupIcon, exact: true },
    { label: "Today's queue", to: '/doctor/queue', icon: HeartIcon },
  ],
};

const receptionGroup: NavGroup = {
  label: 'Reception',
  icon: UserPlusIcon,
  items: [
    { label: "Today's queue", to: '/reception', icon: HeartIcon, exact: true },
    { label: 'Patient search', to: '/reception/search', icon: MagnifyingGlassIcon },
    { label: 'Walk-in', to: '/reception/walk-in', icon: UserPlusIcon },
  ],
};

const patientsGroup: NavGroup = {
  label: 'Patients',
  icon: UsersIcon,
  items: [
    { label: 'All patients', to: '/patients', icon: UsersIcon, exact: true },
    { label: 'Register patient', to: '/patients/new', icon: UserPlusIcon },
  ],
};

const appointmentsGroup: NavGroup = {
  label: 'Appointments',
  icon: CalendarDaysIcon,
  items: [
    { label: 'Schedule', to: '/appointments', icon: CalendarDaysIcon, exact: true },
    { label: 'Book appointment', to: '/appointments/new', icon: PlusCircleIcon },
  ],
};

const billingGroup: NavGroup = {
  label: 'Billing',
  icon: DocumentTextIcon,
  items: [
    { label: 'Invoices', to: '/billing', icon: DocumentTextIcon, exact: true },
    { label: 'New invoice', to: '/billing/new', icon: PlusCircleIcon },
  ],
};

const clinicGroup: NavGroup = {
  label: 'Clinic admin',
  icon: BuildingStorefrontIcon,
  items: [
    { label: 'Overview', to: '/clinic', icon: RectangleGroupIcon, exact: true },
    { label: 'Clinic profile', to: '/clinic/profile', icon: BuildingStorefrontIcon },
    { label: 'Departments', to: '/clinic/departments', icon: BuildingOffice2Icon },
    { label: 'Doctors', to: '/clinic/doctors', icon: UserGroupIcon },
    { label: 'Receptionists', to: '/clinic/receptionists', icon: IdentificationIcon },
    { label: 'Users', to: '/clinic/users', icon: UsersIcon },
    { label: 'Working hours', to: '/clinic/working-hours', icon: ClockIcon },
    { label: 'Reports', to: '/reports', icon: ChartBarIcon },
    { label: 'Notifications', to: '/notifications', icon: ChatBubbleLeftRightIcon },
    { label: 'Clinic settings', to: '/clinic/settings', icon: AdjustmentsHorizontalIcon },
  ],
};

const adminGroup: NavGroup = {
  label: 'Super admin',
  icon: ShieldCheckIcon,
  items: [
    { label: 'Overview', to: '/admin', icon: RectangleGroupIcon, exact: true },
    { label: 'Tenants', to: '/admin/tenants', icon: BuildingOffice2Icon },
    { label: 'Plans', to: '/admin/plans', icon: CurrencyDollarIcon },
    { label: 'Users', to: '/admin/users', icon: UsersIcon },
    { label: 'Feature flags', to: '/admin/feature-flags', icon: FlagIcon },
    { label: 'Global settings', to: '/admin/settings', icon: Cog6ToothIcon },
    { label: 'Audit logs', to: '/admin/audit-logs', icon: ClipboardDocumentListIcon },
    { label: 'Compliance', to: '/admin', icon: ShieldCheckIcon },
  ],
};

interface DashboardSidebarProps {
  mobileOpen?: boolean;
  onMobileClose?: () => void;
}

function isItemActive(pathname: string, item: NavItem): boolean {
  if (item.exact) return pathname === item.to;
  return pathname === item.to || pathname.startsWith(item.to + '/');
}

export function DashboardSidebar({ mobileOpen = false, onMobileClose }: DashboardSidebarProps) {
  const roles = useAuthStore((s) => s.user?.roles ?? []);
  const tenantName = useAuthStore((s) => s.user?.tenantName ?? s.user?.tenantSlug ?? null);
  const isSuperAdmin = roles.includes('SuperAdmin');
  const isClinicAdmin = roles.includes('ClinicAdmin') || isSuperAdmin;
  const canSeeClinicalOps =
    isSuperAdmin || isClinicAdmin || roles.includes('Doctor') || roles.includes('Receptionist');

  const isDoctor = roles.includes('Doctor') || isSuperAdmin;
  const isReceptionist = roles.includes('Receptionist') || isClinicAdmin;
  const isPatient = roles.includes('Patient');

  const groups = useMemo<NavGroup[]>(() => {
    const g: NavGroup[] = [personalGroup];
    if (isPatient) g.push(portalGroup);
    if (isDoctor) g.push(doctorGroup);
    if (isReceptionist) g.push(receptionGroup);
    if (canSeeClinicalOps) {
      g.push(appointmentsGroup);
      g.push(patientsGroup);
    }
    if (isReceptionist) g.push(billingGroup);
    if (isClinicAdmin) g.push(clinicGroup);
    if (isSuperAdmin) g.push(adminGroup);
    return g;
  }, [isPatient, isDoctor, isReceptionist, canSeeClinicalOps, isClinicAdmin, isSuperAdmin]);

  const { pathname } = useLocation();

  const activeGroupLabel = useMemo(() => {
    for (const group of groups) {
      if (group.items.some((item) => isItemActive(pathname, item))) return group.label;
    }
    return groups[0]?.label ?? null;
  }, [groups, pathname]);

  const [openGroup, setOpenGroup] = useState<string | null>(activeGroupLabel);

  useEffect(() => {
    if (activeGroupLabel) setOpenGroup(activeGroupLabel);
  }, [activeGroupLabel]);

  const nav = (
    <>
      <div className="h-16 shrink-0 px-6 flex items-center gap-2 border-b border-gray-100 dark:border-gray-800">
        <span className="inline-flex h-8 w-8 items-center justify-center rounded-lg bg-brand-600 text-white shadow-theme-xs">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} className="h-4 w-4">
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" />
          </svg>
        </span>
        <div className="flex-1 min-w-0">
          <p className="text-caption uppercase tracking-widest text-brand-600 dark:text-brand-400 font-semibold">
            Arrapa
          </p>
          <p className="text-body font-semibold text-gray-900 dark:text-white">ClinicOS</p>
        </div>
      </div>

      <nav className="flex-1 min-h-0 overflow-y-auto u-scrollbar px-3 py-4 space-y-1">
        {groups.map((group) => {
          const isOpen = openGroup === group.label;
          const hasActive = group.items.some((item) => isItemActive(pathname, item));
          const GroupIcon = group.icon;
          return (
            <div key={group.label}>
              <button
                type="button"
                onClick={() => setOpenGroup(isOpen ? null : group.label)}
                aria-expanded={isOpen}
                className={cn(
                  'group w-full flex items-center gap-3 px-3 h-9 rounded-lg text-body transition-colors duration-150',
                  hasActive
                    ? 'text-gray-900 dark:text-white font-medium'
                    : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-300 dark:hover:bg-gray-800/60 dark:hover:text-white',
                )}
              >
                <GroupIcon
                  className={cn(
                    'h-4 w-4 shrink-0 transition-colors',
                    hasActive
                      ? 'text-brand-600 dark:text-brand-400'
                      : 'text-gray-400 group-hover:text-gray-600 dark:text-gray-500 dark:group-hover:text-gray-300',
                  )}
                />
                <span className="flex-1 truncate text-left">{group.label}</span>
                <ChevronDownIcon
                  className={cn(
                    'h-4 w-4 shrink-0 text-gray-400 transition-transform duration-200',
                    isOpen ? 'rotate-180' : 'rotate-0',
                  )}
                />
              </button>

              {isOpen && (
                <div className="mt-1 ml-4 pl-3 border-l border-gray-100 dark:border-gray-800 space-y-0.5">
                  {group.items.map((item) => (
                    <NavLink
                      key={`${group.label}-${item.to}-${item.label}`}
                      to={item.to}
                      end={item.exact}
                      onClick={onMobileClose}
                      className={({ isActive }) =>
                        cn(
                          'group flex items-center gap-3 px-3 h-9 rounded-lg text-body transition-colors duration-150',
                          isActive
                            ? 'bg-brand-50 text-brand-700 font-medium dark:bg-brand-500/10 dark:text-brand-300'
                            : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-gray-300 dark:hover:bg-gray-800/60 dark:hover:text-white',
                        )
                      }
                    >
                      {({ isActive }) => (
                        <>
                          <item.icon
                            className={cn(
                              'h-4 w-4 shrink-0 transition-colors',
                              isActive
                                ? 'text-brand-600 dark:text-brand-400'
                                : 'text-gray-400 group-hover:text-gray-600 dark:text-gray-500 dark:group-hover:text-gray-300',
                            )}
                          />
                          <span className="flex-1 truncate">{item.label}</span>
                          {item.badge && (
                            <span className="ml-auto inline-flex items-center rounded-full bg-brand-50 text-brand-700 dark:bg-brand-500/10 dark:text-brand-300 text-[10px] px-1.5 py-0.5 font-medium">
                              {item.badge}
                            </span>
                          )}
                        </>
                      )}
                    </NavLink>
                  ))}
                </div>
              )}
            </div>
          );
        })}
      </nav>

      <div className="shrink-0 px-4 py-3 border-t border-gray-100 dark:border-gray-800">
        <div className="flex items-center gap-2 rounded-lg bg-gray-50 dark:bg-gray-800/60 px-3 py-2 min-w-0">
          <span className="inline-flex h-2 w-2 rounded-full bg-success-500 shadow-[0_0_0_3px_rgba(18,183,106,0.18)] shrink-0" />
          <div className="min-w-0 flex-1 leading-tight">
            <p className="text-[10px] uppercase tracking-widest text-gray-500 dark:text-gray-400 font-semibold">
              Workspace
            </p>
            <p className="text-caption text-gray-800 dark:text-gray-100 font-medium truncate">
              {tenantName ?? '—'}
            </p>
          </div>
        </div>
      </div>
    </>
  );

  return (
    <>
      {/* Desktop */}
      <aside className="hidden lg:flex flex-col w-64 shrink-0 sticky top-0 h-screen border-r border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900">
        {nav}
      </aside>

      {/* Mobile off-canvas */}
      {mobileOpen && (
        <div className="lg:hidden fixed inset-0 z-40 animate-fade-in">
          <div
            className="absolute inset-0 bg-gray-900/50 backdrop-blur-sm"
            onClick={onMobileClose}
            aria-hidden="true"
          />
          <aside className="absolute inset-y-0 left-0 w-72 max-w-[85vw] bg-white dark:bg-gray-900 shadow-theme-xl flex flex-col animate-slide-in-right">
            {nav}
          </aside>
        </div>
      )}
    </>
  );
}
