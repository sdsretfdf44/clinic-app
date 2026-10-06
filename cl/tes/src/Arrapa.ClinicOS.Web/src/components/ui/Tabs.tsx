import type { ReactNode } from 'react';

import { cn } from '@/utils/cn';

export interface TabDef<T extends string = string> {
  value: T;
  label: ReactNode;
  count?: number | string;
  icon?: ReactNode;
  disabled?: boolean;
}

interface TabsProps<T extends string> {
  tabs: TabDef<T>[];
  value: T;
  onChange: (value: T) => void;
  className?: string;
  variant?: 'underline' | 'pills';
}

export function Tabs<T extends string>({
  tabs,
  value,
  onChange,
  className,
  variant = 'underline',
}: TabsProps<T>) {
  if (variant === 'pills') {
    return (
      <div className={cn('inline-flex items-center rounded-lg bg-gray-100 dark:bg-gray-800 p-1', className)} role="tablist">
        {tabs.map((t) => {
          const active = t.value === value;
          return (
            <button
              key={t.value}
              role="tab"
              aria-selected={active}
              disabled={t.disabled}
              onClick={() => onChange(t.value)}
              className={cn(
                'inline-flex items-center gap-2 rounded-md px-3 h-8 text-body font-medium transition-colors duration-150',
                active
                  ? 'bg-white text-gray-900 shadow-theme-xs dark:bg-gray-900 dark:text-white'
                  : 'text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white',
                t.disabled && 'opacity-50 cursor-not-allowed',
              )}
            >
              {t.icon}
              {t.label}
              {t.count != null && (
                <span
                  className={cn(
                    'inline-flex items-center rounded-full px-1.5 h-4 text-[10px] font-medium tabular-nums',
                    active ? 'bg-brand-50 text-brand-700 dark:bg-brand-500/10 dark:text-brand-300' : 'bg-gray-200 text-gray-700 dark:bg-gray-700 dark:text-gray-200',
                  )}
                >
                  {t.count}
                </span>
              )}
            </button>
          );
        })}
      </div>
    );
  }

  return (
    <div
      className={cn(
        'flex items-center gap-1 border-b border-gray-200/70 dark:border-gray-800 overflow-x-auto u-scrollbar',
        className,
      )}
      role="tablist"
    >
      {tabs.map((t) => {
        const active = t.value === value;
        return (
          <button
            key={t.value}
            role="tab"
            aria-selected={active}
            disabled={t.disabled}
            onClick={() => onChange(t.value)}
            className={cn(
              'group relative inline-flex items-center gap-2 px-3 py-2.5 text-body font-medium whitespace-nowrap transition-colors',
              active
                ? 'text-brand-600 dark:text-brand-400'
                : 'text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-white',
              t.disabled && 'opacity-50 cursor-not-allowed',
            )}
          >
            {t.icon}
            {t.label}
            {t.count != null && (
              <span
                className={cn(
                  'inline-flex items-center rounded-full px-1.5 py-0.5 text-[10px] font-medium tabular-nums',
                  active
                    ? 'bg-brand-50 text-brand-700 dark:bg-brand-500/10 dark:text-brand-300'
                    : 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300',
                )}
              >
                {t.count}
              </span>
            )}
            <span
              aria-hidden="true"
              className={cn(
                'absolute inset-x-2 -bottom-px h-0.5 rounded-full transition-colors',
                active ? 'bg-brand-500' : 'bg-transparent group-hover:bg-gray-200 dark:group-hover:bg-gray-700',
              )}
            />
          </button>
        );
      })}
    </div>
  );
}
