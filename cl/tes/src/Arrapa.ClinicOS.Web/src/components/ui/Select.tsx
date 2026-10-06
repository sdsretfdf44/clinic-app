import { forwardRef, type SelectHTMLAttributes } from 'react';

import { cn } from '@/utils/cn';

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  invalid?: boolean;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select(
  { className, invalid, children, ...rest },
  ref,
) {
  return (
    <div className="relative">
      <select
        ref={ref}
        className={cn(
          'h-10 w-full rounded-lg border bg-white pl-3.5 pr-9 text-body text-gray-900 shadow-theme-xs appearance-none',
          'transition-[border-color,box-shadow] duration-150 ease-out',
          'focus:outline-none focus:border-brand-500 focus:shadow-ring-brand',
          'disabled:cursor-not-allowed disabled:bg-gray-50 disabled:text-gray-400',
          'dark:bg-gray-900 dark:text-gray-100 dark:border-gray-700 dark:focus:border-brand-400',
          invalid ? 'border-danger-500 focus:border-danger-500 focus:shadow-ring-danger' : 'border-gray-200',
          className,
        )}
        aria-invalid={invalid}
        {...rest}
      >
        {children}
      </select>
      <svg
        aria-hidden="true"
        viewBox="0 0 20 20"
        className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400"
      >
        <path fill="currentColor" d="M5.516 7.548L10 12.032l4.484-4.484L15.5 8.564l-5.5 5.5-5.5-5.5z" />
      </svg>
    </div>
  );
});
