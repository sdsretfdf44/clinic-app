import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { Link } from 'react-router-dom';
import {
  BuildingOffice2Icon,
  EnvelopeIcon,
  LockClosedIcon,
  PhoneIcon,
  UserIcon,
} from '@heroicons/react/24/outline';

import { humanizeError } from '@/api/httpClient';
import { Alert, Button, FormField, Input } from '@/components/ui';
import { useRegisterPatient } from '@/features/auth/hooks';
import { registerPatientSchema, type RegisterPatientFormValues } from '@/features/auth/schemas';
import { AuthLayout } from '@/layouts/AuthLayout';

export default function RegisterPage() {
  const register = useRegisterPatient();

  const {
    register: registerField,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<RegisterPatientFormValues>({
    resolver: zodResolver(registerPatientSchema),
    defaultValues: {
      tenantSlug: 'arrapa',
      firstName: '',
      lastName: '',
      email: '',
      phone: '',
      password: '',
      confirmPassword: '',
      dateOfBirth: '',
      acceptTerms: false as unknown as true,
    },
  });

  const onSubmit = handleSubmit((values) => {
    register.mutate(
      {
        tenantSlug: values.tenantSlug.toLowerCase(),
        firstName: values.firstName.trim(),
        lastName: values.lastName.trim(),
        email: values.email.trim(),
        phone: values.phone.trim(),
        password: values.password,
        dateOfBirth: values.dateOfBirth || undefined,
      },
      { onSuccess: () => reset() },
    );
  });

  return (
    <AuthLayout
      title="Create your patient account"
      subtitle="Book appointments, view prescriptions, and get reminders — all in one place."
      footer={
        <div className="text-center">
          Already have an account?{' '}
          <Link to="/sign-in" className="font-medium text-brand-600 hover:text-brand-700 dark:text-brand-400">
            Sign in
          </Link>
        </div>
      }
    >
      <form onSubmit={onSubmit} className="space-y-5" noValidate>
        {register.isSuccess && (
          <Alert variant="success" title="Check your inbox">
            Registration successful. We&apos;ve emailed you a link to verify your address.
          </Alert>
        )}

        {register.isError && <Alert variant="error">{humanizeError(register.error)}</Alert>}

        <FormField id="tenantSlug" label="Clinic workspace" required error={errors.tenantSlug?.message}>
          <Input
            id="tenantSlug"
            autoComplete="organization"
            placeholder="acme"
            leftIcon={<BuildingOffice2Icon />}
            invalid={!!errors.tenantSlug}
            {...registerField('tenantSlug')}
          />
        </FormField>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="firstName" label="First name" required error={errors.firstName?.message}>
            <Input
              id="firstName"
              autoComplete="given-name"
              placeholder="Jane"
              leftIcon={<UserIcon />}
              invalid={!!errors.firstName}
              {...registerField('firstName')}
            />
          </FormField>

          <FormField id="lastName" label="Last name" required error={errors.lastName?.message}>
            <Input
              id="lastName"
              autoComplete="family-name"
              placeholder="Doe"
              leftIcon={<UserIcon />}
              invalid={!!errors.lastName}
              {...registerField('lastName')}
            />
          </FormField>
        </div>

        <FormField id="email" label="Email" required error={errors.email?.message}>
          <Input
            id="email"
            type="email"
            autoComplete="email"
            placeholder="you@example.com"
            leftIcon={<EnvelopeIcon />}
            invalid={!!errors.email}
            {...registerField('email')}
          />
        </FormField>

        <FormField id="phone" label="Phone" required error={errors.phone?.message}>
          <Input
            id="phone"
            type="tel"
            autoComplete="tel"
            placeholder="+1 (555) 000-0000"
            leftIcon={<PhoneIcon />}
            invalid={!!errors.phone}
            {...registerField('phone')}
          />
        </FormField>

        <FormField id="dateOfBirth" label="Date of birth" hint="Optional — helps us match your medical record." error={errors.dateOfBirth?.message}>
          <Input id="dateOfBirth" type="date" invalid={!!errors.dateOfBirth} {...registerField('dateOfBirth')} />
        </FormField>

        <FormField id="password" label="Password" required error={errors.password?.message}>
          <Input
            id="password"
            type="password"
            autoComplete="new-password"
            placeholder="At least 10 characters"
            leftIcon={<LockClosedIcon />}
            allowRevealPassword
            invalid={!!errors.password}
            {...registerField('password')}
          />
        </FormField>

        <FormField id="confirmPassword" label="Confirm password" required error={errors.confirmPassword?.message}>
          <Input
            id="confirmPassword"
            type="password"
            autoComplete="new-password"
            placeholder="Re-enter your password"
            leftIcon={<LockClosedIcon />}
            allowRevealPassword
            invalid={!!errors.confirmPassword}
            {...registerField('confirmPassword')}
          />
        </FormField>

        <label className="flex items-start gap-3 text-body text-gray-600 dark:text-gray-300">
          <input
            type="checkbox"
            className="mt-1 h-4 w-4 rounded border-gray-300 dark:border-gray-700 text-brand-600 focus:ring-brand-500"
            {...registerField('acceptTerms')}
          />
          <span>
            I agree to the <span className="text-gray-800 dark:text-gray-100 font-medium">Terms of Service</span> and{' '}
            <span className="text-gray-800 dark:text-gray-100 font-medium">Privacy Policy</span>.
          </span>
        </label>
        {errors.acceptTerms && <p className="text-caption text-danger-600 -mt-3">{errors.acceptTerms.message}</p>}

        <Button type="submit" size="lg" fullWidth isLoading={register.isPending}>
          Create account
        </Button>
      </form>
    </AuthLayout>
  );
}
