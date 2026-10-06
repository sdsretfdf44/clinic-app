import { useNavigate } from 'react-router-dom';
import {
  Bars3Icon,
  MoonIcon,
  SunIcon,
  BellIcon,
  MagnifyingGlassIcon,
  UserCircleIcon,
  KeyIcon,
  ArrowRightOnRectangleIcon,
} from '@heroicons/react/24/outline';

import { useTheme } from '@/contexts/ThemeContext';
import { useSignOut } from '@/features/auth/hooks';
import { useAuthStore } from '@/store/authStore';
import { Avatar, Dropdown, DropdownItem, DropdownSeparator, Tooltip } from '@/components/ui';

interface DashboardHeaderProps {
  onOpenSidebar?: () => void;
}

export function DashboardHeader({ onOpenSidebar }: DashboardHeaderProps) {
  const user = useAuthStore((s) => s.user);
  const { theme, toggleTheme } = useTheme();
  const signOut = useSignOut();
  const navigate = useNavigate();

  return (
    <header className="h-16 border-b border-gray-100 dark:border-gray-800 bg-white/85 dark:bg-gray-900/85 backdrop-blur px-4 sm:px-6 flex items-center gap-3 sticky top-0 z-20">
      <button
        type="button"
        onClick={onOpenSidebar}
        className="lg:hidden inline-flex h-9 w-9 items-center justify-center rounded-lg text-gray-600 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800 transition-colors"
        aria-label="Open navigation"
      >
        <Bars3Icon className="h-5 w-5" />
      </button>

      {/* Global search */}
      <div className="flex-1 max-w-lg">
        <label className="relative block">
          <span className="pointer-events-none absolute inset-y-0 left-3 flex items-center text-gray-400">
            <MagnifyingGlassIcon className="h-4 w-4" />
          </span>
          <input
            type="search"
            placeholder="Search patients, appointments, invoices…"
            className="w-full h-10 rounded-lg border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 pl-9 pr-16 text-body text-gray-900 dark:text-white placeholder:text-gray-400 focus:outline-none focus:border-brand-500 focus:bg-white dark:focus:bg-gray-900 focus:shadow-ring-brand transition"
          />
          <span className="pointer-events-none absolute inset-y-0 right-2 flex items-center gap-0.5">
            <kbd className="inline-flex items-center justify-center rounded border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 px-1.5 text-[10px] font-medium text-gray-500">⌘</kbd>
            <kbd className="inline-flex items-center justify-center rounded border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 px-1.5 text-[10px] font-medium text-gray-500">K</kbd>
          </span>
        </label>
      </div>

      <div className="flex items-center gap-1.5">
        <Tooltip label={theme === 'dark' ? 'Light mode' : 'Dark mode'} side="bottom">
          <button
            type="button"
            onClick={toggleTheme}
            className="inline-flex h-9 w-9 items-center justify-center rounded-lg text-gray-500 hover:bg-gray-100 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white transition-colors"
            aria-label="Toggle theme"
          >
            {theme === 'dark' ? <SunIcon className="h-4.5 w-4.5" /> : <MoonIcon className="h-4.5 w-4.5" />}
          </button>
        </Tooltip>

        <Tooltip label="Notifications" side="bottom">
          <button
            type="button"
            className="relative inline-flex h-9 w-9 items-center justify-center rounded-lg text-gray-500 hover:bg-gray-100 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white transition-colors"
            aria-label="Notifications"
          >
            <BellIcon className="h-4.5 w-4.5" />
          </button>
        </Tooltip>

        <div className="mx-1 hidden sm:block h-6 w-px bg-gray-100 dark:bg-gray-800" />

        <Dropdown
          align="right"
          trigger={({ toggle }) => (
            <button
              type="button"
              onClick={toggle}
              className="flex items-center gap-2 rounded-full pl-1 pr-2 py-1 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
            >
              <Avatar name={user?.fullName} src={user?.avatarUrl} size="sm" />
              <span className="hidden sm:flex flex-col text-left leading-tight pr-0.5">
                <span className="text-caption font-medium text-gray-900 dark:text-white">
                  {user?.firstName ?? 'Account'}
                </span>
                <span className="text-[10px] text-gray-500 dark:text-gray-400 truncate max-w-[9rem]">
                  {user?.primaryRole ?? '—'}
                </span>
              </span>
              <svg viewBox="0 0 20 20" fill="currentColor" className="hidden sm:block h-3.5 w-3.5 text-gray-400">
                <path fillRule="evenodd" d="M5.516 7.548L10 12.032l4.484-4.484L15.5 8.564l-5.5 5.5-5.5-5.5z" clipRule="evenodd" />
              </svg>
            </button>
          )}
          menuClassName="w-64"
        >
          <div className="px-3 py-2.5 border-b border-gray-100 dark:border-gray-800 mb-1">
            <p className="text-body font-medium text-gray-900 dark:text-white truncate">{user?.fullName}</p>
            <p className="text-caption text-gray-500 dark:text-gray-400 truncate">{user?.email}</p>
            {user?.tenantName && (
              <p className="mt-1 text-caption text-gray-400 truncate">{user.tenantName}</p>
            )}
          </div>
          <DropdownItem icon={<UserCircleIcon />} onSelect={() => navigate('/profile')}>
            Profile
          </DropdownItem>
          <DropdownItem icon={<KeyIcon />} onSelect={() => navigate('/change-password')}>
            Change password
          </DropdownItem>
          <DropdownSeparator />
          <DropdownItem
            icon={<ArrowRightOnRectangleIcon />}
            onSelect={() => signOut.mutate()}
            danger
          >
            Sign out
          </DropdownItem>
        </Dropdown>
      </div>
    </header>
  );
}
