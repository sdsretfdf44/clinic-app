import { Navigate, Outlet } from 'react-router-dom';

import { useAuthStore } from '@/store/authStore';

interface RoleRouteProps {
  allow: string[];
}

/**
 * Redirects to /dashboard when the current user is missing all of the required roles.
 * Renders a nested Outlet otherwise.
 */
export function RoleRoute({ allow }: RoleRouteProps) {
  const user = useAuthStore((s) => s.user);
  if (!user) {
    return <Navigate to="/sign-in" replace />;
  }
  const hasRole = user.roles.some((role) => allow.includes(role));
  return hasRole ? <Outlet /> : <Navigate to="/dashboard" replace />;
}
