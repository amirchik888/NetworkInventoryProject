import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Прокси для dev-сервера: запросы /api перенаправляются на backend (http://localhost:5007)
// Это позволяет фронтенду и API работать на одном origin при разработке, упрощая CORS.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5007',
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
