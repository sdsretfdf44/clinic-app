import type { HTMLAttributes, ReactNode } from 'react';

import { cn } from '@/utils/cn';

interface CardProps extends HTMLAttributes<HTMLDivElement> {
  padding?: 'none' | 'sm' | 'md' | 'lg';
  elevated?: boolean;
}

const paddings = {
  none: '',
  sm: 'p-4',
  md: 'p-5',
  lg: 'p-6 sm:p-7',
} as const;

/**
 * Base surface. Prefer over ad-hoc rounded-xl border div. Same visual language everywhere.
 */
export function Card({ padding = 'md', elevated = false, className, children, ...rest }: CardProps) {
  return (
    <div
      className={cn(
        'rounded-xl border border-gray-200/70 dark:border-gray-800 bg-white dark:bg-gray-900',
        elevated ? 'shadow-theme-sm' : 'shadow-theme-xs',
        paddings[padding],
        className,
      )}
      {...rest}
    >
      {children}
    </div>
  );
}

interface CardHeaderProps {
  title: ReactNode;
  description?: ReactNode;
  action?: ReactNode;
  className?: string;
}

export function CardHeader({ title, description, action, className }: CardHeaderProps) {
  return (
    <div className={cn('flex items-start justify-between gap-3 mb-4', className)}>
      <div className="min-w-0">
        <h3 className="text-title text-gray-900 dark:text-white">{title}</h3>
        {description && <p className="mt-1 text-body text-gray-500 dark:text-gray-400">{description}</p>}
      </div>
      {action && <div className="shrink-0">{action}</div>}
    </div>
  );
}

interface CardSectionProps {
  label: ReactNode;
  action?: ReactNode;
  className?: string;
  children: ReactNode;
}

/**
 * A labelled section inside a card. Renders the label as a small caption above the content.
 */
export function CardSection({ label, action, className, children }: CardSectionProps) {
  return (
    <div className={className}>
      <div className="flex items-center justify-between mb-3">
        <p className="u-label">{label}</p>
        {action && <div className="shrink-0">{action}</div>}
      </div>
      {children}
    </div>
  );
}
