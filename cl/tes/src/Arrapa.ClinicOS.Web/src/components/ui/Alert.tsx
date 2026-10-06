import type { ReactNode } from 'react';

import { cn } from '@/utils/cn';

export type AlertVariant = 'info' | 'success' | 'warning' | 'error';

interface AlertProps {
  variant?: AlertVariant;
  title?: string;
  children: ReactNode;
  className?: string;
  action?: ReactNode;
}

const variantClasses: Record<AlertVariant, string> = {
  info:    'bg-brand-50 text-brand-900 border-brand-200/80 dark:bg-brand-500/10 dark:text-brand-200 dark:border-brand-500/30',
  success: 'bg-success-50 text-success-700 border-success-100 dark:bg-success-500/10 dark:text-success-500 dark:border-success-500/30',
  warning: 'bg-warning-50 text-warning-700 border-warning-100 dark:bg-warning-500/10 dark:text-warning-500 dark:border-warning-500/30',
  error:   'bg-danger-50 text-danger-700 border-danger-100 dark:bg-danger-500/10 dark:text-danger-500 dark:border-danger-500/30',
};

const iconPaths: Record<AlertVariant, string> = {
  info:    'M10 18a8 8 0 100-16 8 8 0 000 16zM10 6a1 1 0 100 2 1 1 0 000-2zm.75 3.5a.75.75 0 00-1.5 0v4.5a.75.75 0 001.5 0V9.5z',
  success: 'M10 18a8 8 0 100-16 8 8 0 000 16zm3.28-9.53a.75.75 0 10-1.06-1.06L9 10.63 7.78 9.41a.75.75 0 10-1.06 1.06l1.75 1.75a.75.75 0 001.06 0l4-4z',
  warning: 'M9.401 2.6a.75.75 0 011.198 0l7.5 12.999A.75.75 0 0117.5 16.75H2.5a.75.75 0 01-.649-1.151l7.55-12.999zm.599 5.65a.75.75 0 00-.75.75v3a.75.75 0 001.5 0v-3a.75.75 0 00-.75-.75zm0 6.5a1 1 0 100-2 1 1 0 000 2z',
  error:   'M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z',
};

export function Alert({ variant = 'info', title, children, className, action }: AlertProps) {
  return (
    <div
      role={variant === 'error' ? 'alert' : 'status'}
      className={cn('rounded-xl border px-4 py-3 flex gap-3 items-start', variantClasses[variant], className)}
    >
      <svg viewBox="0 0 20 20" fill="currentColor" className="mt-0.5 h-5 w-5 shrink-0" aria-hidden="true">
        <path fillRule="evenodd" clipRule="evenodd" d={iconPaths[variant]} />
      </svg>
      <div className="flex-1 min-w-0">
        {title && <p className="font-medium mb-0.5">{title}</p>}
        <div className="text-body leading-relaxed">{children}</div>
      </div>
      {action && <div className="shrink-0">{action}</div>}
    </div>
  );
}
