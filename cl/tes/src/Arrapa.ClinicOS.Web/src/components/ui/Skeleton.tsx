import type { CSSProperties } from 'react';

import { cn } from '@/utils/cn';

interface SkeletonProps {
  className?: string;
  style?: CSSProperties;
}

/**
 * Base skeleton block. Compose into larger placeholders (rows, cards, avatars).
 */
export function Skeleton({ className, style }: SkeletonProps) {
  return <div className={cn('u-skeleton', className)} style={style} aria-hidden="true" />;
}

/** Multi-line text skeleton. */
export function SkeletonText({ lines = 3, className }: { lines?: number; className?: string }) {
  return (
    <div className={cn('space-y-2', className)}>
      {Array.from({ length: lines }).map((_, i) => (
        <Skeleton
          key={i}
          className={cn('h-3 rounded-md', i === lines - 1 ? 'w-2/3' : 'w-full')}
        />
      ))}
    </div>
  );
}

/** Card-shaped skeleton — matches the visual weight of a real card. */
export function SkeletonCard({ className, lines = 3 }: { className?: string; lines?: number }) {
  return (
    <div
      className={cn(
        'rounded-xl border border-gray-200/70 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 shadow-theme-xs',
        className,
      )}
    >
      <Skeleton className="h-3 w-24 rounded-md mb-3" />
      <Skeleton className="h-6 w-2/3 rounded-md mb-4" />
      <SkeletonText lines={lines} />
    </div>
  );
}

/** Circle skeleton (for avatars). */
export function SkeletonAvatar({ size = 40 }: { size?: number }) {
  return <Skeleton className="rounded-full" style={{ height: size, width: size }} />;
}
