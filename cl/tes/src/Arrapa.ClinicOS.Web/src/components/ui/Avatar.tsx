import { useMemo } from 'react';

import { cn } from '@/utils/cn';

interface AvatarProps {
  name?: string | null;
  src?: string | null;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
  className?: string;
}

const sizeClasses = {
  xs: 'h-6 w-6 text-[10px]',
  sm: 'h-8 w-8 text-caption',
  md: 'h-10 w-10 text-body',
  lg: 'h-12 w-12 text-title',
  xl: 'h-16 w-16 text-title-lg',
} as const;

function initialsOf(name?: string | null) {
  if (!name) return '?';
  const parts = name.trim().split(/\s+/).slice(0, 2);
  return parts.map((p) => p[0]?.toUpperCase()).join('') || '?';
}

// Deterministic accent picked from a small healthcare-appropriate palette.
const accents = [
  'bg-brand-100 text-brand-700 dark:bg-brand-500/15 dark:text-brand-300',
  'bg-teal-100 text-teal-700 dark:bg-teal-500/15 dark:text-teal-300',
  'bg-success-100 text-success-700 dark:bg-success-500/15 dark:text-success-500',
  'bg-warning-100 text-warning-700 dark:bg-warning-500/15 dark:text-warning-500',
  'bg-sky-100 text-sky-700 dark:bg-sky-500/15 dark:text-sky-300',
  'bg-gray-200 text-gray-700 dark:bg-gray-700 dark:text-gray-200',
];

export function Avatar({ name, src, size = 'md', className }: AvatarProps) {
  const accent = useMemo(() => {
    if (!name) return accents[accents.length - 1];
    let hash = 0;
    for (let i = 0; i < name.length; i++) hash = (hash * 31 + name.charCodeAt(i)) >>> 0;
    return accents[hash % accents.length];
  }, [name]);

  if (src) {
    return (
      <img
        src={src}
        alt={name ?? 'Avatar'}
        className={cn('rounded-full object-cover ring-1 ring-white/60 dark:ring-gray-900/60', sizeClasses[size], className)}
      />
    );
  }

  return (
    <span
      className={cn(
        'inline-flex items-center justify-center rounded-full font-medium select-none',
        sizeClasses[size],
        accent,
        className,
      )}
      aria-label={name ?? 'Avatar'}
    >
      {initialsOf(name)}
    </span>
  );
}
