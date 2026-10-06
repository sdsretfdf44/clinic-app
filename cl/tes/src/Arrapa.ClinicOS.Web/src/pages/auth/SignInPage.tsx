import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import {
  BuildingOffice2Icon,
  EnvelopeIcon,
  KeyIcon,
  LockClosedIcon,
} from '@heroicons/react/24/outline';

import { extractApiError, humanizeError } from '@/api/httpClient';
import { Alert, Button, FormField, Input } from '@/components/ui';
import { useSignIn } from '@/features/auth/hooks';
import { signInSchema, type SignInFormValues } from '@/features/auth/schemas';
import { AuthLayout } from '@/layouts/AuthLayout';
import { useAuthStore } from '@/store/authStore';

interface LocationState {
  from?: string;
}

export default function SignInPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const signIn = useSignIn();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<SignInFormValues>({
    resolver: zodResolver(signInSchema),
    defaultValues: { email: '', password: '', tenantSlug: 'arrapa' },
  });

  const [twoFactorCode, setTwoFactorCode] = useState('');

  useEffect(() => {
    if (isAuthenticated) {
      const state = location.state as LocationState | null;
      navigate(state?.from ?? '/dashboard', { replace: true });
    }
  }, [isAuthenticated, location.state, navigate]);

  const apiError = signIn.error ? extractApiError(signIn.error) : null;
  const twoFactorRequired = useMemo(() => apiError?.code === 'auth.2fa.required', [apiError]);
  const twoFactorInvalid = useMemo(() => apiError?.code === 'auth.2fa.code_invalid', [apiError]);
  const errorMessage = apiError?.message ?? (signIn.error ? humanizeError(signIn.error) : null);
  const showTwoFactorField = twoFactorRequired || twoFactorInvalid;

  const onSubmit = handleSubmit((values) => {
    signIn.mutate({
      email: values.email,
      password: values.password,
      tenantSlug: values.tenantSlug.toLowerCase(),
      twoFactorCode: showTwoFactorField ? twoFactorCode.replace(/\D/g, '') : undefined,
    });
  });

  return (
    <AuthLayout
      title="Welcome back"
      subtitle="Sign in to your ClinicOS workspace to continue."
      footer={
        <div className="text-center">
          Don&apos;t have an account?{' '}
          <Link to="/register" className="font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400">
            Create one
          </Link>
        </div>
      }
    >
      <form onSubmit={onSubmit} className="space-y-5" noValidate>
        {errorMessage && !twoFactorRequired && (
          <Alert variant="error" title="Unable to sign in">
            {errorMessage}
          </Alert>
        )}

        {twoFactorRequired && !twoFactorInvalid && (
          <Alert variant="info" title="Two-factor code required">
            Open your authenticator app and enter the 6-digit code to continue.
          </Alert>
        )}

        <FormField id="tenantSlug" label="Tenant workspace" required error={errors.tenantSlug?.message}>
          <Input
            id="tenantSlug"
            autoComplete="organization"
            placeholder="acme"
            leftIcon={<BuildingOffice2Icon />}
            invalid={!!errors.tenantSlug}
            {...register('tenantSlug')}
          />
        </FormField>

        <FormField id="email" label="Email" required error={errors.email?.message}>
          <Input
            id="email"
            type="email"
            autoComplete="email"
            placeholder="you@clinic.com"
            leftIcon={<EnvelopeIcon />}
            invalid={!!errors.email}
            {...register('email')}
          />
        </FormField>

        <FormField id="password" label="Password" required error={errors.password?.message}>
          <Input
            id="password"
            type="password"
            autoComplete="current-password"
            placeholder="••••••••"
            leftIcon={<LockClosedIcon />}
            allowRevealPassword
            invalid={!!errors.password}
            {...register('password')}
          />
        </FormField>

        {showTwoFactorField && (
          <FormField
            id="twoFactorCode"
            label="Authenticator code"
            required
            error={twoFactorInvalid ? errorMessage ?? undefined : undefined}
            hint="6-digit code from your authenticator app"
          >
            <Input
              id="twoFactorCode"
              inputMode="numeric"
              autoComplete="one-time-code"
              placeholder="123 456"
              leftIcon={<KeyIcon />}
              invalid={twoFactorInvalid}
              value={twoFactorCode}
              onChange={(event) => setTwoFactorCode(event.target.value)}
            />
          </FormField>
        )}

        <div className="flex items-center justify-end">
          <Link to="/forgot-password" className="text-body font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300 transition-colors">
            Forgot password?
          </Link>
        </div>

        <Button type="submit" size="lg" fullWidth isLoading={signIn.isPending}>
          {showTwoFactorField ? 'Verify and sign in' : 'Sign in'}
        </Button>

        <p className="text-caption text-gray-400 text-center pt-2">
          Protected by 256-bit encryption. Your data is safe with us.
        </p>
      </form>
    </AuthLayout>
  );
}
