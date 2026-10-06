import { forwardRef, type InputHTMLAttributes, type ReactNode } from 'react';
import { MagnifyingGlassIcon, XMarkIcon } from '@heroicons/react/24/outline';

import { cn } from '@/utils/cn';

interface FilterBarProps {
  children?: ReactNode;
  className?: string;
}

/**
 * Groups filter controls under a card-like surface for consistency across list pages.
 */
export function FilterBar({ children, className }: FilterBarProps) {
  return (
    <div
      className={cn(
        'rounded-xl border border-gray-200/70 dark:border-gray-800 bg-white dark:bg-gray-900 p-3 sm:p-4 shadow-theme-xs',
        className,
      )}
    >
      <div className="flex flex-wrap items-end gap-3">{children}</div>
    </div>
  );
}

interface FilterInputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'size'> {
  label?: string;
  wrapperClassName?: string;
  onClear?: () => void;
}

export const FilterSearchInput = forwardRef<HTMLInputElement, FilterInputProps>(function FilterSearchInput(
  { label, wrapperClassName, className, onClear, value, ...rest },
  ref,
) {
  const showClear = onClear && typeof value === 'string' && value.length > 0;
  return (
    <div className={cn('flex flex-col min-w-[220px] flex-1', wrapperClassName)}>
      {label && <label className="text-caption text-gray-500 dark:text-gray-400 mb-1">{label}</label>}
      <div className="relative">
        <span className="pointer-events-none absolute inset-y-0 left-3 flex items-center text-gray-400">
          <MagnifyingGlassIcon className="h-4 w-4" />
        </span>
        <input
          ref={ref}
          type="search"
          value={value}
          className={cn(
            'h-9 w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 pl-9 pr-8 text-body text-gray-900 dark:text-white placeholder:text-gray-400 shadow-theme-xs',
            'transition-[border-color,box-shadow] duration-150 ease-out focus:outline-none focus:border-brand-500 focus:shadow-ring-brand',
            className,
          )}
          {...rest}
        />
        {showClear && (
          <button
            type="button"
            onClick={onClear}
            className="absolute inset-y-0 right-2 flex items-center text-gray-400 hover:text-gray-700 dark:hover:text-gray-200"
            aria-label="Clear search"
          >
            <XMarkIcon className="h-3.5 w-3.5" />
          </button>
        )}
      </div>
    </div>
  );
});

interface FilterFieldProps {
  label?: string;
  children: ReactNode;
  className?: string;
}

export function FilterField({ label, children, className }: FilterFieldProps) {
  return (
    <div className={cn('flex flex-col min-w-[160px]', className)}>
      {label && <label className="text-caption text-gray-500 dark:text-gray-400 mb-1">{label}</label>}
      {children}
    </div>
  );
}
