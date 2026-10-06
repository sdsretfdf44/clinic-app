import { MutationCache, QueryCache, QueryClient } from '@tanstack/react-query';

import { humanizeError } from '@/api/httpClient';

type ToastNotifier = (message: string, title?: string) => void;

let toastNotifier: ToastNotifier | null = null;

/**
 * Registers the app-level toast notifier so query/mutation caches can surface unexpected errors
 * without every hook having to plumb the toast through. Called once by <ToastBridge/> after mount.
 */
export function setGlobalToastNotifier(notifier: ToastNotifier | null): void {
  toastNotifier = notifier;
}

function shouldNotifyForError(error: unknown): boolean {
  if (!(error && typeof error === 'object' && 'response' in error)) {
    return true;
  }
  const status = (error as { response?: { status?: number } }).response?.status;
  if (status === undefined) return true;
  // 401/403 are handled by the auth interceptor + route guards; 4xx often bubble up into forms
  // that show inline errors, so we only auto-toast on 5xx.
  return status >= 500;
}

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 30_000,
      gcTime: 5 * 60_000,
    },
    mutations: {
      retry: 0,
    },
  },
  queryCache: new QueryCache({
    onError: (error) => {
      if (toastNotifier && shouldNotifyForError(error)) {
        toastNotifier(humanizeError(error), 'Something went wrong');
      }
    },
  }),
  mutationCache: new MutationCache({
    onError: (error) => {
      if (toastNotifier && shouldNotifyForError(error)) {
        toastNotifier(humanizeError(error), 'Action failed');
      }
    },
  }),
});
