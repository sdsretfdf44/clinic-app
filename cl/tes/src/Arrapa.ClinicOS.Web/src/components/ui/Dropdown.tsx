import { useEffect, useId, useRef, useState, type ReactNode } from 'react';

import { cn } from '@/utils/cn';

interface DropdownProps {
  trigger: (opts: { open: boolean; toggle: () => void; menuId: string }) => ReactNode;
  children: ReactNode;
  align?: 'left' | 'right';
  className?: string;
  menuClassName?: string;
  /** Accessible label for the menu (announced by screen readers). */
  menuLabel?: string;
}

/**
 * Lightweight controlled dropdown. Close on outside click, Escape, or item click via context.
 * Arrow keys move focus between items when the menu is open.
 */
export function Dropdown({ trigger, children, align = 'right', className, menuClassName, menuLabel }: DropdownProps) {
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement | null>(null);
  const menuRef = useRef<HTMLDivElement | null>(null);
  const menuId = useId();

  useEffect(() => {
    if (!open) return;
    const onClick = (event: MouseEvent) => {
      if (rootRef.current && !rootRef.current.contains(event.target as Node)) setOpen(false);
    };
    const onKey = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setOpen(false);
        return;
      }
      if (event.key !== 'ArrowDown' && event.key !== 'ArrowUp') return;
      const items = menuRef.current?.querySelectorAll<HTMLButtonElement>('[role="menuitem"]:not([disabled])');
      if (!items || items.length === 0) return;
      event.preventDefault();
      const currentIndex = Array.from(items).indexOf(document.activeElement as HTMLButtonElement);
      const delta = event.key === 'ArrowDown' ? 1 : -1;
      const nextIndex = (currentIndex + delta + items.length) % items.length;
      items[nextIndex]?.focus();
    };
    window.addEventListener('mousedown', onClick);
    window.addEventListener('keydown', onKey);

    const focusTimer = window.setTimeout(() => {
      const first = menuRef.current?.querySelector<HTMLButtonElement>('[role="menuitem"]:not([disabled])');
      first?.focus();
    }, 0);

    return () => {
      window.removeEventListener('mousedown', onClick);
      window.removeEventListener('keydown', onKey);
      window.clearTimeout(focusTimer);
    };
  }, [open]);

  return (
    <div ref={rootRef} className={cn('relative inline-block', className)}>
      {trigger({ open, toggle: () => setOpen((v) => !v), menuId })}
      {open && (
        <div
          ref={menuRef}
          id={menuId}
          className={cn(
            'absolute z-30 mt-2 min-w-[12rem] rounded-xl border border-gray-200/70 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-theme-lg p-1 animate-slide-up',
            align === 'right' ? 'right-0' : 'left-0',
            menuClassName,
          )}
          onClick={() => setOpen(false)}
          role="menu"
          aria-label={menuLabel}
        >
          {children}
        </div>
      )}
    </div>
  );
}

interface DropdownItemProps {
  onSelect?: () => void;
  icon?: ReactNode;
  disabled?: boolean;
  danger?: boolean;
  children: ReactNode;
  className?: string;
}

export function DropdownItem({ onSelect, icon, disabled, danger, children, className }: DropdownItemProps) {
  return (
    <button
      type="button"
      role="menuitem"
      disabled={disabled}
      onClick={onSelect}
      className={cn(
        'flex w-full items-center gap-2.5 rounded-lg px-2.5 py-2 text-body text-left transition-colors duration-150 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500',
        disabled
          ? 'opacity-50 cursor-not-allowed'
          : danger
            ? 'text-danger-700 hover:bg-danger-50 dark:text-danger-500 dark:hover:bg-danger-500/10'
            : 'text-gray-700 hover:bg-gray-100 dark:text-gray-200 dark:hover:bg-gray-800',
        className,
      )}
    >
      {icon && <span className="[&>svg]:h-4 [&>svg]:w-4 shrink-0 text-gray-500 dark:text-gray-400" aria-hidden="true">{icon}</span>}
      <span className="flex-1">{children}</span>
    </button>
  );
}

export function DropdownSeparator() {
  return <div role="separator" className="my-1 h-px bg-gray-100 dark:bg-gray-800" />;
}
