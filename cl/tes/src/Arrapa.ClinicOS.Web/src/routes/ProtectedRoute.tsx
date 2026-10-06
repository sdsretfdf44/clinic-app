import { Navigate, Outlet, useLocation } from 'react-router-dom';

import { useAuthStore } from '@/store/authStore';

export function ProtectedRoute() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/sign-in" replace state={{ from: location.pathname + location.search }} />;
  }
  return <Outlet />;
}
