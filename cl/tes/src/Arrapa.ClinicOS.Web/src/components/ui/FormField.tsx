import type { ReactNode } from 'react';

import { cn } from '@/utils/cn';
import { Label } from './Label';

interface FormFieldProps {
  id: string;
  label?: string;
  required?: boolean;
  hint?: string;
  error?: string;
  className?: string;
  children: ReactNode;
}

export function FormField({ id, label, required, hint, error, className, children }: FormFieldProps) {
  const describedBy = error ? `${id}-error` : hint ? `${id}-hint` : undefined;
  return (
    <div className={cn('space-y-1.5', className)}>
      {label && (
        <Label htmlFor={id} required={required}>
          {label}
        </Label>
      )}
      <div aria-describedby={describedBy}>{children}</div>
      {hint && !error && (
        <p id={`${id}-hint`} className="text-caption text-gray-500 dark:text-gray-400">
          {hint}
        </p>
      )}
      {error && (
        <p id={`${id}-error`} className="text-caption text-danger-600 dark:text-danger-500 flex items-start gap-1">
          <svg viewBox="0 0 20 20" className="mt-0.5 h-3.5 w-3.5 shrink-0" fill="currentColor" aria-hidden="true">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm.75-11.5a.75.75 0 00-1.5 0v4.5a.75.75 0 001.5 0v-4.5zm-.75 8.75a1 1 0 100-2 1 1 0 000 2z" clipRule="evenodd" />
          </svg>
          <span>{error}</span>
        </p>
      )}
    </div>
  );
}
