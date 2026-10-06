import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { ArrowLeftIcon, LockClosedIcon } from '@heroicons/react/24/outline';

import { humanizeError } from '@/api/httpClient';
import { Alert, Button, FormField, Input } from '@/components/ui';
import { useResetPassword } from '@/features/auth/hooks';
import { resetPasswordSchema, type ResetPasswordFormValues } from '@/features/auth/schemas';
import { AuthLayout } from '@/layouts/AuthLayout';

export default function ResetPasswordPage() {
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const resetPassword = useResetPassword();

  const email = params.get('email') ?? '';
  const token = params.get('token') ?? '';
  const tenantSlug = params.get('tenant') ?? '';

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { email, token, tenantSlug, newPassword: '', confirmPassword: '' },
  });

  useEffect(() => {
    reset({ email, token, tenantSlug, newPassword: '', confirmPassword: '' });
  }, [email, token, tenantSlug, reset]);

  useEffect(() => {
    if (resetPassword.isSuccess) {
      const timer = window.setTimeout(() => navigate('/sign-in', { replace: true }), 2000);
      return () => window.clearTimeout(timer);
    }
    return undefined;
  }, [resetPassword.isSuccess, navigate]);

  const onSubmit = handleSubmit((values) => {
    resetPassword.mutate({
      email: values.email,
      token: values.token,
      newPassword: values.newPassword,
      tenantSlug: values.tenantSlug,
    });
  });

  const hasRequiredParams = !!email && !!token && !!tenantSlug;

  return (
    <AuthLayout
      title="Reset your password"
      subtitle={hasRequiredParams ? 'Choose a new, strong password to secure your account.' : undefined}
      footer={
        <Link
          to="/sign-in"
          className="inline-flex items-center gap-1.5 font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400 dark:hover:text-brand-300 transition-colors"
        >
          <ArrowLeftIcon className="h-4 w-4" />
          Return to sign in
        </Link>
      }
    >
      {!hasRequiredParams ? (
        <Alert variant="error" title="Invalid or missing link">
          The reset link is missing required parameters. Request a new one from the{' '}
          <Link to="/forgot-password" className="u-link underline underline-offset-2">
            forgot password
          </Link>{' '}
          page.
        </Alert>
      ) : (
        <form onSubmit={onSubmit} className="space-y-5" noValidate>
          {resetPassword.isSuccess && (
            <Alert variant="success" title="Password reset">
              Your password has been reset. Redirecting to sign in…
            </Alert>
          )}
          {resetPassword.isError && (
            <Alert variant="error">{humanizeError(resetPassword.error)}</Alert>
          )}

          <input type="hidden" {...register('email')} />
          <input type="hidden" {...register('token')} />
          <input type="hidden" {...register('tenantSlug')} />

          <FormField
            id="newPassword"
            label="New password"
            required
            hint="At least 10 characters, mix of upper, lower, digit, and symbol."
            error={errors.newPassword?.message}
          >
            <Input
              id="newPassword"
              type="password"
              autoComplete="new-password"
              leftIcon={<LockClosedIcon />}
              allowRevealPassword
              invalid={!!errors.newPassword}
              {...register('newPassword')}
            />
          </FormField>

          <FormField id="confirmPassword" label="Confirm new password" required error={errors.confirmPassword?.message}>
            <Input
              id="confirmPassword"
              type="password"
              autoComplete="new-password"
              leftIcon={<LockClosedIcon />}
              allowRevealPassword
              invalid={!!errors.confirmPassword}
              {...register('confirmPassword')}
            />
          </FormField>

          <Button type="submit" size="lg" fullWidth isLoading={resetPassword.isPending}>
            Reset password
          </Button>
        </form>
      )}
    </AuthLayout>
  );
}
