import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';

import type { AuthUser } from '@/features/auth/types';

interface AuthState {
  user: AuthUser | null;
  accessToken: string | null;
  refreshToken: string | null;
  tenantId: string | null;
  isAuthenticated: boolean;
  signIn: (data: { user: AuthUser; accessToken: string; refreshToken: string }) => void;
  signOut: () => void;
  updateTokens: (accessToken: string, refreshToken: string) => void;
  setUser: (user: AuthUser) => void;
}

const initialState = {
  user: null,
  accessToken: null,
  refreshToken: null,
  tenantId: null,
  isAuthenticated: false,
} as const;

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      ...initialState,
      signIn: ({ user, accessToken, refreshToken }) =>
        set({
          user,
          accessToken,
          refreshToken,
          tenantId: user.tenantId,
          isAuthenticated: true,
        }),
      signOut: () => set({ ...initialState }),
      updateTokens: (accessToken, refreshToken) => set({ accessToken, refreshToken }),
      setUser: (user) => set({ user, tenantId: user.tenantId }),
    }),
    {
      name: 'arrapa-clinicos-auth',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        tenantId: state.tenantId,
        isAuthenticated: state.isAuthenticated,
      }),
    },
  ),
);
