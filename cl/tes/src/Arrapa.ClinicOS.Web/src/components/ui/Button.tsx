import { forwardRef, type ButtonHTMLAttributes, type ReactNode } from 'react';

import { cn } from '@/utils/cn';
import { Spinner } from './Spinner';

export type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger' | 'link' | 'outline' | 'success';
export type ButtonSize = 'sm' | 'md' | 'lg' | 'xl';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  size?: ButtonSize;
  isLoading?: boolean;
  leftIcon?: ReactNode;
  rightIcon?: ReactNode;
  fullWidth?: boolean;
}

const variantClasses: Record<ButtonVariant, string> = {
  primary:
    'bg-brand-600 text-white shadow-theme-xs hover:bg-brand-700 active:bg-brand-800 disabled:bg-brand-300 disabled:cursor-not-allowed',
  secondary:
    'bg-white text-gray-700 border border-gray-200 shadow-theme-xs hover:bg-gray-50 hover:border-gray-300 active:bg-gray-100 dark:bg-gray-900 dark:text-gray-100 dark:border-gray-700 dark:hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed',
  outline:
    'bg-transparent text-brand-700 border border-brand-200 hover:bg-brand-50 hover:border-brand-300 active:bg-brand-100 dark:text-brand-300 dark:border-brand-500/40 dark:hover:bg-brand-500/10 disabled:opacity-50 disabled:cursor-not-allowed',
  ghost:
    'bg-transparent text-gray-700 hover:bg-gray-100 active:bg-gray-200 dark:text-gray-100 dark:hover:bg-gray-800 dark:active:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed',
  danger:
    'bg-danger-600 text-white shadow-theme-xs hover:bg-danger-700 active:bg-danger-700 disabled:bg-danger-500/60 disabled:cursor-not-allowed',
  success:
    'bg-success-600 text-white shadow-theme-xs hover:bg-success-700 active:bg-success-700 disabled:opacity-60 disabled:cursor-not-allowed',
  link:
    'bg-transparent text-brand-600 hover:text-brand-700 underline-offset-4 hover:underline p-0 h-auto shadow-none dark:text-brand-400 dark:hover:text-brand-300',
};

const sizeClasses: Record<ButtonSize, string> = {
  sm: 'text-caption h-8 px-3 gap-1.5',
  md: 'text-body h-10 px-3.5',
  lg: 'text-body h-11 px-4',
  xl: 'text-body-lg h-12 px-5',
};

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(function Button(
  {
    variant = 'primary',
    size = 'md',
    isLoading = false,
    disabled,
    leftIcon,
    rightIcon,
    fullWidth,
    className,
    children,
    type = 'button',
    ...rest
  },
  ref,
) {
  const isDisabled = disabled || isLoading;
  return (
    <button
      ref={ref}
      type={type}
      disabled={isDisabled}
      aria-busy={isLoading || undefined}
      className={cn(
        'inline-flex items-center justify-center gap-2 rounded-lg font-medium whitespace-nowrap',
        'transition-[background-color,color,border-color,box-shadow] duration-150 ease-out-quart',
        variant !== 'link' && sizeClasses[size],
        variantClasses[variant],
        fullWidth && 'w-full',
        className,
      )}
      {...rest}
    >
      {isLoading ? <Spinner className="h-4 w-4" /> : leftIcon}
      {children}
      {!isLoading && rightIcon}
    </button>
  );
});
