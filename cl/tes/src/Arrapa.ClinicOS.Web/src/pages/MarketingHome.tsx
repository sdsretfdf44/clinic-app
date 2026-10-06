import { useState, type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import {
  ArrowRightIcon,
  CalendarDaysIcon,
  UsersIcon,
  BeakerIcon,
  CurrencyDollarIcon,
  ChartBarIcon,
  RectangleGroupIcon,
  ShieldCheckIcon,
  SparklesIcon,
  BoltIcon,
  CheckIcon,
  MinusIcon,
  PlusIcon,
  MoonIcon,
  SunIcon,
  Bars3Icon,
  XMarkIcon,
} from '@heroicons/react/24/outline';

import { useTheme } from '@/contexts/ThemeContext';
import { useAuthStore } from '@/store/authStore';

export default function MarketingHome() {
  return (
    <div className="min-h-screen bg-white dark:bg-gray-950 text-gray-900 dark:text-gray-100 antialiased">
      <MarketingHeader />
      <main>
        <Hero />
        <TrustBar />
        <Features />
        <Showcase />
        <Testimonial />
        <Pricing />
        <FAQ />
        <FinalCTA />
      </main>
      <MarketingFooter />
    </div>
  );
}

/* ─────────────────────────── Header ─────────────────────────── */

function MarketingHeader() {
  const { theme, toggleTheme } = useTheme();
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const [open, setOpen] = useState(false);

  return (
    <header className="sticky top-0 z-40 border-b border-gray-100 dark:border-gray-800 bg-white/80 dark:bg-gray-950/80 backdrop-blur">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
        <Link to="/" className="flex items-center gap-2.5">
          <BrandMark />
          <div className="leading-tight">
            <p className="text-caption uppercase tracking-widest text-brand-600 dark:text-brand-400 font-semibold">
              Arrapa
            </p>
            <p className="text-body font-semibold text-gray-900 dark:text-white">ClinicOS</p>
          </div>
        </Link>

        <nav className="hidden md:flex items-center gap-8 text-body text-gray-600 dark:text-gray-300">
          <a href="#features" className="hover:text-gray-900 dark:hover:text-white transition-colors">Features</a>
          <a href="#showcase" className="hover:text-gray-900 dark:hover:text-white transition-colors">How it works</a>
          <a href="#pricing" className="hover:text-gray-900 dark:hover:text-white transition-colors">Pricing</a>
          <a href="#faq" className="hover:text-gray-900 dark:hover:text-white transition-colors">FAQ</a>
        </nav>

        <div className="hidden md:flex items-center gap-2">
          <button
            type="button"
            onClick={toggleTheme}
            className="inline-flex h-9 w-9 items-center justify-center rounded-lg text-gray-500 hover:bg-gray-100 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white transition-colors"
            aria-label="Toggle theme"
          >
            {theme === 'dark' ? <SunIcon className="h-4 w-4" /> : <MoonIcon className="h-4 w-4" />}
          </button>
          {isAuthenticated ? (
            <Link
              to="/dashboard"
              className="inline-flex items-center gap-1.5 h-9 px-4 rounded-lg text-body font-medium bg-brand-600 text-white hover:bg-brand-700 shadow-theme-xs transition-colors"
            >
              Go to workspace <ArrowRightIcon className="h-3.5 w-3.5" />
            </Link>
          ) : (
            <>
              <Link
                to="/sign-in"
                className="inline-flex h-9 items-center px-3 rounded-lg text-body font-medium text-gray-700 dark:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
              >
                Sign in
              </Link>
              <Link
                to="/register"
                className="inline-flex items-center gap-1.5 h-9 px-4 rounded-lg text-body font-medium bg-brand-600 text-white hover:bg-brand-700 shadow-theme-xs transition-colors"
              >
                Start free trial
              </Link>
            </>
          )}
        </div>

        <button
          type="button"
          onClick={() => setOpen((o) => !o)}
          className="md:hidden inline-flex h-9 w-9 items-center justify-center rounded-lg text-gray-600 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800"
          aria-label={open ? 'Close menu' : 'Open menu'}
        >
          {open ? <XMarkIcon className="h-5 w-5" /> : <Bars3Icon className="h-5 w-5" />}
        </button>
      </div>

      {open && (
        <div className="md:hidden border-t border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-950">
          <div className="mx-auto max-w-7xl px-4 py-4 space-y-1">
            {[
              ['Features', '#features'],
              ['How it works', '#showcase'],
              ['Pricing', '#pricing'],
              ['FAQ', '#faq'],
            ].map(([label, href]) => (
              <a
                key={href}
                href={href}
                onClick={() => setOpen(false)}
                className="block px-3 py-2 rounded-lg text-body text-gray-700 dark:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800"
              >
                {label}
              </a>
            ))}
            <div className="pt-2 mt-2 border-t border-gray-100 dark:border-gray-800 flex gap-2">
              {isAuthenticated ? (
                <Link
                  to="/dashboard"
                  className="flex-1 inline-flex justify-center items-center h-10 rounded-lg bg-brand-600 text-white font-medium"
                >
                  Go to workspace
                </Link>
              ) : (
                <>
                  <Link
                    to="/sign-in"
                    className="flex-1 inline-flex justify-center items-center h-10 rounded-lg border border-gray-200 dark:border-gray-700 text-gray-800 dark:text-gray-100 font-medium"
                  >
                    Sign in
                  </Link>
                  <Link
                    to="/register"
                    className="flex-1 inline-flex justify-center items-center h-10 rounded-lg bg-brand-600 text-white font-medium"
                  >
                    Start free trial
                  </Link>
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </header>
  );
}

/* ─────────────────────────── Hero ─────────────────────────── */

function Hero() {
  return (
    <section className="relative overflow-hidden">
      {/* Decorative gradient wash */}
      <div aria-hidden="true" className="pointer-events-none absolute inset-x-0 -top-40 h-[560px]">
        <div className="mx-auto h-full max-w-6xl relative">
          <div className="absolute -top-16 left-1/2 -translate-x-1/2 h-[420px] w-[720px] rounded-full bg-brand-500/15 blur-3xl" />
          <div className="absolute top-20 right-8 h-[260px] w-[260px] rounded-full bg-teal-500/15 blur-3xl" />
        </div>
      </div>

      <div className="relative mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 pt-16 sm:pt-24 pb-16 sm:pb-24">
        <div className="grid lg:grid-cols-[1.05fr_1fr] gap-12 lg:gap-16 items-center">
          <div>
            <span className="inline-flex items-center gap-2 rounded-full bg-brand-50 dark:bg-brand-500/10 px-3 py-1 text-caption font-medium text-brand-700 dark:text-brand-300 ring-1 ring-inset ring-brand-100 dark:ring-brand-500/20">
              <SparklesIcon className="h-3.5 w-3.5" />
              The complete clinic operating system
            </span>

            <h1 className="mt-5 text-4xl sm:text-5xl lg:text-6xl font-semibold tracking-tight text-gray-900 dark:text-white leading-[1.05]">
              Run your clinic —{' '}
              <span className="bg-gradient-to-r from-brand-600 to-teal-500 bg-clip-text text-transparent">
                from booking to billing
              </span>
              , in one place.
            </h1>

            <p className="mt-5 text-body-lg text-gray-600 dark:text-gray-400 max-w-xl">
              Arrapa ClinicOS gives every role — reception, doctors, admins, and patients — the exact tools they
              need. Modern, fast, and built for the way small and mid-sized clinics actually work.
            </p>

            <div className="mt-8 flex flex-col sm:flex-row gap-3">
              <Link
                to="/register"
                className="inline-flex justify-center items-center gap-1.5 h-12 px-6 rounded-lg bg-brand-600 text-white text-body-lg font-medium shadow-theme-sm hover:bg-brand-700 transition-colors"
              >
                Start free 14-day trial
                <ArrowRightIcon className="h-4 w-4" />
              </Link>
              <a
                href="#showcase"
                className="inline-flex justify-center items-center gap-1.5 h-12 px-6 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 text-gray-800 dark:text-gray-100 text-body-lg font-medium hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
              >
                See it in action
              </a>
            </div>

            <ul className="mt-8 flex flex-wrap gap-x-6 gap-y-2 text-caption text-gray-500 dark:text-gray-400">
              <li className="inline-flex items-center gap-1.5">
                <CheckIcon className="h-4 w-4 text-success-600" /> No credit card required
              </li>
              <li className="inline-flex items-center gap-1.5">
                <CheckIcon className="h-4 w-4 text-success-600" /> Data isolated per clinic
              </li>
              <li className="inline-flex items-center gap-1.5">
                <CheckIcon className="h-4 w-4 text-success-600" /> Cancel any time
              </li>
            </ul>
          </div>

          {/* Dashboard preview mock */}
          <div className="relative">
            <div className="relative rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-theme-xl overflow-hidden">
              {/* window chrome */}
              <div className="flex items-center gap-1.5 px-4 h-9 border-b border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-900/60">
                <span className="h-2.5 w-2.5 rounded-full bg-gray-300 dark:bg-gray-700" />
                <span className="h-2.5 w-2.5 rounded-full bg-gray-300 dark:bg-gray-700" />
                <span className="h-2.5 w-2.5 rounded-full bg-gray-300 dark:bg-gray-700" />
                <span className="ml-3 text-caption text-gray-400 dark:text-gray-500 font-mono truncate">
                  app.arrapa.software/clinic
                </span>
              </div>

              <div className="p-4 sm:p-5 space-y-4">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-caption uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      Today
                    </p>
                    <p className="text-title font-semibold text-gray-900 dark:text-white">Clinic overview</p>
                  </div>
                  <span className="inline-flex items-center gap-1.5 rounded-full bg-success-50 dark:bg-success-500/10 px-2 py-0.5 text-caption font-medium text-success-700 dark:text-success-500">
                    <span className="h-1.5 w-1.5 rounded-full bg-success-500" /> All systems
                  </span>
                </div>

                <div className="grid grid-cols-3 gap-3">
                  {[
                    { l: 'Appointments', v: '128', t: 'text-brand-600' },
                    { l: 'In consult', v: '6', t: 'text-warning-600' },
                    { l: 'Revenue', v: '$4.2k', t: 'text-teal-600' },
                  ].map((s) => (
                    <div
                      key={s.l}
                      className="rounded-xl border border-gray-100 dark:border-gray-800 p-3 bg-gray-50 dark:bg-gray-950/40"
                    >
                      <p className="text-caption text-gray-500 dark:text-gray-400">{s.l}</p>
                      <p className={`mt-0.5 text-title font-semibold tabular-nums ${s.t}`}>{s.v}</p>
                    </div>
                  ))}
                </div>

                <div className="rounded-xl border border-gray-100 dark:border-gray-800 p-3">
                  <div className="flex items-center justify-between mb-3">
                    <p className="text-caption uppercase tracking-wider text-gray-500 dark:text-gray-400">
                      Appointments — last 7 days
                    </p>
                    <span className="text-caption text-success-600 font-medium">+12%</span>
                  </div>
                  <div className="flex items-end gap-2 h-24">
                    {[38, 52, 44, 66, 58, 72, 84].map((h, i) => (
                      <div
                        key={i}
                        className="flex-1 rounded-md bg-gradient-to-t from-brand-500 to-brand-300 opacity-90"
                        style={{ height: `${h}%` }}
                      />
                    ))}
                  </div>
                </div>

                <div className="space-y-2">
                  {[
                    { name: 'Sarah M.', time: '10:30', status: 'Confirmed', tone: 'bg-success-50 text-success-700' },
                    { name: 'Alex T.', time: '11:00', status: 'Checked in', tone: 'bg-brand-50 text-brand-700' },
                    { name: 'Priya N.', time: '11:30', status: 'Scheduled', tone: 'bg-gray-100 text-gray-700' },
                  ].map((a) => (
                    <div key={a.name} className="flex items-center gap-3 py-2 px-3 rounded-lg bg-white dark:bg-gray-900 border border-gray-100 dark:border-gray-800">
                      <div className="h-7 w-7 rounded-full bg-gradient-to-br from-brand-400 to-teal-400 text-white text-caption font-semibold flex items-center justify-center">
                        {a.name.charAt(0)}
                      </div>
                      <div className="min-w-0 flex-1">
                        <p className="text-caption font-medium text-gray-900 dark:text-white truncate">{a.name}</p>
                        <p className="text-micro text-gray-500 dark:text-gray-400 font-mono">{a.time}</p>
                      </div>
                      <span className={`text-micro font-medium px-2 py-0.5 rounded-full ${a.tone} dark:bg-opacity-10`}>
                        {a.status}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Floating trust chip */}
            <div className="absolute -left-4 -bottom-4 hidden sm:flex items-center gap-2 rounded-xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 px-3 py-2 shadow-theme-lg">
              <span className="inline-flex h-8 w-8 items-center justify-center rounded-lg bg-success-50 dark:bg-success-500/10 text-success-600">
                <ShieldCheckIcon className="h-4 w-4" />
              </span>
              <div>
                <p className="text-caption font-semibold text-gray-900 dark:text-white">Tenant-isolated</p>
                <p className="text-micro text-gray-500 dark:text-gray-400">Enforced at the database</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

/* ─────────────────────────── Trust bar ─────────────────────────── */

function TrustBar() {
  const logos = ['Northwood Clinic', 'Rivergate Health', 'Meridian Care', 'Bayline Medical', 'Cedarwood', 'Elmpath'];
  return (
    <section className="border-y border-gray-100 dark:border-gray-800 bg-gray-50/60 dark:bg-gray-900/40">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-8">
        <p className="text-center text-caption uppercase tracking-widest text-gray-500 dark:text-gray-400">
          Built for the clinics that raise the bar
        </p>
        <div className="mt-5 grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-6 items-center">
          {logos.map((l) => (
            <div
              key={l}
              className="text-center text-body font-semibold text-gray-400 dark:text-gray-500 tracking-tight"
            >
              {l}
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

/* ─────────────────────────── Features ─────────────────────────── */

const features = [
  {
    icon: CalendarDaysIcon,
    title: 'Smart appointment scheduling',
    body: 'Doctor working hours, department calendars, walk-ins, and no-show tracking — with reminders that actually get read.',
    tone: 'brand',
  },
  {
    icon: UsersIcon,
    title: 'Patient records that scale',
    body: 'One clean patient chart. History, allergies, prescriptions, and visits — searchable in milliseconds.',
    tone: 'teal',
  },
  {
    icon: BeakerIcon,
    title: 'e-Prescriptions & consultations',
    body: 'Doctors capture consultations and prescribe from templates — patients get a copy in their portal instantly.',
    tone: 'brand',
  },
  {
    icon: CurrencyDollarIcon,
    title: 'Billing without the friction',
    body: 'Invoice at checkout, accept payments, and reconcile — with tax and discount rules that stay out of the way.',
    tone: 'teal',
  },
  {
    icon: RectangleGroupIcon,
    title: 'A portal patients actually use',
    body: 'Book, reschedule, view prescriptions, and pay invoices. Fewer front-desk calls, more happy patients.',
    tone: 'brand',
  },
  {
    icon: ChartBarIcon,
    title: 'Insights, not spreadsheets',
    body: 'KPIs across appointments, revenue, and staff productivity — the numbers you need before the next quarter.',
    tone: 'teal',
  },
];

const toneMap = {
  brand: 'bg-brand-50 text-brand-600 dark:bg-brand-500/10 dark:text-brand-300 ring-brand-100 dark:ring-brand-500/20',
  teal: 'bg-teal-50 text-teal-600 dark:bg-teal-500/10 dark:text-teal-300 ring-teal-100 dark:ring-teal-500/20',
} as const;

function Features() {
  return (
    <section id="features" className="scroll-mt-24">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-20 sm:py-28">
        <div className="max-w-2xl">
          <p className="text-caption uppercase tracking-widest text-brand-600 dark:text-brand-400 font-semibold">
            Everything you need
          </p>
          <h2 className="mt-2 text-3xl sm:text-4xl font-semibold tracking-tight text-gray-900 dark:text-white">
            One platform. Every workflow your clinic runs on.
          </h2>
          <p className="mt-3 text-body-lg text-gray-600 dark:text-gray-400">
            Stop stitching together booking tools, spreadsheets, and paper. Arrapa ClinicOS handles the entire
            clinical journey — from first appointment to final invoice.
          </p>
        </div>

        <div className="mt-12 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {features.map((f) => (
            <FeatureCard
              key={f.title}
              icon={<f.icon className="h-5 w-5" />}
              title={f.title}
              body={f.body}
              toneClass={toneMap[f.tone as keyof typeof toneMap]}
            />
          ))}
        </div>
      </div>
    </section>
  );
}

function FeatureCard({ icon, title, body, toneClass }: { icon: ReactNode; title: string; body: string; toneClass: string }) {
  return (
    <div className="group rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 hover:border-gray-200 dark:hover:border-gray-700 hover:shadow-theme-md transition-all duration-200">
      <span className={`inline-flex h-10 w-10 items-center justify-center rounded-xl ring-1 ring-inset ${toneClass}`}>
        {icon}
      </span>
      <h3 className="mt-4 text-title font-semibold text-gray-900 dark:text-white">{title}</h3>
      <p className="mt-2 text-body text-gray-600 dark:text-gray-400 leading-relaxed">{body}</p>
    </div>
  );
}

/* ─────────────────────────── Showcase ─────────────────────────── */

function Showcase() {
  return (
    <section id="showcase" className="scroll-mt-24 bg-gray-50/60 dark:bg-gray-900/40 border-y border-gray-100 dark:border-gray-800">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-20 sm:py-28 space-y-24">
        <ShowcaseRow
          eyebrow="For reception"
          title="Turn a busy front desk into a calm one."
          body="Check patients in, register walk-ins, resolve balances, and see the entire queue at a glance. Every action is one keystroke away."
          bullets={[
            'Live check-in queue with wait times',
            'Walk-in registration in under 30 seconds',
            'Payment collection and receipts at the desk',
          ]}
          mock={<ReceptionMock />}
        />
        <ShowcaseRow
          reverse
          eyebrow="For doctors"
          title="Consult without fighting your software."
          body="Your daily queue, the patient chart, and a clean consultation canvas — all a click apart. Templates keep prescriptions and notes fast."
          bullets={[
            'Today\'s queue with real-time status',
            'Structured consultation notes',
            'Reusable prescription templates',
          ]}
          mock={<DoctorMock />}
        />
      </div>
    </section>
  );
}

function ShowcaseRow({
  eyebrow,
  title,
  body,
  bullets,
  mock,
  reverse,
}: {
  eyebrow: string;
  title: string;
  body: string;
  bullets: string[];
  mock: ReactNode;
  reverse?: boolean;
}) {
  return (
    <div className={`grid lg:grid-cols-2 gap-12 items-center ${reverse ? 'lg:[&>*:first-child]:order-2' : ''}`}>
      <div>
        <p className="text-caption uppercase tracking-widest text-brand-600 dark:text-brand-400 font-semibold">
          {eyebrow}
        </p>
        <h3 className="mt-2 text-2xl sm:text-3xl font-semibold tracking-tight text-gray-900 dark:text-white">
          {title}
        </h3>
        <p className="mt-3 text-body-lg text-gray-600 dark:text-gray-400">{body}</p>
        <ul className="mt-6 space-y-3">
          {bullets.map((b) => (
            <li key={b} className="flex items-start gap-3 text-body text-gray-700 dark:text-gray-300">
              <span className="mt-0.5 inline-flex h-5 w-5 items-center justify-center rounded-full bg-success-50 dark:bg-success-500/10 text-success-600 shrink-0">
                <CheckIcon className="h-3 w-3" strokeWidth={3} />
              </span>
              <span>{b}</span>
            </li>
          ))}
        </ul>
      </div>
      <div>{mock}</div>
    </div>
  );
}

function ReceptionMock() {
  const rows = [
    { name: 'Sarah Miller', t: '10:30', status: 'Waiting', wait: '4m', tone: 'bg-warning-50 text-warning-700' },
    { name: 'Alex Turner', t: '10:45', status: 'In consult', wait: '—', tone: 'bg-brand-50 text-brand-700' },
    { name: 'Priya Nair', t: '11:00', status: 'Checked in', wait: '1m', tone: 'bg-success-50 text-success-700' },
    { name: 'David Ochoa', t: '11:15', status: 'Scheduled', wait: '—', tone: 'bg-gray-100 text-gray-700' },
  ];
  return (
    <div className="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-theme-lg overflow-hidden">
      <div className="px-5 py-3 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between">
        <p className="text-body font-semibold text-gray-900 dark:text-white">Today's queue</p>
        <span className="text-caption text-gray-500 dark:text-gray-400 tabular-nums">4 patients · 2 doctors on floor</span>
      </div>
      <div className="divide-y divide-gray-100 dark:divide-gray-800">
        {rows.map((r) => (
          <div key={r.name} className="flex items-center gap-3 px-5 py-3">
            <div className="h-8 w-8 rounded-full bg-gradient-to-br from-brand-400 to-teal-400 text-white text-caption font-semibold flex items-center justify-center">
              {r.name.charAt(0)}
            </div>
            <div className="min-w-0 flex-1">
              <p className="text-body font-medium text-gray-900 dark:text-white truncate">{r.name}</p>
              <p className="text-caption text-gray-500 dark:text-gray-400 font-mono">{r.t}</p>
            </div>
            <span className="text-caption text-gray-500 dark:text-gray-400 tabular-nums w-10 text-right">{r.wait}</span>
            <span className={`text-caption font-medium px-2 py-0.5 rounded-full ${r.tone} dark:bg-opacity-10`}>
              {r.status}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}

function DoctorMock() {
  return (
    <div className="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-theme-lg overflow-hidden">
      <div className="px-5 py-3 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between">
        <div>
          <p className="text-body font-semibold text-gray-900 dark:text-white">Consultation · Alex Turner</p>
          <p className="text-caption text-gray-500 dark:text-gray-400">MRN A-00821 · 34y · General practice</p>
        </div>
        <span className="text-caption font-medium px-2 py-0.5 rounded-full bg-brand-50 text-brand-700 dark:bg-brand-500/10 dark:text-brand-300">
          In consult
        </span>
      </div>
      <div className="p-5 space-y-4">
        <div>
          <p className="text-caption uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Chief complaint</p>
          <p className="text-body text-gray-800 dark:text-gray-100">Sore throat, low-grade fever for 3 days.</p>
        </div>
        <div className="grid grid-cols-3 gap-3">
          {[
            { l: 'BP', v: '118/76' },
            { l: 'HR', v: '82 bpm' },
            { l: 'Temp', v: '37.6°C' },
          ].map((v) => (
            <div key={v.l} className="rounded-lg bg-gray-50 dark:bg-gray-950/40 px-3 py-2 border border-gray-100 dark:border-gray-800">
              <p className="text-caption text-gray-500 dark:text-gray-400">{v.l}</p>
              <p className="text-body font-semibold text-gray-900 dark:text-white tabular-nums">{v.v}</p>
            </div>
          ))}
        </div>
        <div>
          <p className="text-caption uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-2">Prescription</p>
          <div className="rounded-lg border border-gray-100 dark:border-gray-800 divide-y divide-gray-100 dark:divide-gray-800">
            {[
              ['Paracetamol 500mg', '1 tab · TID · 5 days'],
              ['Amoxicillin 500mg', '1 cap · TID · 7 days'],
            ].map(([n, d]) => (
              <div key={n} className="flex items-center justify-between px-3 py-2">
                <p className="text-body font-medium text-gray-900 dark:text-white">{n}</p>
                <p className="text-caption text-gray-500 dark:text-gray-400 font-mono">{d}</p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

/* ─────────────────────────── Testimonial ─────────────────────────── */

function Testimonial() {
  return (
    <section className="scroll-mt-24">
      <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8 py-20 sm:py-24">
        <figure className="text-center">
          <BoltIcon className="mx-auto h-8 w-8 text-brand-500" />
          <blockquote className="mt-6 text-2xl sm:text-3xl font-medium tracking-tight text-gray-900 dark:text-white leading-snug">
            &ldquo;We replaced three separate tools with ClinicOS. Front desk is calmer, doctors are faster, and I
            finally have the numbers to run the business — not just the clinic.&rdquo;
          </blockquote>
          <figcaption className="mt-6 flex items-center justify-center gap-3">
            <div className="h-10 w-10 rounded-full bg-gradient-to-br from-brand-500 to-teal-500 text-white font-semibold flex items-center justify-center">
              R
            </div>
            <div className="text-left">
              <p className="text-body font-semibold text-gray-900 dark:text-white">Dr. Reena Kapoor</p>
              <p className="text-caption text-gray-500 dark:text-gray-400">Founder, Northwood Clinic</p>
            </div>
          </figcaption>
        </figure>
      </div>
    </section>
  );
}

/* ─────────────────────────── Pricing ─────────────────────────── */

const plans = [
  {
    name: 'Starter',
    price: '$49',
    tagline: 'For solo practitioners just getting online.',
    features: ['1 doctor', 'Up to 500 patients', 'Appointments & billing', 'Patient portal', 'Email support'],
    cta: 'Start trial',
    featured: false,
  },
  {
    name: 'Growth',
    price: '$149',
    tagline: 'For multi-doctor clinics ready to scale.',
    features: [
      'Up to 10 doctors',
      'Unlimited patients',
      'Full clinical workflows',
      'Reports & KPIs',
      'Priority support',
      'Notifications (email, SMS-ready)',
    ],
    cta: 'Start trial',
    featured: true,
  },
  {
    name: 'Enterprise',
    price: 'Custom',
    tagline: 'For clinic groups with advanced needs.',
    features: [
      'Unlimited doctors & sites',
      'Custom SLAs & DPA',
      'SSO & audit exports',
      'Dedicated success manager',
      'Custom integrations',
    ],
    cta: 'Talk to sales',
    featured: false,
  },
];

function Pricing() {
  return (
    <section id="pricing" className="scroll-mt-24 border-y border-gray-100 dark:border-gray-800 bg-gray-50/60 dark:bg-gray-900/40">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-20 sm:py-28">
        <div className="text-center max-w-2xl mx-auto">
          <p className="text-caption uppercase tracking-widest text-brand-600 dark:text-brand-400 font-semibold">
            Simple, honest pricing
          </p>
          <h2 className="mt-2 text-3xl sm:text-4xl font-semibold tracking-tight text-gray-900 dark:text-white">
            Pay for the size of your clinic — not per feature.
          </h2>
          <p className="mt-3 text-body-lg text-gray-600 dark:text-gray-400">
            Every plan includes core clinical workflows. Cancel any time. No setup fee.
          </p>
        </div>

        <div className="mt-12 grid grid-cols-1 md:grid-cols-3 gap-6">
          {plans.map((p) => (
            <div
              key={p.name}
              className={`relative rounded-2xl border p-6 flex flex-col ${
                p.featured
                  ? 'border-brand-500 bg-white dark:bg-gray-900 shadow-theme-lg ring-1 ring-brand-500/20'
                  : 'border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900'
              }`}
            >
              {p.featured && (
                <span className="absolute -top-3 left-1/2 -translate-x-1/2 inline-flex items-center gap-1 rounded-full bg-brand-600 text-white text-caption font-medium px-2.5 py-0.5 shadow-theme-sm">
                  Most popular
                </span>
              )}
              <div>
                <p className="text-body font-semibold text-gray-900 dark:text-white">{p.name}</p>
                <p className="mt-1 text-caption text-gray-500 dark:text-gray-400">{p.tagline}</p>
                <div className="mt-5 flex items-baseline gap-1">
                  <span className="text-4xl font-semibold tracking-tight text-gray-900 dark:text-white tabular-nums">
                    {p.price}
                  </span>
                  {p.price !== 'Custom' && (
                    <span className="text-caption text-gray-500 dark:text-gray-400">/ clinic / month</span>
                  )}
                </div>
              </div>
              <ul className="mt-6 space-y-3 flex-1">
                {p.features.map((f) => (
                  <li key={f} className="flex items-start gap-2 text-body text-gray-700 dark:text-gray-300">
                    <CheckIcon className="mt-0.5 h-4 w-4 text-brand-600 shrink-0" strokeWidth={2.5} />
                    <span>{f}</span>
                  </li>
                ))}
              </ul>
              <Link
                to={p.name === 'Enterprise' ? '/register' : '/register'}
                className={`mt-8 inline-flex justify-center items-center h-11 rounded-lg text-body font-medium transition-colors ${
                  p.featured
                    ? 'bg-brand-600 text-white hover:bg-brand-700 shadow-theme-xs'
                    : 'bg-white dark:bg-gray-900 text-gray-800 dark:text-gray-100 border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800'
                }`}
              >
                {p.cta}
              </Link>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

/* ─────────────────────────── FAQ ─────────────────────────── */

const faqs = [
  {
    q: 'How long does it take to get set up?',
    a: 'Most clinics are live the same day. Import your patient list from CSV, add your doctors and working hours, and you can start booking appointments in under an hour.',
  },
  {
    q: 'Is my clinic\'s data isolated from other clinics?',
    a: 'Yes. Every tenant has its own logical scope enforced at the database level. Writes are stamped and filtered by tenant on every query — a single, tested enforcement point.',
  },
  {
    q: 'Can patients book their own appointments?',
    a: 'Yes. The patient portal lets patients register, book, reschedule, view prescriptions, and pay invoices — reducing front-desk load without adding another tool.',
  },
  {
    q: 'Do you support prescriptions?',
    a: 'Yes. Doctors can capture consultation notes and prescriptions from reusable templates. Patients receive a copy in their portal immediately.',
  },
  {
    q: 'What happens if I cancel?',
    a: 'You can export your full dataset any time. Cancel with one click from your workspace settings — no phone call required.',
  },
];

function FAQ() {
  const [openIdx, setOpenIdx] = useState<number | null>(0);
  return (
    <section id="faq" className="scroll-mt-24">
      <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8 py-20 sm:py-24">
        <div className="text-center">
          <p className="text-caption uppercase tracking-widest text-brand-600 dark:text-brand-400 font-semibold">
            Frequently asked
          </p>
          <h2 className="mt-2 text-3xl sm:text-4xl font-semibold tracking-tight text-gray-900 dark:text-white">
            Answers, before you ask.
          </h2>
        </div>
        <div className="mt-10 divide-y divide-gray-100 dark:divide-gray-800 border-y border-gray-100 dark:border-gray-800">
          {faqs.map((f, i) => {
            const open = openIdx === i;
            return (
              <div key={f.q}>
                <button
                  type="button"
                  onClick={() => setOpenIdx(open ? null : i)}
                  className="w-full flex items-center justify-between gap-6 py-5 text-left"
                  aria-expanded={open}
                >
                  <span className="text-body-lg font-medium text-gray-900 dark:text-white">{f.q}</span>
                  <span className="inline-flex h-8 w-8 shrink-0 items-center justify-center rounded-full border border-gray-200 dark:border-gray-700 text-gray-500 dark:text-gray-400">
                    {open ? <MinusIcon className="h-4 w-4" /> : <PlusIcon className="h-4 w-4" />}
                  </span>
                </button>
                {open && (
                  <p className="pb-6 -mt-1 text-body text-gray-600 dark:text-gray-400 leading-relaxed max-w-3xl">
                    {f.a}
                  </p>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </section>
  );
}

/* ─────────────────────────── Final CTA ─────────────────────────── */

function FinalCTA() {
  return (
    <section className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 pb-20">
      <div className="relative overflow-hidden rounded-3xl bg-brand-950 text-white p-10 sm:p-14 lg:p-16">
        <div aria-hidden="true" className="pointer-events-none absolute inset-0 opacity-70">
          <div className="absolute -top-24 -right-16 h-96 w-96 rounded-full bg-brand-500 blur-3xl opacity-40" />
          <div className="absolute -bottom-24 -left-24 h-96 w-96 rounded-full bg-teal-500 blur-3xl opacity-25" />
        </div>
        <div className="relative flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6">
          <div className="max-w-xl">
            <h2 className="text-3xl sm:text-4xl font-semibold tracking-tight">Ready to modernize your clinic?</h2>
            <p className="mt-3 text-body-lg text-brand-100/80">
              Start free for 14 days. No credit card. Cancel any time.
            </p>
          </div>
          <div className="flex flex-col sm:flex-row gap-3 shrink-0">
            <Link
              to="/register"
              className="inline-flex justify-center items-center gap-1.5 h-12 px-6 rounded-lg bg-white text-brand-700 text-body-lg font-medium hover:bg-brand-50 transition-colors"
            >
              Start free trial
              <ArrowRightIcon className="h-4 w-4" />
            </Link>
            <Link
              to="/sign-in"
              className="inline-flex justify-center items-center h-12 px-6 rounded-lg border border-white/20 text-white text-body-lg font-medium hover:bg-white/10 transition-colors"
            >
              Sign in
            </Link>
          </div>
        </div>
      </div>
    </section>
  );
}

/* ─────────────────────────── Footer ─────────────────────────── */

function MarketingFooter() {
  const year = new Date().getFullYear();
  return (
    <footer className="border-t border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-950">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-12 grid grid-cols-2 md:grid-cols-4 gap-8">
        <div className="col-span-2 md:col-span-1">
          <div className="flex items-center gap-2.5">
            <BrandMark />
            <div className="leading-tight">
              <p className="text-caption uppercase tracking-widest text-brand-600 dark:text-brand-400 font-semibold">
                Arrapa
              </p>
              <p className="text-body font-semibold text-gray-900 dark:text-white">ClinicOS</p>
            </div>
          </div>
          <p className="mt-4 text-caption text-gray-500 dark:text-gray-400 max-w-xs">
            The complete operating system for modern clinics.
          </p>
        </div>
        <FooterColumn
          title="Product"
          links={[
            ['Features', '#features'],
            ['How it works', '#showcase'],
            ['Pricing', '#pricing'],
            ['FAQ', '#faq'],
          ]}
        />
        <FooterColumn
          title="Company"
          links={[
            ['About', '#'],
            ['Contact', '#'],
            ['Careers', '#'],
          ]}
        />
        <FooterColumn
          title="Legal"
          links={[
            ['Privacy', '#'],
            ['Terms', '#'],
            ['Security', '#'],
          ]}
        />
      </div>
      <div className="border-t border-gray-100 dark:border-gray-800">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-5 flex flex-col sm:flex-row items-center justify-between gap-3 text-caption text-gray-500 dark:text-gray-400">
          <span>© {year} Arrapa Software. All rights reserved.</span>
          <span className="inline-flex items-center gap-1.5">
            <ShieldCheckIcon className="h-3.5 w-3.5" /> Tenant-isolated · Auditable · Encrypted
          </span>
        </div>
      </div>
    </footer>
  );
}

function FooterColumn({ title, links }: { title: string; links: [string, string][] }) {
  return (
    <div>
      <p className="text-caption uppercase tracking-widest text-gray-500 dark:text-gray-400 font-semibold">
        {title}
      </p>
      <ul className="mt-3 space-y-2">
        {links.map(([label, href]) => (
          <li key={label}>
            <a
              href={href}
              className="text-body text-gray-700 dark:text-gray-300 hover:text-brand-600 dark:hover:text-brand-400 transition-colors"
            >
              {label}
            </a>
          </li>
        ))}
      </ul>
    </div>
  );
}

/* ─────────────────────────── Brand mark ─────────────────────────── */

function BrandMark() {
  return (
    <span className="inline-flex h-8 w-8 items-center justify-center rounded-lg bg-brand-600 text-white shadow-theme-xs">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} className="h-4 w-4">
        <path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" />
      </svg>
    </span>
  );
}
