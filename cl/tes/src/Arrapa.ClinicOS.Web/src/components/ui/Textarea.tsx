import { forwardRef, type TextareaHTMLAttributes } from 'react';

import { cn } from '@/utils/cn';

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  invalid?: boolean;
}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(function Textarea(
  { className, invalid, rows = 4, ...rest },
  ref,
) {
  return (
    <textarea
      ref={ref}
      rows={rows}
      className={cn(
        'w-full rounded-lg border bg-white px-3.5 py-2.5 text-body text-gray-900 placeholder:text-gray-400 shadow-theme-xs',
        'transition-[border-color,box-shadow] duration-150 ease-out',
        'focus:outline-none focus:border-brand-500 focus:shadow-ring-brand',
        'disabled:cursor-not-allowed disabled:bg-gray-50 disabled:text-gray-400',
        'dark:bg-gray-900 dark:text-gray-100 dark:placeholder:text-gray-500 dark:border-gray-700 dark:focus:border-brand-400',
        invalid ? 'border-danger-500 focus:border-danger-500 focus:shadow-ring-danger' : 'border-gray-200',
        className,
      )}
      aria-invalid={invalid}
      {...rest}
    />
  );
});
