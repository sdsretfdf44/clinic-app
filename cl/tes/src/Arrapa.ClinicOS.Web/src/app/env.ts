interface AppEnv {
  apiBaseUrl: string;
  appName: string;
}

export const env: AppEnv = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? '/api',
  appName: 'Arrapa ClinicOS',
};
