import type { ReactNode } from 'react';
import { MoonIcon, SunIcon, ShieldCheckIcon, LockClosedIcon, HeartIcon, BoltIcon } from '@heroicons/react/24/outline';

import { useTheme } from '@/contexts/ThemeContext';

interface AuthLayoutProps {
  title: string;
  subtitle?: string;
  children: ReactNode;
  footer?: ReactNode;
}

const highlights = [
  {
    icon: HeartIcon,
    title: 'Purpose-built for clinics',
    description: 'Patients, appointments, consultations, prescriptions, billing — one platform.',
  },
  {
    icon: ShieldCheckIcon,
    title: 'Tenant-isolated & audited',
    description: 'Every write is stamped and tenant-scoped by design.',
  },
  {
    icon: BoltIcon,
    title: 'Modern, fast, delightful',
    description: 'A UI your team will actually want to use — day after day.',
  },
];

export function AuthLayout({ title, subtitle, children, footer }: AuthLayoutProps) {
  const { theme, toggleTheme } = useTheme();

  return (
    <div className="min-h-screen grid lg:grid-cols-[1.15fr_1fr] bg-white dark:bg-gray-950">
      {/* Left — brand */}
      <aside className="hidden lg:flex relative flex-col justify-between overflow-hidden bg-brand-950 text-white p-12">
        {/* Decorative background */}
        <div aria-hidden="true" className="pointer-events-none absolute inset-0 opacity-70">
          <div className="absolute -top-24 -right-16 h-96 w-96 rounded-full bg-brand-500 blur-3xl opacity-40" />
          <div className="absolute -bottom-24 -left-24 h-96 w-96 rounded-full bg-teal-500 blur-3xl opacity-25" />
        </div>

        <div className="relative z-10">
          <div className="flex items-center gap-2.5">
            <span className="inline-flex h-9 w-9 items-center justify-center rounded-xl bg-white/10 ring-1 ring-inset ring-white/20 backdrop-blur">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} className="h-4 w-4">
                <path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" />
              </svg>
            </span>
            <div>
              <p className="text-caption uppercase tracking-widest text-brand-200">Arrapa Software</p>
              <p className="text-title-lg font-semibold">ClinicOS</p>
            </div>
          </div>
          <p className="mt-8 text-display text-white/95 leading-tight max-w-md">
            The complete operating system for modern clinics.
          </p>
          <p className="mt-3 text-body-lg text-brand-100/80 max-w-md">
            Sign in to manage patients, appointments, and billing — from one clean dashboard.
          </p>
        </div>

        <ul className="relative z-10 space-y-4 max-w-md">
          {highlights.map((h) => (
            <li key={h.title} className="flex gap-3">
              <span className="inline-flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-white/10 ring-1 ring-inset ring-white/15">
                <h.icon className="h-4 w-4 text-white" />
              </span>
              <div>
                <p className="text-body font-medium text-white">{h.title}</p>
                <p className="text-caption text-brand-100/80">{h.description}</p>
              </div>
            </li>
          ))}
        </ul>

        <div className="relative z-10 flex items-center justify-between text-caption text-brand-100/70">
          <span>© {new Date().getFullYear()} Arrapa Software</span>
          <span className="inline-flex items-center gap-1.5">
            <LockClosedIcon className="h-3.5 w-3.5" /> Bank-grade security
          </span>
        </div>
      </aside>

      {/* Right — form */}
      <main className="flex items-center justify-center px-4 py-10 sm:px-8 bg-white dark:bg-gray-950">
        <div className="w-full max-w-md">
          <div className="flex items-center justify-between mb-10">
            <div className="lg:hidden flex items-center gap-2">
              <span className="inline-flex h-8 w-8 items-center justify-center rounded-lg bg-brand-600 text-white">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} className="h-4 w-4">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" />
                </svg>
              </span>
              <div>
                <p className="text-caption uppercase tracking-widest text-brand-600 dark:text-brand-400 font-semibold">Arrapa</p>
                <p className="text-body font-semibold text-gray-900 dark:text-white">ClinicOS</p>
              </div>
            </div>
            <button
              type="button"
              onClick={toggleTheme}
              className="ml-auto inline-flex h-9 w-9 items-center justify-center rounded-lg text-gray-500 hover:bg-gray-100 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white transition-colors"
              aria-label="Toggle theme"
            >
              {theme === 'dark' ? <SunIcon className="h-4 w-4" /> : <MoonIcon className="h-4 w-4" />}
            </button>
          </div>

          <h1 className="text-display text-gray-900 dark:text-white">{title}</h1>
          {subtitle && (
            <p className="mt-2 text-body-lg text-gray-500 dark:text-gray-400">{subtitle}</p>
          )}

          <div className="mt-8">{children}</div>

          {footer && <div className="mt-8 text-body text-gray-500 dark:text-gray-400">{footer}</div>}
        </div>
      </main>
    </div>
  );
}
