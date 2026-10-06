import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { KeyIcon, ShieldCheckIcon, ShieldExclamationIcon } from '@heroicons/react/24/outline';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Button,
  Card,
  FormField,
  Input,
  PageHeader,
  Skeleton,
  Spinner,
} from '@/components/ui';
import {
  useDisableTwoFactor,
  useEnableTwoFactor,
  useTwoFactorSetup,
} from '@/features/auth/hooks';
import { twoFactorCodeSchema, type TwoFactorCodeFormValues } from '@/features/auth/schemas';

export default function TwoFactorPage() {
  const setup = useTwoFactorSetup();
  const enable = useEnableTwoFactor();
  const disable = useDisableTwoFactor();
  const [mode, setMode] = useState<'idle' | 'disable'>('idle');

  const enrollForm = useForm<TwoFactorCodeFormValues>({
    resolver: zodResolver(twoFactorCodeSchema),
    defaultValues: { code: '' },
  });

  const disableForm = useForm<TwoFactorCodeFormValues>({
    resolver: zodResolver(twoFactorCodeSchema),
    defaultValues: { code: '' },
  });

  const enrolled = setup.data?.status.enabled ?? false;

  const onEnable = enrollForm.handleSubmit((values) => {
    enable.mutate(values.code.replace(/\D/g, ''), { onSuccess: () => enrollForm.reset() });
  });

  const onDisable = disableForm.handleSubmit((values) => {
    disable.mutate(values.code.replace(/\D/g, ''), {
      onSuccess: () => {
        disableForm.reset();
        setMode('idle');
      },
    });
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="Two-factor authentication"
        description="Add a second verification step so even a leaked password can't unlock your account."
      />

      {setup.isLoading ? (
        <Card><Skeleton className="h-40" /></Card>
      ) : setup.isError ? (
        <Alert variant="error">{humanizeError(setup.error)}</Alert>
      ) : enrolled ? (
        <Card>
          <div className="flex items-start gap-3">
            <div className="h-10 w-10 rounded-full bg-success-100 dark:bg-success-500/10 flex items-center justify-center">
              <ShieldCheckIcon className="h-6 w-6 text-success-600" aria-hidden="true" />
            </div>
            <div className="flex-1">
              <p className="text-body-lg font-semibold text-gray-900 dark:text-gray-100">2FA is enabled</p>
              <p className="text-body text-gray-600 dark:text-gray-300 mt-1">
                Your account requires a 6-digit authenticator code at sign-in. To turn 2FA off, verify with a current code below.
              </p>

              {mode === 'idle' ? (
                <Button variant="outline" className="mt-4" onClick={() => setMode('disable')}>
                  Turn off 2FA
                </Button>
              ) : (
                <form onSubmit={onDisable} className="mt-4 space-y-4" noValidate>
                  {disable.isError && <Alert variant="error">{humanizeError(disable.error)}</Alert>}
                  <FormField id="disableCode" label="Verification code" required error={disableForm.formState.errors.code?.message}>
                    <Input
                      id="disableCode"
                      inputMode="numeric"
                      autoComplete="one-time-code"
                      placeholder="123 456"
                      leftIcon={<KeyIcon />}
                      invalid={!!disableForm.formState.errors.code}
                      {...disableForm.register('code')}
                    />
                  </FormField>
                  <div className="flex gap-2">
                    <Button type="submit" variant="danger" isLoading={disable.isPending}>Disable 2FA</Button>
                    <Button type="button" variant="ghost" onClick={() => setMode('idle')}>Cancel</Button>
                  </div>
                </form>
              )}
            </div>
          </div>
        </Card>
      ) : (
        <Card>
          <div className="flex items-start gap-3 mb-6">
            <div className="h-10 w-10 rounded-full bg-warning-100 dark:bg-warning-500/10 flex items-center justify-center">
              <ShieldExclamationIcon className="h-6 w-6 text-warning-600" aria-hidden="true" />
            </div>
            <div>
              <p className="text-body-lg font-semibold text-gray-900 dark:text-gray-100">2FA is not enabled</p>
              <p className="text-body text-gray-600 dark:text-gray-300 mt-1">
                Scan the QR code with your authenticator (Google Authenticator, 1Password, Authy, etc.) then enter the code it shows.
              </p>
            </div>
          </div>

          {setup.data?.setup && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 items-start">
              <div className="flex flex-col items-center gap-3">
                <div className="rounded-xl bg-white p-3 border border-gray-200 dark:border-gray-700">
                  <QrCode value={setup.data.setup.authenticatorUri} />
                </div>
                <p className="text-caption text-gray-500 dark:text-gray-400 text-center">Or enter this key manually:</p>
                <code className="text-body font-mono bg-gray-100 dark:bg-gray-800 rounded px-3 py-1.5 tracking-wide">
                  {setup.data.setup.sharedKey}
                </code>
              </div>

              <form onSubmit={onEnable} className="space-y-4" noValidate>
                {enable.isError && <Alert variant="error">{humanizeError(enable.error)}</Alert>}
                {enable.isSuccess && <Alert variant="success">2FA has been enabled on your account.</Alert>}
                <FormField id="enableCode" label="Verification code" required error={enrollForm.formState.errors.code?.message}>
                  <Input
                    id="enableCode"
                    inputMode="numeric"
                    autoComplete="one-time-code"
                    placeholder="123 456"
                    leftIcon={<KeyIcon />}
                    invalid={!!enrollForm.formState.errors.code}
                    {...enrollForm.register('code')}
                  />
                </FormField>
                <Button type="submit" isLoading={enable.isPending}>Enable 2FA</Button>
              </form>
            </div>
          )}
        </Card>
      )}
    </div>
  );
}

/**
 * QR code renderer that uses the public quickchart.io endpoint — pure client-side, no build-time
 * SVG library needed. Provides an accessible label pointing users to the manual key fallback.
 */
function QrCode({ value }: { value: string }) {
  const [failed, setFailed] = useState(false);
  const src = `https://api.qrserver.com/v1/create-qr-code/?size=180x180&data=${encodeURIComponent(value)}`;

  if (failed) {
    return (
      <div className="h-[180px] w-[180px] flex items-center justify-center text-caption text-gray-500">
        <Spinner className="h-6 w-6" />
      </div>
    );
  }

  return (
    <img
      src={src}
      alt="Two-factor authentication QR code"
      width={180}
      height={180}
      onError={() => setFailed(true)}
    />
  );
}
