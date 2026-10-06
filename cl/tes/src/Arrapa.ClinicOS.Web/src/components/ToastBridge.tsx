import { useEffect } from 'react';

import { setGlobalToastNotifier } from '@/app/queryClient';
import { useToast } from '@/components/ui/Toast';

/**
 * Bridges the toast context to the TanStack Query cache so unexpected server errors surface
 * without every hook wiring the toast manually. Mount once, high in the tree, under both providers.
 */
export function ToastBridge() {
  const toast = useToast();

  useEffect(() => {
    setGlobalToastNotifier((message, title) => toast.error(message, title));
    return () => setGlobalToastNotifier(null);
  }, [toast]);

  return null;
}
