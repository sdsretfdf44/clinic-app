import { useEffect, type ReactNode } from 'react';

import { Button } from './Button';

interface ConfirmDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  description?: ReactNode;
  confirmLabel?: string;
  cancelLabel?: string;
  variant?: 'primary' | 'danger';
  isLoading?: boolean;
}

const dangerIconPath =
  'M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z';
const infoIconPath =
  'M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z';

export function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  description,
  confirmLabel = 'Confirm',
  cancelLabel = 'Cancel',
  variant = 'primary',
  isLoading = false,
}: ConfirmDialogProps) {
  useEffect(() => {
    if (!open) return;
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', onKeyDown);
    const prev = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    return () => {
      window.removeEventListener('keydown', onKeyDown);
      document.body.style.overflow = prev;
    };
  }, [open, onClose]);

  if (!open) return null;
  const isDanger = variant === 'danger';

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center px-4 animate-fade-in" role="dialog" aria-modal="true">
      <div className="absolute inset-0 bg-gray-900/50 backdrop-blur-sm" onClick={onClose} aria-hidden="true" />
      <div className="relative w-full max-w-md rounded-2xl bg-white dark:bg-gray-900 shadow-theme-xl p-6 animate-slide-up">
        <div className="flex items-start gap-4">
          <div
            className={
              isDanger
                ? 'flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-danger-50 text-danger-600 dark:bg-danger-500/10 dark:text-danger-500'
                : 'flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-brand-50 text-brand-600 dark:bg-brand-500/10 dark:text-brand-300'
            }
          >
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} className="h-5 w-5" aria-hidden="true">
              <path strokeLinecap="round" strokeLinejoin="round" d={isDanger ? dangerIconPath : infoIconPath} />
            </svg>
          </div>
          <div className="flex-1 min-w-0">
            <h3 className="text-title text-gray-900 dark:text-white">{title}</h3>
            {description && <div className="mt-2 text-body text-gray-600 dark:text-gray-300">{description}</div>}
          </div>
        </div>
        <div className="mt-6 flex items-center justify-end gap-3">
          <Button type="button" variant="secondary" onClick={onClose} disabled={isLoading}>
            {cancelLabel}
          </Button>
          <Button
            type="button"
            variant={variant === 'danger' ? 'danger' : 'primary'}
            onClick={onConfirm}
            isLoading={isLoading}
          >
            {confirmLabel}
          </Button>
        </div>
      </div>
    </div>
  );
}
