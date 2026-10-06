import type { LabelHTMLAttributes, ReactNode } from 'react';

import { cn } from '@/utils/cn';

interface LabelProps extends LabelHTMLAttributes<HTMLLabelElement> {
  required?: boolean;
  children: ReactNode;
}

export function Label({ required, className, children, ...rest }: LabelProps) {
  return (
    <label
      className={cn('block text-body font-medium text-gray-700 dark:text-gray-200 mb-1.5', className)}
      {...rest}
    >
      {children}
      {required && (
        <span className="ml-0.5 text-danger-500" aria-hidden="true">
          *
        </span>
      )}
    </label>
  );
}
