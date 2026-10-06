import type { ReactNode } from 'react';

import { cn } from '@/utils/cn';

export type BadgeTone = 'gray' | 'brand' | 'green' | 'amber' | 'red' | 'sky' | 'teal';
export type BadgeVariant = 'soft' | 'outline' | 'solid' | 'dot';

interface BadgeProps {
  tone?: BadgeTone;
  variant?: BadgeVariant;
  className?: string;
  children: ReactNode;
}

const softClasses: Record<BadgeTone, string> = {
  gray:  'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-200 ring-1 ring-inset ring-gray-200/70 dark:ring-gray-700',
  brand: 'bg-brand-50 text-brand-700 dark:bg-brand-500/10 dark:text-brand-300 ring-1 ring-inset ring-brand-200/70 dark:ring-brand-500/30',
  green: 'bg-success-50 text-success-700 dark:bg-success-500/10 dark:text-success-500 ring-1 ring-inset ring-success-500/20',
  amber: 'bg-warning-50 text-warning-700 dark:bg-warning-500/10 dark:text-warning-500 ring-1 ring-inset ring-warning-500/20',
  red:   'bg-danger-50 text-danger-700 dark:bg-danger-500/10 dark:text-danger-500 ring-1 ring-inset ring-danger-500/20',
  sky:   'bg-sky-50 text-sky-700 dark:bg-sky-500/10 dark:text-sky-300 ring-1 ring-inset ring-sky-500/20',
  teal:  'bg-teal-50 text-teal-700 dark:bg-teal-500/10 dark:text-teal-300 ring-1 ring-inset ring-teal-500/20',
};

const solidClasses: Record<BadgeTone, string> = {
  gray:  'bg-gray-700 text-white',
  brand: 'bg-brand-600 text-white',
  green: 'bg-success-600 text-white',
  amber: 'bg-warning-600 text-white',
  red:   'bg-danger-600 text-white',
  sky:   'bg-sky-600 text-white',
  teal:  'bg-teal-600 text-white',
};

const outlineClasses: Record<BadgeTone, string> = {
  gray:  'text-gray-700 dark:text-gray-200 ring-1 ring-inset ring-gray-300 dark:ring-gray-700',
  brand: 'text-brand-700 dark:text-brand-300 ring-1 ring-inset ring-brand-300 dark:ring-brand-500/40',
  green: 'text-success-700 ring-1 ring-inset ring-success-500/40',
  amber: 'text-warning-700 ring-1 ring-inset ring-warning-500/40',
  red:   'text-danger-700 ring-1 ring-inset ring-danger-500/40',
  sky:   'text-sky-700 dark:text-sky-300 ring-1 ring-inset ring-sky-500/40',
  teal:  'text-teal-700 dark:text-teal-300 ring-1 ring-inset ring-teal-500/40',
};

const dotColor: Record<BadgeTone, string> = {
  gray:  'bg-gray-400',
  brand: 'bg-brand-500',
  green: 'bg-success-500',
  amber: 'bg-warning-500',
  red:   'bg-danger-500',
  sky:   'bg-sky-500',
  teal:  'bg-teal-500',
};

export function Badge({ tone = 'gray', variant = 'soft', className, children }: BadgeProps) {
  const base =
    variant === 'solid'   ? solidClasses[tone]   :
    variant === 'outline' ? outlineClasses[tone] :
    variant === 'dot'     ? softClasses[tone]    :
                            softClasses[tone];
  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 text-caption font-medium whitespace-nowrap',
        base,
        className,
      )}
    >
      {variant === 'dot' && <span className={cn('h-1.5 w-1.5 rounded-full', dotColor[tone])} aria-hidden="true" />}
      {children}
    </span>
  );
}
