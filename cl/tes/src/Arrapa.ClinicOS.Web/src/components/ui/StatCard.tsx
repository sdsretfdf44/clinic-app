import type { ReactNode } from 'react';

import { cn } from '@/utils/cn';

export type StatTrend = 'up' | 'down' | 'flat';

interface StatCardProps {
  label: string;
  value: ReactNode;
  hint?: ReactNode;
  icon?: ReactNode;
  /** Colored icon container tone */
  tone?: 'brand' | 'teal' | 'success' | 'warning' | 'danger' | 'gray';
  trend?: {
    direction: StatTrend;
    label: string;
  };
  className?: string;
}

const toneChip: Record<NonNullable<StatCardProps['tone']>, string> = {
  brand:   'bg-brand-50 text-brand-600 dark:bg-brand-500/10 dark:text-brand-300 ring-1 ring-inset ring-brand-100 dark:ring-brand-500/20',
  teal:    'bg-teal-50 text-teal-600 dark:bg-teal-500/10 dark:text-teal-300 ring-1 ring-inset ring-teal-100 dark:ring-teal-500/20',
  success: 'bg-success-50 text-success-600 dark:bg-success-500/10 dark:text-success-500 ring-1 ring-inset ring-success-100 dark:ring-success-500/20',
  warning: 'bg-warning-50 text-warning-600 dark:bg-warning-500/10 dark:text-warning-500 ring-1 ring-inset ring-warning-100 dark:ring-warning-500/20',
  danger:  'bg-danger-50 text-danger-600 dark:bg-danger-500/10 dark:text-danger-500 ring-1 ring-inset ring-danger-100 dark:ring-danger-500/20',
  gray:    'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-300 ring-1 ring-inset ring-gray-200 dark:ring-gray-700',
};

const toneAccent: Record<NonNullable<StatCardProps['tone']>, string> = {
  brand:   'from-brand-500/40 via-brand-500/10',
  teal:    'from-teal-500/40 via-teal-500/10',
  success: 'from-success-500/40 via-success-500/10',
  warning: 'from-warning-500/40 via-warning-500/10',
  danger:  'from-danger-500/40 via-danger-500/10',
  gray:    'from-gray-400/30 via-gray-400/5',
};

const trendClasses: Record<StatTrend, { badge: string; icon: ReactNode }> = {
  up: {
    badge: 'text-success-700 bg-success-50 dark:bg-success-500/10 dark:text-success-500',
    icon: (
      <svg viewBox="0 0 20 20" fill="currentColor" className="h-3 w-3">
        <path fillRule="evenodd" d="M10 3a.75.75 0 01.75.75v9.19l3.72-3.72a.75.75 0 111.06 1.06l-5 5a.75.75 0 01-1.06 0l-5-5a.75.75 0 111.06-1.06l3.72 3.72V3.75A.75.75 0 0110 3z" transform="rotate(180 10 10)" clipRule="evenodd" />
      </svg>
    ),
  },
  down: {
    badge: 'text-danger-700 bg-danger-50 dark:bg-danger-500/10 dark:text-danger-500',
    icon: (
      <svg viewBox="0 0 20 20" fill="currentColor" className="h-3 w-3">
        <path fillRule="evenodd" d="M10 3a.75.75 0 01.75.75v9.19l3.72-3.72a.75.75 0 111.06 1.06l-5 5a.75.75 0 01-1.06 0l-5-5a.75.75 0 111.06-1.06l3.72 3.72V3.75A.75.75 0 0110 3z" clipRule="evenodd" />
      </svg>
    ),
  },
  flat: {
    badge: 'text-gray-700 bg-gray-100 dark:bg-gray-800 dark:text-gray-300',
    icon: (
      <svg viewBox="0 0 20 20" fill="currentColor" className="h-3 w-3">
        <path d="M4 10a.75.75 0 01.75-.75h10.5a.75.75 0 010 1.5H4.75A.75.75 0 014 10z" />
      </svg>
    ),
  },
};

export function StatCard({ label, value, hint, icon, tone = 'brand', trend, className }: StatCardProps) {
  return (
    <div
      className={cn(
        'group relative overflow-hidden rounded-xl border border-gray-200/70 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 shadow-theme-xs',
        'transition-[transform,box-shadow] duration-200 ease-out-quart hover:-translate-y-0.5 hover:shadow-theme-md',
        className,
      )}
    >
      {/* Subtle top-right glow tinted to tone */}
      <div
        aria-hidden="true"
        className={cn(
          'pointer-events-none absolute -top-16 -right-16 h-40 w-40 rounded-full blur-3xl opacity-70 bg-gradient-to-br to-transparent',
          toneAccent[tone],
        )}
      />

      <div className="relative flex items-start justify-between gap-3">
        {icon && (
          <div className={cn('flex h-10 w-10 items-center justify-center rounded-lg [&>svg]:h-5 [&>svg]:w-5', toneChip[tone])}>
            {icon}
          </div>
        )}
        {trend && (
          <span
            className={cn(
              'inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-caption font-medium',
              trendClasses[trend.direction].badge,
            )}
          >
            {trendClasses[trend.direction].icon}
            {trend.label}
          </span>
        )}
      </div>
      <p className="relative mt-4 u-label">{label}</p>
      <p className="relative mt-1 text-display text-gray-900 dark:text-white tabular-nums tracking-tight">
        {value}
      </p>
      {hint && <p className="relative mt-1 text-caption text-gray-500 dark:text-gray-400">{hint}</p>}
    </div>
  );
}
