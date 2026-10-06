import { Component } from 'react';
import type { ErrorInfo, ReactNode } from 'react';

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: (props: { error: Error; reset: () => void }) => ReactNode;
}

interface ErrorBoundaryState {
  error: Error | null;
}

/**
 * Catches render-time errors in the tree beneath it and shows a recoverable fallback UI.
 * Wrap the whole app once at the root, and again around risky isolated widgets if you want
 * a page to keep functioning when a sub-tree crashes.
 */
export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  state: ErrorBoundaryState = { error: null };

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo): void {
    // eslint-disable-next-line no-console
    console.error('[ErrorBoundary] Unhandled render error:', error, info);
  }

  reset = () => this.setState({ error: null });

  render() {
    if (!this.state.error) {
      return this.props.children;
    }

    if (this.props.fallback) {
      return this.props.fallback({ error: this.state.error, reset: this.reset });
    }

    return <DefaultErrorFallback error={this.state.error} onReset={this.reset} />;
  }
}

function DefaultErrorFallback({ error, onReset }: { error: Error; onReset: () => void }) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-950 px-4">
      <div className="max-w-lg w-full rounded-2xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-theme-lg p-8 text-center">
        <div className="mx-auto h-12 w-12 rounded-full bg-danger-100 dark:bg-danger-500/10 flex items-center justify-center mb-4">
          <svg viewBox="0 0 20 20" fill="currentColor" className="h-6 w-6 text-danger-600" aria-hidden="true">
            <path
              fillRule="evenodd"
              clipRule="evenodd"
              d="M9.401 2.6a.75.75 0 011.198 0l7.5 12.999A.75.75 0 0117.5 16.75H2.5a.75.75 0 01-.649-1.151l7.55-12.999zm.599 5.65a.75.75 0 00-.75.75v3a.75.75 0 001.5 0v-3a.75.75 0 00-.75-.75zm0 6.5a1 1 0 100-2 1 1 0 000 2z"
            />
          </svg>
        </div>
        <h1 className="text-display-md font-semibold text-gray-900 dark:text-gray-100 mb-2">Something went wrong</h1>
        <p className="text-body text-gray-600 dark:text-gray-400 mb-6">
          The page hit an unexpected error. You can try again — if it keeps happening, refresh the app or contact support.
        </p>
        {import.meta.env.DEV && (
          <pre className="text-caption text-left mb-6 overflow-auto max-h-48 rounded-lg bg-gray-100 dark:bg-gray-950 p-3 text-danger-600 dark:text-danger-400">
            {error.message}
          </pre>
        )}
        <div className="flex gap-3 justify-center">
          <button
            type="button"
            onClick={onReset}
            className="inline-flex items-center justify-center h-10 px-4 rounded-lg bg-brand-600 text-white text-body font-medium hover:bg-brand-700"
          >
            Try again
          </button>
          <button
            type="button"
            onClick={() => window.location.assign('/')}
            className="inline-flex items-center justify-center h-10 px-4 rounded-lg bg-white text-gray-700 border border-gray-200 text-body font-medium hover:bg-gray-50 dark:bg-gray-900 dark:text-gray-100 dark:border-gray-700"
          >
            Go home
          </button>
        </div>
      </div>
    </div>
  );
}
