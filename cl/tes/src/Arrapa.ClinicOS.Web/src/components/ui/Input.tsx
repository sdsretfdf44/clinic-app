import { forwardRef, useState, type InputHTMLAttributes, type ReactNode } from 'react';

import { cn } from '@/utils/cn';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  invalid?: boolean;
  leftIcon?: ReactNode;
  rightIcon?: ReactNode;
  allowRevealPassword?: boolean;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { className, invalid, leftIcon, rightIcon, allowRevealPassword, type = 'text', ...rest },
  ref,
) {
  const [revealed, setRevealed] = useState(false);
  const effectiveType = allowRevealPassword && type === 'password' && revealed ? 'text' : type;

  return (
    <div className="relative group">
      {leftIcon && (
        <span className="pointer-events-none absolute inset-y-0 left-3 flex items-center text-gray-400 group-focus-within:text-brand-600 dark:group-focus-within:text-brand-400 [&>svg]:h-4 [&>svg]:w-4 transition-colors">
          {leftIcon}
        </span>
      )}
      <input
        ref={ref}
        type={effectiveType}
        className={cn(
          'h-10 w-full rounded-lg border bg-white text-body text-gray-900 placeholder:text-gray-400 shadow-theme-xs',
          'transition-[border-color,box-shadow] duration-150 ease-out',
          'focus:outline-none focus:border-brand-500 focus:shadow-ring-brand',
          'disabled:cursor-not-allowed disabled:bg-gray-50 disabled:text-gray-400',
          'dark:bg-gray-900 dark:text-gray-100 dark:placeholder:text-gray-500 dark:border-gray-700 dark:focus:border-brand-400',
          leftIcon ? 'pl-9' : 'pl-3.5',
          rightIcon || allowRevealPassword ? 'pr-10' : 'pr-3.5',
          invalid
            ? 'border-danger-500 focus:border-danger-500 focus:shadow-ring-danger'
            : 'border-gray-200',
          className,
        )}
        aria-invalid={invalid}
        {...rest}
      />
      {allowRevealPassword && type === 'password' && (
        <button
          type="button"
          tabIndex={-1}
          onClick={() => setRevealed((r) => !r)}
          className="absolute inset-y-0 right-3 flex items-center text-caption font-medium text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200 transition-colors"
          aria-label={revealed ? 'Hide password' : 'Show password'}
        >
          {revealed ? 'Hide' : 'Show'}
        </button>
      )}
      {!allowRevealPassword && rightIcon && (
        <span className="pointer-events-none absolute inset-y-0 right-3 flex items-center text-gray-400 [&>svg]:h-4 [&>svg]:w-4">
          {rightIcon}
        </span>
      )}
    </div>
  );
});
