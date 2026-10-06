import { z } from 'zod';

export const passwordStrength = z
  .string()
  .min(10, 'Password must be at least 10 characters.')
  .max(128, 'Password is too long.')
  .regex(/[A-Z]/, 'Must include an uppercase letter.')
  .regex(/[a-z]/, 'Must include a lowercase letter.')
  .regex(/[0-9]/, 'Must include a digit.')
  .regex(/[^A-Za-z0-9]/, 'Must include a symbol.');

export const signInSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Enter a valid email.'),
  password: z.string().min(1, 'Password is required.'),
  tenantSlug: z
    .string()
    .min(1, 'Tenant is required.')
    .max(64)
    .regex(/^[a-z0-9-]+$/i, 'Tenant may only contain letters, digits, and hyphens.'),
  rememberMe: z.boolean().optional(),
});

export type SignInFormValues = z.infer<typeof signInSchema>;

export const forgotPasswordSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Enter a valid email.'),
  tenantSlug: z
    .string()
    .min(1, 'Tenant is required.')
    .max(64)
    .regex(/^[a-z0-9-]+$/i, 'Tenant may only contain letters, digits, and hyphens.'),
});

export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>;

export const resetPasswordSchema = z
  .object({
    email: z.string().email(),
    token: z.string().min(1),
    tenantSlug: z.string().min(1),
    newPassword: passwordStrength,
    confirmPassword: z.string(),
  })
  .refine((v) => v.newPassword === v.confirmPassword, {
    path: ['confirmPassword'],
    message: 'Passwords do not match.',
  });

export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>;

export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Current password is required.'),
    newPassword: passwordStrength,
    confirmPassword: z.string(),
  })
  .refine((v) => v.newPassword === v.confirmPassword, {
    path: ['confirmPassword'],
    message: 'Passwords do not match.',
  })
  .refine((v) => v.newPassword !== v.currentPassword, {
    path: ['newPassword'],
    message: 'New password must differ from current.',
  });

export type ChangePasswordFormValues = z.infer<typeof changePasswordSchema>;

export const registerPatientSchema = z
  .object({
    tenantSlug: z
      .string()
      .min(1, 'Please choose a clinic.')
      .max(64)
      .regex(/^[a-z0-9-]+$/i, 'Tenant may only contain letters, digits, and hyphens.'),
    firstName: z.string().min(1, 'First name is required.').max(100),
    lastName: z.string().min(1, 'Last name is required.').max(100),
    email: z.string().min(1, 'Email is required.').email('Enter a valid email.'),
    phone: z
      .string()
      .min(1, 'Phone number is required.')
      .max(30)
      .regex(/^\+?[0-9\s\-()]{7,}$/, 'Enter a valid phone number.'),
    password: passwordStrength,
    confirmPassword: z.string(),
    dateOfBirth: z.string().optional().or(z.literal('')),
    acceptTerms: z.literal(true, { errorMap: () => ({ message: 'You must accept the terms to continue.' }) }),
  })
  .refine((v) => v.password === v.confirmPassword, {
    path: ['confirmPassword'],
    message: 'Passwords do not match.',
  });

export type RegisterPatientFormValues = z.infer<typeof registerPatientSchema>;

export const twoFactorCodeSchema = z.object({
  code: z
    .string()
    .min(6, 'Enter the 6-digit code.')
    .max(8)
    .regex(/^[0-9\s-]+$/, 'Only digits allowed.'),
});
export type TwoFactorCodeFormValues = z.infer<typeof twoFactorCodeSchema>;

export const updateProfileSchema = z.object({
  firstName: z.string().min(1, 'First name is required.').max(100),
  lastName: z.string().min(1, 'Last name is required.').max(100),
  phoneNumber: z.string().max(30).optional().or(z.literal('')),
  avatarUrl: z
    .string()
    .max(500)
    .url('Avatar URL must be a valid URL.')
    .optional()
    .or(z.literal('')),
});

export type UpdateProfileFormValues = z.infer<typeof updateProfileSchema>;
