import { httpClient } from '@/api/httpClient';
import type { ApiResponse } from '@/types/api';

import type { AuthTokens, AuthUser } from './types';

interface LoginPayload {
  email: string;
  password: string;
  tenantSlug: string;
  twoFactorCode?: string;
}

interface RefreshPayload {
  refreshToken: string;
}

interface LogoutPayload {
  refreshToken: string;
}

interface ForgotPasswordPayload {
  email: string;
  tenantSlug: string;
}

interface ResetPasswordPayload {
  email: string;
  token: string;
  newPassword: string;
  tenantSlug: string;
}

interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
}

interface UpdateProfilePayload {
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
  avatarUrl: string | null;
}

export interface RegisterPatientPayload {
  tenantSlug: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  password: string;
  dateOfBirth?: string;
}

export interface VerifyEmailPayload {
  tenantSlug: string;
  email: string;
  token: string;
}

export interface ResendVerificationPayload {
  tenantSlug: string;
  email: string;
}

export interface TwoFactorSetup {
  sharedKey: string;
  authenticatorUri: string;
}

export interface TwoFactorStatus {
  enabled: boolean;
}

export interface TwoFactorSetupResponse {
  status: TwoFactorStatus;
  setup: TwoFactorSetup | null;
}

function unwrap<T>(response: ApiResponse<T>): T {
  if (!response.success || response.data === null || response.data === undefined) {
    throw new Error(response.error?.message ?? response.message ?? 'Request failed.');
  }
  return response.data;
}

export const authApi = {
  async login(payload: LoginPayload): Promise<AuthTokens> {
    const { data } = await httpClient.post<ApiResponse<AuthTokens>>('/v1/auth/login', payload);
    return unwrap(data);
  },

  async refresh(payload: RefreshPayload): Promise<AuthTokens> {
    const { data } = await httpClient.post<ApiResponse<AuthTokens>>('/v1/auth/refresh', payload);
    return unwrap(data);
  },

  async logout(payload: LogoutPayload): Promise<void> {
    await httpClient.post<ApiResponse<unknown>>('/v1/auth/logout', payload);
  },

  async forgotPassword(payload: ForgotPasswordPayload): Promise<void> {
    await httpClient.post<ApiResponse<unknown>>('/v1/auth/forgot-password', payload);
  },

  async resetPassword(payload: ResetPasswordPayload): Promise<void> {
    await httpClient.post<ApiResponse<unknown>>('/v1/auth/reset-password', payload);
  },

  async changePassword(payload: ChangePasswordPayload): Promise<void> {
    await httpClient.post<ApiResponse<unknown>>('/v1/auth/change-password', payload);
  },

  async currentUser(): Promise<AuthUser> {
    const { data } = await httpClient.get<ApiResponse<AuthUser>>('/v1/auth/me');
    return unwrap(data);
  },

  async updateProfile(payload: UpdateProfilePayload): Promise<AuthUser> {
    const { data } = await httpClient.put<ApiResponse<AuthUser>>('/v1/profile', payload);
    return unwrap(data);
  },

  async registerPatient(payload: RegisterPatientPayload): Promise<void> {
    await httpClient.post<ApiResponse<unknown>>('/v1/auth/register-patient', payload);
  },

  async verifyEmail(payload: VerifyEmailPayload): Promise<void> {
    await httpClient.post<ApiResponse<unknown>>('/v1/auth/verify-email', payload);
  },

  async resendVerificationEmail(payload: ResendVerificationPayload): Promise<void> {
    await httpClient.post<ApiResponse<unknown>>('/v1/auth/resend-verification-email', payload);
  },

  async getTwoFactorSetup(): Promise<TwoFactorSetupResponse> {
    const { data } = await httpClient.get<ApiResponse<TwoFactorSetupResponse>>('/v1/auth/2fa');
    return unwrap(data);
  },

  async enableTwoFactor(verificationCode: string): Promise<void> {
    await httpClient.post<ApiResponse<unknown>>('/v1/auth/2fa/enable', { verificationCode });
  },

  async disableTwoFactor(verificationCode: string): Promise<void> {
    await httpClient.post<ApiResponse<unknown>>('/v1/auth/2fa/disable', { verificationCode });
  },
};
