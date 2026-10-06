import path from 'node:path';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
// The Vite dev proxy is intentionally *not* configured. In development the frontend calls the
// backend directly at `VITE_API_BASE_URL` (default `http://localhost:5103/api`); the API's CORS
// policy allows the Vite origin. This mirrors production, where the browser also talks to the API
// origin directly with no proxy layer in between.
export default defineConfig(function (_a) {
    var mode = _a.mode;
    return ({
        plugins: [react()],
        resolve: {
            alias: {
                '@': path.resolve(__dirname, './src'),
            },
        },
        server: {
            port: 5173,
            strictPort: false,
        },
        build: {
            outDir: 'dist',
            sourcemap: mode !== 'production',
            chunkSizeWarningLimit: 1024,
        },
    });
});
