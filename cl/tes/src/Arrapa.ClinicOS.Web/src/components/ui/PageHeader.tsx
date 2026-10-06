import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { ChevronRightIcon, HomeIcon } from '@heroicons/react/24/outline';

import { cn } from '@/utils/cn';

export interface Breadcrumb {
  label: string;
  to?: string;
}

interface PageHeaderProps {
  eyebrow?: ReactNode;
  title: string;
  description?: ReactNode;
  actions?: ReactNode;
  breadcrumbs?: Breadcrumb[];
  className?: string;
  /** Compact spacing when true — used on nested pages. */
  compact?: boolean;
}

export function PageHeader({
  eyebrow,
  title,
  description,
  actions,
  breadcrumbs,
  className,
  compact,
}: PageHeaderProps) {
  return (
    <div className={cn('relative', compact ? 'mb-4' : 'mb-6', className)}>
      {breadcrumbs && breadcrumbs.length > 0 && (
        <nav aria-label="Breadcrumb" className="mb-3">
          <ol className="flex items-center gap-1 text-caption text-gray-500 dark:text-gray-400">
            <li>
              <Link
                to="/dashboard"
                className="inline-flex items-center gap-1 hover:text-gray-800 dark:hover:text-gray-200 transition-colors"
              >
                <HomeIcon className="h-3.5 w-3.5" />
              </Link>
            </li>
            {breadcrumbs.map((c, i) => {
              const last = i === breadcrumbs.length - 1;
              return (
                <li key={`${c.label}-${i}`} className="inline-flex items-center gap-1">
                  <ChevronRightIcon className="h-3 w-3 text-gray-300 dark:text-gray-700" />
                  {c.to && !last ? (
                    <Link to={c.to} className="hover:text-gray-800 dark:hover:text-gray-200 transition-colors">
                      {c.label}
                    </Link>
                  ) : (
                    <span
                      className={cn(
                        last ? 'text-gray-800 dark:text-gray-200 font-medium' : undefined,
                      )}
                    >
                      {c.label}
                    </span>
                  )}
                </li>
              );
            })}
          </ol>
        </nav>
      )}

      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0">
          {eyebrow && (
            <div className="inline-flex items-center gap-1.5 rounded-full bg-brand-50 dark:bg-brand-500/10 px-2.5 py-0.5 text-[11px] font-semibold uppercase tracking-wider text-brand-700 dark:text-brand-300 ring-1 ring-inset ring-brand-100 dark:ring-brand-500/20 mb-2.5">
              {eyebrow}
            </div>
          )}
          <h1
            className={cn(
              compact ? 'text-title-lg' : 'text-display',
              'text-gray-900 dark:text-white tracking-tight',
            )}
          >
            {title}
          </h1>
          {description && (
            <p className="mt-1.5 text-body text-gray-500 dark:text-gray-400 max-w-3xl">{description}</p>
          )}
        </div>
        {actions && <div className="flex items-center gap-2 shrink-0 flex-wrap">{actions}</div>}
      </div>
    </div>
  );
}
