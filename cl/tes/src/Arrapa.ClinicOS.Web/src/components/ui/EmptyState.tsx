import type { ReactNode } from 'react';

import { cn } from '@/utils/cn';

interface EmptyStateProps {
  title: string;
  description?: string;
  action?: ReactNode;
  secondaryAction?: ReactNode;
  icon?: ReactNode;
  /** Colored halo tone behind the icon. */
  tone?: 'brand' | 'teal' | 'success' | 'warning' | 'danger' | 'gray';
  className?: string;
}

const toneHalo: Record<NonNullable<EmptyStateProps['tone']>, { chip: string; glow: string }> = {
  brand:   { chip: 'bg-brand-50 text-brand-600 dark:bg-brand-500/10 dark:text-brand-300 ring-1 ring-inset ring-brand-100 dark:ring-brand-500/20', glow: 'from-brand-500/25' },
  teal:    { chip: 'bg-teal-50 text-teal-600 dark:bg-teal-500/10 dark:text-teal-300 ring-1 ring-inset ring-teal-100 dark:ring-teal-500/20', glow: 'from-teal-500/25' },
  success: { chip: 'bg-success-50 text-success-600 dark:bg-success-500/10 dark:text-success-500 ring-1 ring-inset ring-success-100 dark:ring-success-500/20', glow: 'from-success-500/25' },
  warning: { chip: 'bg-warning-50 text-warning-600 dark:bg-warning-500/10 dark:text-warning-500 ring-1 ring-inset ring-warning-100 dark:ring-warning-500/20', glow: 'from-warning-500/25' },
  danger:  { chip: 'bg-danger-50 text-danger-600 dark:bg-danger-500/10 dark:text-danger-500 ring-1 ring-inset ring-danger-100 dark:ring-danger-500/20', glow: 'from-danger-500/25' },
  gray:    { chip: 'bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-300 ring-1 ring-inset ring-gray-200 dark:ring-gray-700', glow: 'from-gray-400/20' },
};

export function EmptyState({
  title,
  description,
  action,
  secondaryAction,
  icon,
  tone = 'brand',
  className,
}: EmptyStateProps) {
  const t = toneHalo[tone];
  return (
    <div
      className={cn(
        'relative overflow-hidden flex flex-col items-center justify-center rounded-2xl border border-dashed border-gray-200 dark:border-gray-800 bg-white/60 dark:bg-gray-900/40 px-6 py-16 text-center',
        className,
      )}
    >
      <div
        aria-hidden="true"
        className={cn(
          'pointer-events-none absolute left-1/2 -translate-x-1/2 -top-20 h-56 w-56 rounded-full blur-3xl opacity-70 bg-gradient-to-b to-transparent',
          t.glow,
        )}
      />

      <div className="relative">
        <span
          className={cn(
            'inline-flex h-14 w-14 items-center justify-center rounded-2xl [&>svg]:h-6 [&>svg]:w-6',
            t.chip,
          )}
        >
          {icon ?? (
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.5} className="h-6 w-6" aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d="M9.75 3.104v5.714a2.25 2.25 0 01-.659 1.591L5 14.5M9.75 3.104c-.251.023-.501.05-.75.082m.75-.082a24.301 24.301 0 014.5 0m0 0v5.714c0 .597.237 1.17.659 1.591L19.8 15.3M14.25 3.104c.251.023.501.05.75.082M19.8 15.3l-1.57.393A9.065 9.065 0 0112 15a9.065 9.065 0 00-6.23-.693L5 14.5m14.8.8l1.402 1.402c1.232 1.232.65 3.318-1.067 3.611A48.309 48.309 0 0112 21c-2.773 0-5.491-.235-8.135-.687-1.718-.293-2.3-2.379-1.067-3.61L5 14.5" />
            </svg>
          )}
        </span>
      </div>
      <p className="relative mt-5 text-title-lg font-semibold text-gray-900 dark:text-white tracking-tight">
        {title}
      </p>
      {description && (
        <p className="relative mt-2 text-body text-gray-500 dark:text-gray-400 max-w-md">{description}</p>
      )}
      {(action || secondaryAction) && (
        <div className="relative mt-6 flex flex-wrap items-center justify-center gap-2">
          {action}
          {secondaryAction}
        </div>
      )}
    </div>
  );
}
