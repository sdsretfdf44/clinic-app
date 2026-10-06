import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { CheckCircleIcon, EnvelopeIcon, ExclamationTriangleIcon } from '@heroicons/react/24/outline';

import { humanizeError } from '@/api/httpClient';
import { Alert, Button, Spinner } from '@/components/ui';
import { useResendVerificationEmail, useVerifyEmail } from '@/features/auth/hooks';
import { AuthLayout } from '@/layouts/AuthLayout';

export default function VerifyEmailPage() {
  const [params] = useSearchParams();
  const email = params.get('email') ?? '';
  const token = params.get('token') ?? '';
  const tenantSlug = params.get('tenant') ?? '';

  const verify = useVerifyEmail();
  const resend = useResendVerificationEmail();
  const [attempted, setAttempted] = useState(false);

  useEffect(() => {
    if (email && token && tenantSlug && !attempted) {
      setAttempted(true);
      verify.mutate({ email, token, tenantSlug });
    }
  }, [email, token, tenantSlug, attempted, verify]);

  const missing = !email || !token || !tenantSlug;

  return (
    <AuthLayout
      title="Verify your email"
      subtitle="This confirms that the address is yours so we can safely deliver clinic communications."
      footer={
        <Link to="/sign-in" className="font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400">
          Return to sign in
        </Link>
      }
    >
      <div className="space-y-5">
        {missing ? (
          <Alert variant="warning" title="This link is incomplete">
            Please open the verification link exactly as it was sent in your email.
          </Alert>
        ) : verify.isPending ? (
          <div className="flex flex-col items-center gap-3 py-6">
            <Spinner className="h-8 w-8" />
            <p className="text-body text-gray-600 dark:text-gray-300">Verifying your email…</p>
          </div>
        ) : verify.isSuccess ? (
          <div className="flex flex-col items-center gap-3 py-6 text-center">
            <div className="h-12 w-12 rounded-full bg-success-100 dark:bg-success-500/10 flex items-center justify-center">
              <CheckCircleIcon className="h-7 w-7 text-success-600" />
            </div>
            <p className="text-display-sm font-semibold text-gray-900 dark:text-gray-100">You&apos;re all set</p>
            <p className="text-body text-gray-600 dark:text-gray-300">Your email is verified. Sign in to continue.</p>
            <Link to="/sign-in" className="pt-2">
              <Button size="lg">Sign in</Button>
            </Link>
          </div>
        ) : verify.isError ? (
          <div className="flex flex-col items-center gap-3 py-6 text-center">
            <div className="h-12 w-12 rounded-full bg-danger-100 dark:bg-danger-500/10 flex items-center justify-center">
              <ExclamationTriangleIcon className="h-7 w-7 text-danger-600" />
            </div>
            <p className="text-display-sm font-semibold text-gray-900 dark:text-gray-100">This link isn&apos;t working</p>
            <p className="text-body text-gray-600 dark:text-gray-300">{humanizeError(verify.error)}</p>
            <Button
              size="lg"
              leftIcon={<EnvelopeIcon className="h-4 w-4" />}
              isLoading={resend.isPending}
              onClick={() => resend.mutate({ email, tenantSlug })}
              disabled={!email || !tenantSlug}
            >
              Send a new verification email
            </Button>
            {resend.isSuccess && (
              <Alert variant="success" className="mt-3">
                If the account exists, a new verification link has been sent.
              </Alert>
            )}
          </div>
        ) : null}
      </div>
    </AuthLayout>
  );
}
