import { useEffect, useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';

import { DashboardHeader } from './parts/DashboardHeader';
import { DashboardSidebar } from './parts/DashboardSidebar';

export function DashboardLayout() {
  const [mobileNavOpen, setMobileNavOpen] = useState(false);
  const location = useLocation();

  useEffect(() => {
    setMobileNavOpen(false);
  }, [location.pathname]);

  return (
    <div className="min-h-screen flex bg-gray-50 dark:bg-gray-950">
      <div className="u-no-print contents">
        <DashboardSidebar mobileOpen={mobileNavOpen} onMobileClose={() => setMobileNavOpen(false)} />
      </div>
      <div className="flex flex-col flex-1 min-w-0">
        <div className="u-no-print contents">
          <DashboardHeader onOpenSidebar={() => setMobileNavOpen(true)} />
        </div>
        <main className="flex-1 px-4 py-6 sm:px-6 lg:px-8 lg:py-8 print:p-0">
          <div className="mx-auto w-full max-w-[1400px] animate-fade-in print:max-w-none">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
}
