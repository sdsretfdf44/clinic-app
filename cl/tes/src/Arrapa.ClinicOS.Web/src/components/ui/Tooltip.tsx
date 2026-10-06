import type { ReactNode } from 'react';

import { cn } from '@/utils/cn';

interface TooltipProps {
  label: ReactNode;
  side?: 'top' | 'bottom' | 'left' | 'right';
  children: ReactNode;
  className?: string;
}

const sideClasses = {
  top:    'bottom-full left-1/2 -translate-x-1/2 mb-2',
  bottom: 'top-full left-1/2 -translate-x-1/2 mt-2',
  left:   'right-full top-1/2 -translate-y-1/2 mr-2',
  right:  'left-full top-1/2 -translate-y-1/2 ml-2',
} as const;

/**
 * Tiny CSS-only tooltip. Use for icon buttons and truncated text; not for rich content.
 */
export function Tooltip({ label, side = 'top', children, className }: TooltipProps) {
  return (
    <span className={cn('relative inline-flex group', className)}>
      {children}
      <span
        role="tooltip"
        className={cn(
          'pointer-events-none absolute z-40 whitespace-nowrap rounded-md bg-gray-900 dark:bg-gray-800 px-2 py-1 text-[11px] font-medium text-white shadow-theme-md',
          'opacity-0 group-hover:opacity-100 group-focus-within:opacity-100 transition-opacity duration-150',
          sideClasses[side],
        )}
      >
        {label}
      </span>
    </span>
  );
}
