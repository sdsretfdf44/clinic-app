import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { Link } from 'react-router-dom';
import { ArrowLeftIcon, BuildingOffice2Icon, EnvelopeIcon } from '@heroicons/react/24/outline';

import { humanizeError } from '@/api/httpClient';
import { Alert, Button, FormField, Input } from '@/components/ui';
import { useForgotPassword } from '@/features/auth/hooks';
import { forgotPasswordSchema, type ForgotPasswordFormValues } from '@/features/auth/schemas';
import { AuthLayout } from '@/layouts/AuthLayout';

export default function ForgotPasswordPage() {
  const forgotPassword = useForgotPassword();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: { email: '', tenantSlug: 'arrapa' },
  });

  const onSubmit = handleSubmit((values) => {
    forgotPassword.mutate({
      email: values.email,
      tenantSlug: values.tenantSlug.toLowerCase(),
    });
  });

  return (
    <AuthLayout
      title="Forgot your password?"
      subtitle="No problem — enter your workspace and email, and we'll send you a reset link."
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
      <form onSubmit={onSubmit} className="space-y-5" noValidate>
        {forgotPassword.isSuccess && (
          <Alert variant="success" title="Check your inbox">
            If an account exists for that email, a reset link has been sent.
          </Alert>
        )}

        {forgotPassword.isError && (
          <Alert variant="error">{humanizeError(forgotPassword.error)}</Alert>
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

        <Button type="submit" size="lg" fullWidth isLoading={forgotPassword.isPending}>
          Send reset link
        </Button>
      </form>
    </AuthLayout>
  );
}
