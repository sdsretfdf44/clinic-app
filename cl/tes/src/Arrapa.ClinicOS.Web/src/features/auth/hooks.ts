import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { useAuthStore } from '@/store/authStore';

import { authApi } from './api';
import type { AuthTokens } from './types';

export const authKeys = {
  currentUser: ['auth', 'me'] as const,
  twoFactor: ['auth', '2fa'] as const,
};

export function useSignIn() {
  const signIn = useAuthStore((s) => s.signIn);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: authApi.login,
    onSuccess: (tokens: AuthTokens) => {
      signIn({
        user: tokens.user,
        accessToken: tokens.accessToken,
        refreshToken: tokens.refreshToken,
      });
      queryClient.setQueryData(authKeys.currentUser, tokens.user);
    },
  });
}

export function useSignOut() {
  const refreshToken = useAuthStore((s) => s.refreshToken);
  const signOut = useAuthStore((s) => s.signOut);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      if (refreshToken) {
        try {
          await authApi.logout({ refreshToken });
        } catch {
          // best-effort — server may already have revoked
        }
      }
    },
    onSettled: () => {
      signOut();
      queryClient.clear();
    },
  });
}

export function useCurrentUser() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const setUser = useAuthStore((s) => s.setUser);

  return useQuery({
    queryKey: authKeys.currentUser,
    queryFn: async () => {
      const user = await authApi.currentUser();
      setUser(user);
      return user;
    },
    enabled: isAuthenticated,
    staleTime: 60_000,
  });
}

export function useForgotPassword() {
  return useMutation({ mutationFn: authApi.forgotPassword });
}

export function useResetPassword() {
  return useMutation({ mutationFn: authApi.resetPassword });
}

export function useChangePassword() {
  return useMutation({ mutationFn: authApi.changePassword });
}

export function useUpdateProfile() {
  const setUser = useAuthStore((s) => s.setUser);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: authApi.updateProfile,
    onSuccess: (user) => {
      setUser(user);
      queryClient.setQueryData(authKeys.currentUser, user);
    },
  });
}

export function useRegisterPatient() {
  return useMutation({ mutationFn: authApi.registerPatient });
}

export function useVerifyEmail() {
  return useMutation({ mutationFn: authApi.verifyEmail });
}

export function useResendVerificationEmail() {
  return useMutation({ mutationFn: authApi.resendVerificationEmail });
}

export function useTwoFactorSetup() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  return useQuery({
    queryKey: authKeys.twoFactor,
    queryFn: authApi.getTwoFactorSetup,
    enabled: isAuthenticated,
    staleTime: 0,
  });
}

export function useEnableTwoFactor() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: authApi.enableTwoFactor,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: authKeys.twoFactor });
    },
  });
}

export function useDisableTwoFactor() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: authApi.disableTwoFactor,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: authKeys.twoFactor });
    },
  });
}
