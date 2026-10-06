import { Link } from 'react-router-dom';
import { ArrowLeftIcon, HomeIcon, MagnifyingGlassIcon } from '@heroicons/react/24/outline';

export default function NotFoundPage() {
  return (
    <main className="relative min-h-screen flex items-center justify-center px-4 overflow-hidden bg-white dark:bg-gray-950">
      {/* Decorative background */}
      <div aria-hidden="true" className="pointer-events-none absolute inset-0">
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 h-[560px] w-[560px] rounded-full bg-brand-500/10 blur-3xl" />
        <div className="absolute bottom-0 right-0 h-80 w-80 rounded-full bg-teal-500/10 blur-3xl" />
        <div
          className="absolute inset-0 opacity-[0.04] dark:opacity-[0.06]"
          style={{
            backgroundImage:
              'radial-gradient(circle at 1px 1px, currentColor 1px, transparent 0)',
            backgroundSize: '24px 24px',
          }}
        />
      </div>

      <div className="relative z-10 text-center max-w-xl">
        <div className="inline-flex items-center gap-1.5 rounded-full bg-brand-50 dark:bg-brand-500/10 px-3 py-1 text-caption font-semibold text-brand-700 dark:text-brand-300 ring-1 ring-inset ring-brand-100 dark:ring-brand-500/20">
          Error 404
        </div>

        <h1 className="mt-6 text-6xl sm:text-8xl font-semibold tracking-tight text-gray-900 dark:text-white leading-none">
          <span className="bg-gradient-to-b from-brand-600 to-teal-500 bg-clip-text text-transparent">4</span>
          <span className="text-gray-900 dark:text-white">0</span>
          <span className="bg-gradient-to-b from-brand-600 to-teal-500 bg-clip-text text-transparent">4</span>
        </h1>

        <h2 className="mt-4 text-2xl sm:text-3xl font-semibold text-gray-900 dark:text-white tracking-tight">
          We couldn&apos;t find that page.
        </h2>
        <p className="mt-3 text-body-lg text-gray-500 dark:text-gray-400">
          The page you&apos;re looking for may have moved, been renamed, or never existed. Let&apos;s get you back on
          track.
        </p>

        <div className="mt-8 flex flex-col sm:flex-row gap-3 justify-center">
          <Link
            to="/"
            className="inline-flex justify-center items-center gap-1.5 h-11 px-5 rounded-lg bg-brand-600 text-white text-body font-medium shadow-theme-xs hover:bg-brand-700 transition-colors"
          >
            <HomeIcon className="h-4 w-4" />
            Back to home
          </Link>
          <button
            type="button"
            onClick={() => window.history.back()}
            className="inline-flex justify-center items-center gap-1.5 h-11 px-5 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-gray-800 dark:text-gray-100 text-body font-medium hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
          >
            <ArrowLeftIcon className="h-4 w-4" />
            Go back
          </button>
        </div>

        <div className="mt-10 pt-8 border-t border-gray-100 dark:border-gray-800">
          <p className="text-caption uppercase tracking-widest text-gray-500 dark:text-gray-400 font-semibold mb-4">
            Common destinations
          </p>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-2">
            {[
              { to: '/dashboard', label: 'Dashboard' },
              { to: '/patients', label: 'Patients' },
              { to: '/appointments', label: 'Appointments' },
            ].map((l) => (
              <Link
                key={l.to}
                to={l.to}
                className="group inline-flex items-center justify-between gap-2 px-4 py-2.5 rounded-lg border border-gray-100 dark:border-gray-800 bg-white/50 dark:bg-gray-900/50 hover:border-brand-200 dark:hover:border-brand-500/40 hover:bg-brand-50 dark:hover:bg-brand-500/10 transition-colors"
              >
                <span className="text-body text-gray-700 dark:text-gray-200 group-hover:text-brand-700 dark:group-hover:text-brand-300">
                  {l.label}
                </span>
                <MagnifyingGlassIcon className="h-3.5 w-3.5 text-gray-400 group-hover:text-brand-500" />
              </Link>
            ))}
          </div>
        </div>
      </div>
    </main>
  );
}
