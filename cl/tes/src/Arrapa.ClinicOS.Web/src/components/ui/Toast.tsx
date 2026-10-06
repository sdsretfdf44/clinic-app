import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from 'react';
import type { ReactNode } from 'react';

import { cn } from '@/utils/cn';

export type ToastVariant = 'info' | 'success' | 'warning' | 'error';

export interface Toast {
  id: string;
  title?: string;
  message: string;
  variant: ToastVariant;
  durationMs: number;
}

interface ToastContextValue {
  show: (toast: Omit<Toast, 'id' | 'variant' | 'durationMs'> & Partial<Pick<Toast, 'variant' | 'durationMs'>>) => string;
  success: (message: string, title?: string) => string;
  error: (message: string, title?: string) => string;
  info: (message: string, title?: string) => string;
  warning: (message: string, title?: string) => string;
  dismiss: (id: string) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

const DEFAULT_DURATION_MS = 5000;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);
  const timersRef = useRef<Map<string, ReturnType<typeof setTimeout>>>(new Map());

  const dismiss = useCallback((id: string) => {
    setToasts((current) => current.filter((toast) => toast.id !== id));
    const timer = timersRef.current.get(id);
    if (timer) {
      clearTimeout(timer);
      timersRef.current.delete(id);
    }
  }, []);

  const show = useCallback<ToastContextValue['show']>((toast) => {
    const id = crypto.randomUUID();
    const next: Toast = {
      id,
      title: toast.title,
      message: toast.message,
      variant: toast.variant ?? 'info',
      durationMs: toast.durationMs ?? DEFAULT_DURATION_MS,
    };
    setToasts((current) => [...current, next]);
    if (next.durationMs > 0) {
      const timer = setTimeout(() => dismiss(id), next.durationMs);
      timersRef.current.set(id, timer);
    }
    return id;
  }, [dismiss]);

  const value = useMemo<ToastContextValue>(() => ({
    show,
    success: (message, title) => show({ message, title, variant: 'success' }),
    error: (message, title) => show({ message, title, variant: 'error', durationMs: 7000 }),
    info: (message, title) => show({ message, title, variant: 'info' }),
    warning: (message, title) => show({ message, title, variant: 'warning' }),
    dismiss,
  }), [show, dismiss]);

  useEffect(() => () => {
    timersRef.current.forEach((timer) => clearTimeout(timer));
    timersRef.current.clear();
  }, []);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <ToastViewport toasts={toasts} onDismiss={dismiss} />
    </ToastContext.Provider>
  );
}

export function useToast(): ToastContextValue {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToast must be used within a ToastProvider.');
  }
  return context;
}

const variantClasses: Record<ToastVariant, string> = {
  info:    'border-brand-200 bg-white text-gray-900 dark:bg-gray-900 dark:text-gray-100 dark:border-brand-500/30',
  success: 'border-success-200 bg-white text-gray-900 dark:bg-gray-900 dark:text-gray-100 dark:border-success-500/30',
  warning: 'border-warning-200 bg-white text-gray-900 dark:bg-gray-900 dark:text-gray-100 dark:border-warning-500/30',
  error:   'border-danger-200 bg-white text-gray-900 dark:bg-gray-900 dark:text-gray-100 dark:border-danger-500/30',
};

const accentClasses: Record<ToastVariant, string> = {
  info:    'text-brand-600 dark:text-brand-400',
  success: 'text-success-600 dark:text-success-400',
  warning: 'text-warning-600 dark:text-warning-400',
  error:   'text-danger-600 dark:text-danger-400',
};

function ToastViewport({ toasts, onDismiss }: { toasts: Toast[]; onDismiss: (id: string) => void }) {
  return (
    <div
      className="pointer-events-none fixed inset-x-0 top-4 z-[100] flex flex-col items-center gap-2 px-4 sm:top-6 sm:items-end sm:pr-6"
      aria-live="polite"
      aria-atomic="true"
    >
      {toasts.map((toast) => (
        <div
          key={toast.id}
          role={toast.variant === 'error' ? 'alert' : 'status'}
          className={cn(
            'pointer-events-auto w-full max-w-sm rounded-xl border shadow-theme-lg px-4 py-3 flex items-start gap-3',
            variantClasses[toast.variant],
          )}
        >
          <span className={cn('mt-0.5 h-2 w-2 shrink-0 rounded-full', accentClasses[toast.variant].replace('text-', 'bg-'))} />
          <div className="flex-1 min-w-0">
            {toast.title && <p className="text-caption font-semibold mb-0.5">{toast.title}</p>}
            <p className="text-body leading-snug break-words">{toast.message}</p>
          </div>
          <button
            type="button"
            className="text-gray-400 hover:text-gray-600 dark:text-gray-500 dark:hover:text-gray-300"
            onClick={() => onDismiss(toast.id)}
            aria-label="Dismiss notification"
          >
            <svg viewBox="0 0 20 20" fill="currentColor" className="h-4 w-4" aria-hidden="true">
              <path d="M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z" />
            </svg>
          </button>
        </div>
      ))}
    </div>
  );
}
